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

global using LogSeverity = Matcha.LogSeverity;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.Extensions.DependencyInjection;
using SammBot.Bot.Logging;
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
    private DiscordShardedClient _ShardedClient = default!;
    private InteractionService _InteractionService = default!;
    private BootLogger _BootLogger = default!;

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
        _BootLogger = new BootLogger();

        _BootLogger.Log($"Loading {SettingsManager.CONFIG_FILE}...", LogSeverity.Information);
        if (!SettingsManager.Instance.LoadConfiguration())
        {
            string serializedSettings = JsonSerializer.Serialize(SettingsManager.Instance.LoadedConfig, Constants.JsonSettings);

            _BootLogger.Log($"Could not load {SettingsManager.CONFIG_FILE}. An empty template has been written.", LogSeverity.Fatal);

            await File.WriteAllTextAsync(Path.Combine(Constants.BotDataDirectory, Constants.CONFIG_FILE), serializedSettings);

            PromptExit(1);
        }

        _BootLogger.Log("Loaded configuration successfully.", LogSeverity.Success);

#if DEBUG
        if (SettingsManager.Instance.LoadedConfig.WaitForDebugger && !Debugger.IsAttached)
        {
            _BootLogger.Log("Waiting for debugger to attach...", LogSeverity.Information);

            while (!Debugger.IsAttached)
            {
                Thread.Sleep(100);
            }

            _BootLogger.Log("Debugger has been attached!", LogSeverity.Success);
        }
#endif

        string logsDirectory = Path.Combine(SettingsManager.Instance.BotDataDirectory, "Logs");

        if (!Directory.Exists(logsDirectory))
        {
            _BootLogger.Log("Logs directory did not exist. Creating...", LogSeverity.Warning);

            try
            {
                Directory.CreateDirectory(logsDirectory);
                _BootLogger.Log("Created Logs directory successfully.", LogSeverity.Success);
            }
            catch (Exception ex)
            {
                _BootLogger.Log("Could not create Logs folder. Running the bot without file logging has yet to be implemented.\n" +
                                $"Exception Message: {ex.Message}", LogSeverity.Error);

                PromptExit(1);
            }
        }

        _BootLogger.Log("Creating Discord client...", LogSeverity.Information);

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

        _ShardedClient = new DiscordShardedClient(socketConfig);
        _InteractionService = new InteractionService(_ShardedClient, interactionConfig);

        _BootLogger.Log("Created Discord client successfully.", LogSeverity.Success);
        _BootLogger.Log("Configuring service provider...", LogSeverity.Information);

        ServiceProvider serviceProvider = ConfigureServiceProvider();

        _BootLogger.Log("Configured service provider successfully.", LogSeverity.Success);
        _BootLogger.Log("Starting the startup service...", LogSeverity.Information);
        
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

        serviceCollection.AddSingleton(_ShardedClient)
                         .AddSingleton(_InteractionService)
                         .AddSingleton<HttpService>()
                         .AddSingleton<CommandService>()
                         .AddSingleton<InteractiveService>()
                         .AddSingleton<StartupService>()
                         .AddSingleton<Logger>()
                         .AddSingleton<RandomService>()
                         .AddSingleton<BooruService>()
                         .AddSingleton<EventLoggingService>();

        return serviceCollection.BuildServiceProvider();
    }

    /// <summary>
    /// Prompts the user to press any key to exit.
    /// </summary>
    /// <param name="exitCode">The exit code to use.</param>
    [DoesNotReturn]
    private void PromptExit(int exitCode)
    {
        if (Console.IsInputRedirected)
        {
            _BootLogger.Log("Press any key to exit...", LogSeverity.Information);
            Console.ReadKey();
        }

        Environment.Exit(exitCode);
    }
}