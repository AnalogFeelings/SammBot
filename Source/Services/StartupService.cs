using Discord;
using Discord.Commands;
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
using Color = System.Drawing.Color;

namespace SammBotNET.Services
{
    public class StartupService
    {
        private IServiceProvider ServiceProvider;
        private DiscordSocketClient SocketClient { get; set; }
        private CommandService CommandsService { get; set; }
        private Logger BotLogger { get; set; }

        private Timer StatusTimer;
        private Timer AvatarTimer;

        private AutoDequeueList<string> RecentAvatars;

        private bool FirstTimeConnection = true;

        public StartupService(IServiceProvider provider, DiscordSocketClient client, CommandService command, Logger logger)
        {
            ServiceProvider = provider;
            SocketClient = client;
            CommandsService = command;
            BotLogger = logger;

            RecentAvatars = new AutoDequeueList<string>(Settings.Instance.LoadedConfig.AvatarRecentQueueSize);
        }

        public async Task StartAsync()
        {
            BotLogger.Log("Logging in as a bot...", LogSeverity.Information);
            await SocketClient.LoginAsync(TokenType.Bot, Settings.Instance.LoadedConfig.BotToken);
            await SocketClient.StartAsync();
            BotLogger.Log("Succesfully connected to web socket.", LogSeverity.Success);

            SocketClient.Connected += OnConnected;
            SocketClient.Ready += OnReady;
            SocketClient.Disconnected += OnDisconnect;
            Console.Clear();

            await CommandsService.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceProvider);
            Settings.Instance.StartupStopwatch.Stop();

            Console.Title = $"{Settings.BOT_NAME} {Settings.Instance.LoadedConfig.BotVersion}";

            string DiscordNetVersion = Assembly.GetAssembly(typeof(SessionStartLimit)).GetName().Version.ToString(3);
            string MatchaVersion = Assembly.GetAssembly(typeof(MatchaLogger)).GetName().Version.ToString(3);

            Console.Write(FiggleFonts.Slant.Render(Settings.BOT_NAME).Pastel(Color.SkyBlue));
            Console.Write("===========".Pastel(Color.CadetBlue));
            Console.Write($"Source code {Settings.Instance.LoadedConfig.BotVersion}, Discord.NET {DiscordNetVersion}".Pastel(Color.LightCyan));
            Console.WriteLine("===========".Pastel(Color.CadetBlue));
            Console.WriteLine();

            Settings.Instance.RuntimeStopwatch.Start();

            BotLogger.Log($"Using MatchaLogger {MatchaVersion}.", LogSeverity.Information);

            BotLogger.Log($"{Settings.BOT_NAME} took" +
                $" {Settings.Instance.StartupStopwatch.ElapsedMilliseconds}ms to boot.", LogSeverity.Information);
        }

        private Task OnConnected()
        {
            if (FirstTimeConnection)
            {
                BotLogger.Log("Connected to gateway.", LogSeverity.Success);
                FirstTimeConnection = false;
            }
            else BotLogger.Log("Reconnected to gateway.", LogSeverity.Success);

            return Task.CompletedTask;
        }

        private Task OnReady()
        {
            if (Settings.Instance.LoadedConfig.StatusList.Count > 0 && Settings.Instance.LoadedConfig.RotatingStatus)
                StatusTimer = new Timer(RotateStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(20));

            if (Settings.Instance.LoadedConfig.RotatingAvatar)
            {
                TimeSpan AvatarDelay = TimeSpan.FromHours(Settings.Instance.LoadedConfig.AvatarRotationTime);

                AvatarTimer = new Timer(RotateAvatar, null, AvatarDelay, AvatarDelay);
            }

            BotLogger.Log($"{Settings.BOT_NAME} is ready to run.", LogSeverity.Success);

            return Task.CompletedTask;
        }

        private Task OnDisconnect(Exception IncludedException)
        {
            BotLogger.Log("Client has disconnected from the gateway! Reason: " + IncludedException.Message, LogSeverity.Warning);

            return Task.CompletedTask;
        }

        private async void RotateStatus(object State)
        {
            BotStatus ChosenStatus = Settings.Instance.LoadedConfig.StatusList.PickRandom();

            string GameUrl = ChosenStatus.Type == 1 ? Settings.Instance.LoadedConfig.TwitchUrl : null;

            await SocketClient.SetGameAsync(ChosenStatus.Content, GameUrl, (ActivityType)ChosenStatus.Type);
        }

        private async void RotateAvatar(object State)
        {
            List<string> AvatarList = Directory.EnumerateFiles(Path.Combine(Settings.Instance.BotDataDirectory, "Avatars")).ToList();
            if (AvatarList.Count < 2) return;

            List<string> FilteredList = AvatarList.Except(RecentAvatars).ToList();

            string ChosenAvatar = FilteredList.PickRandom();
            BotLogger.Log($"Setting bot avatar to \"{Path.GetFileName(ChosenAvatar)}\".", LogSeverity.Debug);

            using (FileStream AvatarStream = new(ChosenAvatar, FileMode.Open))
            {
                Image LoadedAvatar = new Image(AvatarStream);

                await SocketClient.CurrentUser.ModifyAsync(x => x.Avatar = LoadedAvatar);
            }

            RecentAvatars.Push(ChosenAvatar);
        }
    }
}
