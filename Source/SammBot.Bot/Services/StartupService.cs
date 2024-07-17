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
using SammBot.Library;
using SammBot.Library.Extensions;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SammBot.Library.Models.Data;
using Color = System.Drawing.Color;

namespace SammBot.Bot.Services;

/// <summary>
/// Handles bot kickstart and shard events.
/// </summary>
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

    /// <summary>
    /// Provides a fast way to convert between Discord.NET severities and Matcha ones.
    /// </summary>
    private readonly Dictionary<Discord.LogSeverity, LogSeverity> _severityDictionary = new Dictionary<Discord.LogSeverity, LogSeverity>()
    {
        [Discord.LogSeverity.Debug] = LogSeverity.Debug,
        [Discord.LogSeverity.Error] = LogSeverity.Error,
        [Discord.LogSeverity.Critical] = LogSeverity.Fatal,
        [Discord.LogSeverity.Warning] = LogSeverity.Warning,
        [Discord.LogSeverity.Info] = LogSeverity.Information,
        [Discord.LogSeverity.Verbose] = LogSeverity.Debug
    };

    /// <summary>
    /// Creates a new <see cref="StartupService"/>.
    /// </summary>
    /// <param name="services">The current active service provider.</param>
    public StartupService(IServiceProvider services)
    {
        _commandService = services.GetRequiredService<CommandService>();
        _shardedClient = services.GetRequiredService<DiscordShardedClient>();
        _interactionService = services.GetRequiredService<InteractionService>();
        _logger = services.GetRequiredService<MatchaLogger>();
        _settingsService = services.GetRequiredService<SettingsService>();
        _informationService = services.GetRequiredService<InformationService>();
    }

    /// <summary>
    /// Initializes the Discord client, prints information to the screen
    /// and thaws the bot database.
    /// </summary>
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

    /// <summary>
    /// Thaws the bot database.
    /// </summary>
    /// <remarks>
    /// This function should be fired and forgotten for startup performance reasons.
    /// </remarks>
    private async Task ThawBotDatabase()
    {
        try
        {
            using (DatabaseService databaseService = new DatabaseService())
            {
                // Hack to force EF to load the database.
                IModel model = databaseService.Model;
            }

            await _logger.LogAsync(LogSeverity.Success, "Database has been thawed successfully!");
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(LogSeverity.Error, "Database could not be thawed: {0}", ex);
        }
    }

    /// <summary>
    /// Raised when a shard gets connected to the gateway.
    /// Logs the event to the screen if on Debug configuration.
    /// </summary>
    /// <param name="shardClient">The shard's Discord client.</param>
    private async Task OnShardConnected(DiscordSocketClient shardClient)
    {
        await _logger.LogAsync(LogSeverity.Debug, "Shard #{0} has connected to the gateway.", shardClient.ShardId);
    }

    /// <summary>
    /// Raised when a shard is ready to receive events.
    /// Sets up the status timer and registers commands.
    /// </summary>
    /// <param name="shardClient">The shard's Discord client.</param>
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

    /// <summary>
    /// Raised when a shard disconnects from the gateway.
    /// Logs the error if it isn't a scheduled or early disconnect.
    /// </summary>
    /// <param name="includedException">The related exception.</param>
    /// <param name="shardClient">The shard's Discord client.</param>
    private async Task OnShardDisconnect(Exception includedException, DiscordSocketClient shardClient)
    {
        // Prevent spamming logs with reconnect requests.
        if (includedException is GatewayReconnectException)
            return;
        if (includedException is WebSocketException { WebSocketErrorCode: WebSocketError.ConnectionClosedPrematurely })
            return;

        await _logger.LogAsync(LogSeverity.Warning, "Shard #{0} has disconnected from the gateway! Reason provided below.\n{1}", shardClient.ShardId, includedException);
    }

    /// <summary>
    /// Raised when <see cref="_statusTimer"/> ticks.
    /// Picks a random status from the settings and sets it.
    /// </summary>
    /// <param name="state">The timer's state.</param>
    private async void RotateStatus(object? state)
    {
        try
        {
            BotStatus chosenStatus = _settingsService.Settings.StatusList.PickRandom();
            ActivityType gameType = chosenStatus.Type;
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

    /// <summary>
    /// Raised when Discord.NET logs a message.
    /// Does conversions for Matcha.
    /// </summary>
    /// <param name="message"></param>
    private async Task DiscordLogAsync(LogMessage message)
    {
        LogSeverity severity = _severityDictionary[message.Severity];

        await _logger.LogAsync(severity, message.Message);
    }
}