﻿#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021 Analog Feelings
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
using Microsoft.Extensions.DependencyInjection;
using SammBot.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SammBot.Library.Models;
using SammBot.Library.Models.Data;
using SammBot.Library.Services;
using SammBot.Services;

namespace SammBot;

/// <summary>
/// A class containing all of the startup logic.
/// </summary>
public class EntryPoint
{
    private DiscordShardedClient? _shardedClient;
    private InteractionService? _interactionService;
    private MatchaLogger? _matchaLogger;
    private SettingsService? _settingsService;
    private InformationService? _informationService;
    private PluginService? _pluginService;

    public static async Task Main()
    {
        EntryPoint entryPoint = new EntryPoint();
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

        await entryPoint.MainAsync();
    }

    /// <summary>
    /// The actual entry point for the application.
    /// </summary>
    private async Task MainAsync()
    {
        _informationService = new InformationService();
        _informationService.Uptime.Start();

        Console.WriteLine("Initializing logger...");

        _matchaLogger = InitializeLogger();
        _settingsService = new SettingsService(_matchaLogger);

        BotConfig? config = _settingsService.GetSettings<BotConfig>();
        if (config == null)
        {
            await _matchaLogger.LogAsync(LogSeverity.Fatal, $"Could not load main bot settings. Please fill the template file.");

            PromptExit(1);
        }

        _pluginService = new PluginService(_settingsService, _matchaLogger);
        
        _pluginService.LoadPlugins();

#if DEBUG
        if (config.WaitForDebugger && !Debugger.IsAttached)
        {
            await _matchaLogger.LogAsync(LogSeverity.Information, "Waiting for debugger to attach...");

            while (!Debugger.IsAttached)
            {
                Thread.Sleep(100);
            }

            await _matchaLogger.LogAsync(LogSeverity.Success, "Debugger has been attached!");
        }
#endif

        await _matchaLogger.LogAsync(LogSeverity.Information, "Creating Discord client...");

        DiscordSocketConfig socketConfig = new DiscordSocketConfig()
        {
            LogLevel = Discord.LogSeverity.Warning,
            MessageCacheSize = config.MessageCacheSize,
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

        StartupService startupService = serviceProvider.GetRequiredService<StartupService>();

        await startupService.StartAsync();

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
        
        foreach(KeyValuePair<IPlugin, Assembly> plugin in _pluginService!.Plugins)
            plugin.Key.Initialize(serviceCollection);

        // The previous objects are always initialized by this point,
        // so use a null-forgiving operator to shut the compiler up.
        serviceCollection.AddSingleton(_shardedClient!);
        serviceCollection.AddSingleton(_interactionService!);
        serviceCollection.AddSingleton(_matchaLogger!);
        serviceCollection.AddSingleton(_settingsService!);
        serviceCollection.AddSingleton(_informationService!);
        serviceCollection.AddSingleton(_pluginService!);
        serviceCollection.AddSingleton<HttpService>();
        serviceCollection.AddSingleton<ICommandService, CommandService>();
        serviceCollection.AddSingleton<StartupService>();
        serviceCollection.AddSingleton<EventLoggingService>();
        
        serviceCollection.AddScoped<DatabaseService>();

        return serviceCollection.BuildServiceProvider();
    }

    /// <summary>
    /// Initializes a new logger and its sinks.
    /// </summary>
    private MatchaLogger InitializeLogger()
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
            FilePath = Path.Combine(Constants.BotDataDirectory, "Logs")
        };

        ConsoleSink consoleSink = new ConsoleSink()
        {
            Config = consoleConfig
        };
        FileSink fileSink = new FileSink()
        {
            Config = fileConfig
        };

        return new MatchaLogger(consoleSink, fileSink);
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