﻿#region License Information (GPLv3)
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

global using LogSeverity = AnalogFeelings.Matcha.Enums.LogSeverity;
using AnalogFeelings.Matcha;
using AnalogFeelings.Matcha.Sinks.Console;
using AnalogFeelings.Matcha.Sinks.File;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.Extensions.DependencyInjection;
using SammBot.Bot.Services;
using SammBot.Bot.Settings;
using SammBot.Library;
using SammBot.Library.Helpers;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SammBot.Bot;

/// <summary>
/// A class containing all of the startup logic.
/// </summary>
public class EntryPoint
{
    private DiscordShardedClient _shardedClient = default!;
    private InteractionService _interactionService = default!;
    private MatchaLogger _matchaLogger = default!;

    public static void Main()
    {
        Constants.RuntimeStopwatch.Start();

        EntryPoint entryPoint = new EntryPoint();
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

        AsyncHelper.RunSync(() => entryPoint.MainAsync());
    }

    /// <summary>
    /// The actual entry point for the application.
    /// </summary>
    private async Task MainAsync()
    {
        if (!SettingsManager.Instance.LoadConfiguration())
        {
            string serializedSettings = JsonSerializer.Serialize(SettingsManager.Instance.LoadedConfig, Constants.JsonSettings);

            Console.WriteLine($"Could not load {SettingsManager.CONFIG_FILE}. An empty template has been written.");

            await File.WriteAllTextAsync(Path.Combine(Constants.BotDataDirectory, Constants.CONFIG_FILE), serializedSettings);

            PromptExit(1);
        }

        Console.WriteLine("Initializing logger...");
        
        InitializeLogger();

#if DEBUG
        if (SettingsManager.Instance.LoadedConfig.WaitForDebugger && !Debugger.IsAttached)
        {
            await _matchaLogger.LogAsync(LogSeverity.Information, "Waiting for debugger to attach...");

            while (!Debugger.IsAttached)
            {
                Thread.Sleep(100);
            }

            await _matchaLogger.LogAsync(LogSeverity.Success, "Debugger has been attached!");
        }
#endif

        string logsDirectory = Path.Combine(SettingsManager.Instance.BotDataDirectory, "Logs");

        if (!Directory.Exists(logsDirectory))
        {
            await _matchaLogger.LogAsync(LogSeverity.Warning, "Logs directory did not exist. Creating...");

            try
            {
                Directory.CreateDirectory(logsDirectory);
                
                await _matchaLogger.LogAsync(LogSeverity.Success, "Created Logs directory successfully.");
            }
            catch (Exception)
            {
                // Ignored
            }
        }

        await _matchaLogger.LogAsync(LogSeverity.Information, "Creating Discord client...");

        DiscordSocketConfig socketConfig = new DiscordSocketConfig()
        {
                LogLevel = Discord.LogSeverity.Warning,
                MessageCacheSize = SettingsManager.Instance.LoadedConfig.MessageCacheSize,
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.All,
                LogGatewayIntentWarnings = false
        };
        InteractionServiceConfig interactionConfig = new InteractionServiceConfig()
        {
                LogLevel = Discord.LogSeverity.Info,
                DefaultRunMode = RunMode.Async,
        };

        _shardedClient = new DiscordShardedClient(socketConfig);
        _interactionService = new InteractionService(_shardedClient, interactionConfig);

        await _matchaLogger.LogAsync(LogSeverity.Success, "Created Discord client successfully.");
        await _matchaLogger.LogAsync(LogSeverity.Information, "Configuring service provider...");
        
        ServiceProvider serviceProvider = ConfigureServiceProvider();

        await _matchaLogger.LogAsync(LogSeverity.Success, "Configured service provider successfully.");
        await _matchaLogger.LogAsync(LogSeverity.Information, "Starting the startup service...");
        
        await serviceProvider.GetRequiredService<StartupService>().StartAsync();

        // Never exit unless a critical exception occurs.
        await Task.Delay(-1);
    }

    /// <summary>
    /// Configures a new <see cref="ServiceProvider"/> for use.
    /// </summary>
    /// <returns>A new service provider.</returns>
    private ServiceProvider ConfigureServiceProvider()
    {
        ServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton(_shardedClient)
                         .AddSingleton(_interactionService)
                         .AddSingleton(_matchaLogger)
                         .AddSingleton<HttpService>()
                         .AddSingleton<CommandService>()
                         .AddSingleton<InteractiveService>()
                         .AddSingleton<StartupService>()
                         .AddSingleton<RandomService>()
                         .AddSingleton<BooruService>()
                         .AddSingleton<EventLoggingService>();

        return serviceCollection.BuildServiceProvider();
    }

    /// <summary>
    /// Initializes the logger and its sinks.
    /// </summary>
    private void InitializeLogger()
    {
        LogSeverity filterLevel;
        
#if DEBUG
        filterLevel = LogSeverity.Debug;
#else
        filterLevel = LogSeverity.Information;
#endif
        
        ConsoleSinkConfig consoleConfig = new ConsoleSinkConfig()
        {
                SeverityFilterLevel = filterLevel
        };
        FileSinkConfig fileConfig = new FileSinkConfig()
        {
                SeverityFilterLevel = filterLevel,
                FilePath = Path.Combine(SettingsManager.Instance.BotDataDirectory, "Logs")
        };

        ConsoleSink consoleSink = new ConsoleSink()
        {
                Config = consoleConfig
        };
        FileSink fileSink = new FileSink()
        {
                Config = fileConfig
        };

        _matchaLogger = new MatchaLogger(consoleSink, fileSink);
    }

    /// <summary>
    /// Prompts the user to press any key to exit.
    /// </summary>
    /// <param name="exitCode">The exit code to use.</param>
    [DoesNotReturn]
    private void PromptExit(int exitCode)
    {
        if (!Console.IsInputRedirected)
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        Environment.Exit(exitCode);
    }
}