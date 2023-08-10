#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2023 Analog Feelings
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
#endregion

using Discord;
using Discord.WebSocket;
using Figgle;
using Matcha;
using Pastel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord.Interactions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using SammBot.Bot.Core;
using SammBot.Bot.Database;
using SammBot.Library;
using SammBot.Library.Components;
using SammBot.Library.Extensions;
using Color = System.Drawing.Color;

namespace SammBot.Bot.Services;

public class StartupService
{
    private IServiceProvider ServiceProvider { get; set; }
    private DiscordShardedClient ShardedClient { get; set; }
    private InteractionService InteractionService { get; set; }
    private Logger BotLogger { get; set; }
    
    [UsedImplicitly]
    private Timer? _StatusTimer;
    [UsedImplicitly]
    private Timer? _AvatarTimer;

    private AutoDequeueList<string> RecentAvatars { get; set; }

    private bool _EventsSetUp;
    private int _ShardsReady;
        
    public StartupService(IServiceProvider ServiceProvider, DiscordShardedClient ShardedClient, InteractionService InteractionService, Logger Logger)
    {
        this.ServiceProvider = ServiceProvider;
        this.ShardedClient = ShardedClient;
        this.InteractionService = InteractionService;
        BotLogger = Logger;

        RecentAvatars = new AutoDequeueList<string>(SettingsManager.Instance.LoadedConfig.AvatarRecentQueueSize);
    }

    public async Task StartAsync()
    {
        BotLogger.Log("Logging in as a bot...", LogSeverity.Information);
        await ShardedClient.LoginAsync(TokenType.Bot, SettingsManager.Instance.LoadedConfig.BotToken);
        await ShardedClient.StartAsync();
        BotLogger.Log("Succesfully connected to web socket.", LogSeverity.Success);

        ShardedClient.ShardConnected += OnShardConnected;
        ShardedClient.ShardReady += OnShardReady;
        ShardedClient.ShardDisconnected += OnShardDisconnect;
            
        BotGlobals.Instance.RuntimeStopwatch.Stop();

        Console.Title = $"{SettingsManager.BOT_NAME} v{SettingsManager.GetBotVersion()}";

        string discordNetVersion = Assembly.GetAssembly(typeof(SessionStartLimit))!.GetName().Version!.ToString(3);
        string matchaVersion = Assembly.GetAssembly(typeof(MatchaLogger))!.GetName().Version!.ToString(3);
            
        Console.Clear();

        Console.Write(FiggleFonts.Slant.Render(SettingsManager.BOT_NAME).Pastel(Color.SkyBlue));
        Console.Write("===========".Pastel(Color.CadetBlue));
        Console.Write($"Source code v{SettingsManager.GetBotVersion()}, Discord.NET {discordNetVersion}".Pastel(Color.LightCyan));
        Console.WriteLine("===========".Pastel(Color.CadetBlue));
        Console.WriteLine();
        
        BotLogger.Log($"Using MatchaLogger {matchaVersion}.", LogSeverity.Information);

        BotLogger.Log($"{SettingsManager.BOT_NAME} took" +
                      $" {BotGlobals.Instance.RuntimeStopwatch.ElapsedMilliseconds}ms to boot.", LogSeverity.Information);

        BotGlobals.Instance.RuntimeStopwatch.Restart();
            
#if DEBUG
        BotLogger.Log($"{SettingsManager.BOT_NAME} has been built on Debug configuration. Extra logging will be available.", LogSeverity.Warning);
#endif
            
        if(SettingsManager.Instance.LoadedConfig.OnlyOwnerMode)
            BotLogger.Log($"Only Owner Mode is active. {SettingsManager.BOT_NAME} will only handle commands sent by the bot account owner.", LogSeverity.Warning);
            
        BotLogger.Log("Warming up the bot database...", LogSeverity.Information);
        _ = Task.Run(() => WarmUpDatabase());

        BotLogger.Log("Initializing command handler...", LogSeverity.Information);
        await ServiceProvider.GetRequiredService<CommandHandler>().InitializeHandlerAsync();
        BotLogger.Log("Succesfully initialized command handler.", LogSeverity.Success);
    }

