#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021-2024 Analog Feelings
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using AnalogFeelings.Matcha;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Figgle;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Pastel;
using SammBot.Bot.Database;
using SammBot.Library;
using SammBot.Library.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SammBot.Library.Models.Data;
using Color = System.Drawing.Color;

namespace SammBot.Bot.Services;

public class StartupService
{
    private readonly CommandService _commandService;
    private readonly DiscordShardedClient _shardedClient;
    private readonly InteractionService _interactionService;
    private readonly MatchaLogger _logger;
    private readonly SettingsService _settingsService;
    private readonly InformationService _informationService;

    [UsedImplicitly]
    private Timer? _statusTimer;

    private bool _eventsSetUp;
    private int _shardsReady;

    private readonly Dictionary<Discord.LogSeverity, LogSeverity> _severityDictionary = new Dictionary<Discord.LogSeverity, LogSeverity>()
    {
        [Discord.LogSeverity.Debug] = LogSeverity.Debug,
        [Discord.LogSeverity.Error] = LogSeverity.Error,
        [Discord.LogSeverity.Critical] = LogSeverity.Fatal,
        [Discord.LogSeverity.Warning] = LogSeverity.Warning,
        [Discord.LogSeverity.Info] = LogSeverity.Information,
        [Discord.LogSeverity.Verbose] = LogSeverity.Debug
    };

    public StartupService(IServiceProvider provider)
    {
        _commandService = provider.GetRequiredService<CommandService>();
        _shardedClient = provider.GetRequiredService<DiscordShardedClient>();
        _interactionService = provider.GetRequiredService<InteractionService>();
        _logger = provider.GetRequiredService<MatchaLogger>();
        _settingsService = provider.GetRequiredService<SettingsService>();
        _informationService = provider.GetRequiredService<InformationService>();
    }

    public async Task StartAsync()
    {
        await _logger.LogAsync(LogSeverity.Information, "Logging in as a bot...");
        await _shardedClient.LoginAsync(TokenType.Bot, _settingsService.Settings.BotToken);
        await _shardedClient.StartAsync();
        await _logger.LogAsync(LogSeverity.Success, "Succesfully connected to web socket.");

        _shardedClient.ShardConnected += OnShardConnected;
        _shardedClient.ShardReady += OnShardReady;
        _shardedClient.ShardDisconnected += OnShardDisconnect;

        _shardedClient.Log += DiscordLogAsync;
        _interactionService.Log += DiscordLogAsync;

        Constants.RuntimeStopwatch.Stop();

        string botVersion = _informationService.Version;

        Console.Title = $"{Constants.BOT_NAME} v{botVersion}";

        string discordNetVersion = Assembly.GetAssembly(typeof(SessionStartLimit))!.GetName().Version!.ToString(3);
        string matchaVersion = Assembly.GetAssembly(typeof(MatchaLogger))!.GetName().Version!.ToString(3);

        Console.Clear();

        Console.Write(FiggleFonts.Slant.Render(Constants.BOT_NAME).Pastel(Color.SkyBlue));
        Console.Write("===========".Pastel(Color.CadetBlue));
        Console.Write($"Source code v{botVersion}, Discord.NET {discordNetVersion}".Pastel(Color.LightCyan));
        Console.WriteLine("===========".Pastel(Color.CadetBlue));
        Console.WriteLine();

        await _logger.LogAsync(LogSeverity.Information, "Using Matcha {0}.", matchaVersion);

        await _logger.LogAsync(LogSeverity.Information, "{0} took {1}ms to boot.", Constants.BOT_NAME, Constants.RuntimeStopwatch.ElapsedMilliseconds);

        Constants.RuntimeStopwatch.Restart();

#if DEBUG
        await _logger.LogAsync(LogSeverity.Warning, "{0} has been built on Debug configuration. Extra logging will be available.", Constants.BOT_NAME);
#endif

        if (_settingsService.Settings.OnlyOwnerMode)
            await _logger.LogAsync(LogSeverity.Warning, "Only Owner Mode is active. {0} will only handle commands sent by the bot account owner.", Constants.BOT_NAME);

        await _logger.LogAsync(LogSeverity.Information, "Thawing the bot database...");
        _ = Task.Run(() => ThawBotDatabase());

        await _logger.LogAsync(LogSeverity.Information, "Initializing command handler...");
        await _commandService.InitializeHandlerAsync();
        await _logger.LogAsync(LogSeverity.Success, "Succesfully initialized command handler.");
    }

    private async Task ThawBotDatabase()
    {
        try
        {
            using (BotDatabase botDatabase = new BotDatabase())
            {
                // Hack to force EF to load the database.
                IModel model = botDatabase.Model;
            }

            await _logger.LogAsync(LogSeverity.Success, "Database has been thawed successfully!");
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(LogSeverity.Error, "Database could not be thawed: {0}", ex);
        }
    }

    private async Task OnShardConnected(DiscordSocketClient shardClient)
    {
        await _logger.LogAsync(LogSeverity.Debug, "Shard #{0} has connected to the gateway.", shardClient.ShardId);
    }

    private async Task OnShardReady(DiscordSocketClient shardClient)
    {
        await _logger.LogAsync(LogSeverity.Debug, "Shard #{0} is ready to run.", shardClient.ShardId);

        if (!_eventsSetUp)
        {
            _shardsReady++;

            if (_shardsReady == _shardedClient.Shards.Count)
            {
                if (_settingsService.Settings.StatusList.Count > 0 && _settingsService.Settings.RotatingStatus)
                    _statusTimer = new Timer(RotateStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(20));

                await _interactionService.RegisterCommandsGloballyAsync();

                await _logger.LogAsync(LogSeverity.Success, "{0} is ready to run.", Constants.BOT_NAME);

                _eventsSetUp = true;
            }
        }
    }

    private async Task OnShardDisconnect(Exception includedException, DiscordSocketClient shardClient)
    {
        await _logger.LogAsync(LogSeverity.Warning, "Shard #{0} has disconnected from the gateway! Reason: {1}", shardClient.ShardId, includedException);
    }

    private async void RotateStatus(object? state)
    {
        try
        {
            BotStatus chosenStatus = _settingsService.Settings.StatusList.PickRandom();
            ActivityType gameType = (ActivityType)chosenStatus.Type;
            string? gameUrl = gameType == ActivityType.Streaming ? _settingsService.Settings.TwitchUrl : null;

            if (gameType == ActivityType.CustomStatus)
                await _shardedClient.SetCustomStatusAsync(chosenStatus.Content);
            else
                await _shardedClient.SetGameAsync(chosenStatus.Content, gameUrl, gameType);
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(LogSeverity.Error, "An exception has ocurred when rotating the status: {0}", ex);
        }
    }

    private async Task DiscordLogAsync(LogMessage message)
    {
        LogSeverity severity = _severityDictionary[message.Severity];

        await _logger.LogAsync(severity, message.Message);
    }
}