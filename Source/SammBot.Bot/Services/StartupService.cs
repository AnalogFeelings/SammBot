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
using SammBot.Bot.Settings;
using SammBot.Library;
using SammBot.Library.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Color = System.Drawing.Color;

namespace SammBot.Bot.Services;

public class StartupService
{
    private CommandService CommandService { get; }
    private DiscordShardedClient ShardedClient { get; }
    private InteractionService InteractionService { get; }
    private MatchaLogger Logger { get; }

    [UsedImplicitly]
    private Timer? _StatusTimer;

    private bool _EventsSetUp;
    private int _ShardsReady;

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
        CommandService = provider.GetRequiredService<CommandService>();
        ShardedClient = provider.GetRequiredService<DiscordShardedClient>();
        InteractionService = provider.GetRequiredService<InteractionService>();
        Logger = provider.GetRequiredService<MatchaLogger>();
    }

    public async Task StartAsync()
    {
        await Logger.LogAsync(LogSeverity.Information, "Logging in as a bot...");
        await ShardedClient.LoginAsync(TokenType.Bot, SettingsManager.Instance.LoadedConfig.BotToken);
        await ShardedClient.StartAsync();
        await Logger.LogAsync(LogSeverity.Success, "Succesfully connected to web socket.");

        ShardedClient.ShardConnected += OnShardConnected;
        ShardedClient.ShardReady += OnShardReady;
        ShardedClient.ShardDisconnected += OnShardDisconnect;

        ShardedClient.Log += DiscordLogAsync;
        InteractionService.Log += DiscordLogAsync;

        Constants.RuntimeStopwatch.Stop();

        Console.Title = $"{SettingsManager.BOT_NAME} v{SettingsManager.GetBotVersion()}";

        string discordNetVersion = Assembly.GetAssembly(typeof(SessionStartLimit))!.GetName().Version!.ToString(3);
        string matchaVersion = Assembly.GetAssembly(typeof(MatchaLogger))!.GetName().Version!.ToString(3);

        Console.Clear();

        Console.Write(FiggleFonts.Slant.Render(SettingsManager.BOT_NAME).Pastel(Color.SkyBlue));
        Console.Write("===========".Pastel(Color.CadetBlue));
        Console.Write($"Source code v{SettingsManager.GetBotVersion()}, Discord.NET {discordNetVersion}".Pastel(Color.LightCyan));
        Console.WriteLine("===========".Pastel(Color.CadetBlue));
        Console.WriteLine();

        await Logger.LogAsync(LogSeverity.Information, "Using Matcha {0}.", matchaVersion);

        await Logger.LogAsync(LogSeverity.Information, "{0} took {1}ms to boot.", SettingsManager.BOT_NAME, Constants.RuntimeStopwatch.ElapsedMilliseconds);

        Constants.RuntimeStopwatch.Restart();

#if DEBUG
        await Logger.LogAsync(LogSeverity.Warning, "{0} has been built on Debug configuration. Extra logging will be available.", SettingsManager.BOT_NAME);
#endif

        if (SettingsManager.Instance.LoadedConfig.OnlyOwnerMode)
            await Logger.LogAsync(LogSeverity.Warning, "Only Owner Mode is active. {0} will only handle commands sent by the bot account owner.", SettingsManager.BOT_NAME);

        await Logger.LogAsync(LogSeverity.Information, "Thawing the bot database...");
        _ = Task.Run(() => ThawBotDatabase());

        await Logger.LogAsync(LogSeverity.Information, "Initializing command handler...");
        await CommandService.InitializeHandlerAsync();
        await Logger.LogAsync(LogSeverity.Success, "Succesfully initialized command handler.");
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

            await Logger.LogAsync(LogSeverity.Success, "Database has been thawed successfully!");
        }
        catch (Exception ex)
        {
            await Logger.LogAsync(LogSeverity.Error, "Database could not be thawed: {0}", ex);
        }
    }

    private async Task OnShardConnected(DiscordSocketClient shardClient)
    {
        await Logger.LogAsync(LogSeverity.Debug, "Shard #{0} has connected to the gateway.", shardClient.ShardId);
    }

    private async Task OnShardReady(DiscordSocketClient shardClient)
    {
        await Logger.LogAsync(LogSeverity.Debug, "Shard #{0} is ready to run.", shardClient.ShardId);

        if (!_EventsSetUp)
        {
            _ShardsReady++;

            if (_ShardsReady == ShardedClient.Shards.Count)
            {
                if (SettingsManager.Instance.LoadedConfig.StatusList.Count > 0 && SettingsManager.Instance.LoadedConfig.RotatingStatus)
                    _StatusTimer = new Timer(RotateStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(20));

                await InteractionService.RegisterCommandsGloballyAsync();

                await Logger.LogAsync(LogSeverity.Success, "{0} is ready to run.", SettingsManager.BOT_NAME);

                _EventsSetUp = true;
            }
        }
    }

    private async Task OnShardDisconnect(Exception includedException, DiscordSocketClient shardClient)
    {
        await Logger.LogAsync(LogSeverity.Warning, "Shard #{0} has disconnected from the gateway! Reason: {1}", shardClient.ShardId, includedException.Message);
    }

    private async void RotateStatus(object? state)
    {
        try
        {
            BotStatus chosenStatus = SettingsManager.Instance.LoadedConfig.StatusList.PickRandom();
            ActivityType gameType = (ActivityType)chosenStatus.Type;
            string? gameUrl = gameType == ActivityType.Streaming ? SettingsManager.Instance.LoadedConfig.TwitchUrl : null;

            if (gameType == ActivityType.CustomStatus)
                await ShardedClient.SetCustomStatusAsync(chosenStatus.Content);
            else
                await ShardedClient.SetGameAsync(chosenStatus.Content, gameUrl, gameType);
        }
        catch (Exception ex)
        {
            await Logger.LogAsync(LogSeverity.Error, "An exception has ocurred when rotating the status: {0}", ex);
        }
    }

    private async Task DiscordLogAsync(LogMessage message)
    {
        LogSeverity severity = _severityDictionary[message.Severity];

        await Logger.LogAsync(severity, message.Message);
    }
}