    private Task WarmUpDatabase()
    {
        try
        {
            using (BotDatabase botDatabase = new BotDatabase())
            {
                // Hack to force EF to load the database.
                IModel model = botDatabase.Model;
            }
            
            BotLogger.Log("Database has been warmed up!", LogSeverity.Success);
        }
        catch (Exception ex)
        {
            BotLogger.Log($"Database could not be warmed up: {ex.Message}", LogSeverity.Error);
        }
            
        return Task.CompletedTask;
    }

    private Task OnShardConnected(DiscordSocketClient ShardClient)
    {
        BotLogger.Log($"Shard #{ShardClient.ShardId} has connected to the gateway.", LogSeverity.Debug);

        return Task.CompletedTask;
    }

    private async Task OnShardReady(DiscordSocketClient ShardClient)
    {
        BotLogger.Log($"Shard #{ShardClient.ShardId} is ready to run.", LogSeverity.Debug);
            
        if (!_EventsSetUp)
        {
            _ShardsReady++;
                
            if (_ShardsReady == ShardedClient.Shards.Count)
            {
                if (SettingsManager.Instance.LoadedConfig.StatusList.Count > 0 && SettingsManager.Instance.LoadedConfig.RotatingStatus)
                    _StatusTimer = new Timer(RotateStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(20));

                if (SettingsManager.Instance.LoadedConfig.RotatingAvatar)
                {
                    TimeSpan avatarDelay = TimeSpan.FromHours(SettingsManager.Instance.LoadedConfig.AvatarRotationTime);

                    _AvatarTimer = new Timer(RotateAvatar, null, avatarDelay, avatarDelay);
                }

                await InteractionService.RegisterCommandsGloballyAsync();

                BotLogger.Log($"{SettingsManager.BOT_NAME} is ready to run.", LogSeverity.Success);

                _EventsSetUp = true;
            }
        }
    }

    private Task OnShardDisconnect(Exception IncludedException, DiscordSocketClient ShardClient)
    {
        BotLogger.Log($"Shard #{ShardClient.ShardId} has disconnected from the gateway! Reason: " + IncludedException.Message, LogSeverity.Warning);

        return Task.CompletedTask;
    }

    private async void RotateStatus(object? State)
    {
        try
        {
            BotStatus chosenStatus = SettingsManager.Instance.LoadedConfig.StatusList.PickRandom();
            string gameUrl = SettingsManager.Instance.LoadedConfig.TwitchUrl;
            ActivityType gameType = (ActivityType)chosenStatus.Type;

            if (gameType == ActivityType.CustomStatus)
                await ShardedClient.SetCustomStatusAsync(chosenStatus.Content);
            else
                await ShardedClient.SetGameAsync(chosenStatus.Content, gameUrl, gameType);
        }
        catch (Exception ex)
        {
            BotLogger.Log($"An exception has ocurred when rotating the status: {ex}", LogSeverity.Error);
        }
    }

    private async void RotateAvatar(object? State)
    {
        try
        {
            List<string> avatarList = Directory.EnumerateFiles(Path.Combine(SettingsManager.Instance.BotDataDirectory, "Avatars")).ToList();
            if (avatarList.Count < 2) return;

            List<string> filteredList = avatarList.Except(RecentAvatars).ToList();

            string chosenAvatar = filteredList.PickRandom();
            BotLogger.Log($"Setting bot avatar to \"{Path.GetFileName(chosenAvatar)}\".", LogSeverity.Debug);

            using (FileStream avatarStream = new FileStream(chosenAvatar, FileMode.Open))
            {
                Image loadedAvatar = new Image(avatarStream);

                await ShardedClient.CurrentUser.ModifyAsync(x => x.Avatar = loadedAvatar);
            }

            RecentAvatars.Push(chosenAvatar);
        }
        catch (Exception ex)
        {
            BotLogger.Log($"An exception has ocurred when rotating the avatar: {ex}", LogSeverity.Error);
        }
    }
}