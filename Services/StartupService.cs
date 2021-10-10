using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Figgle;
using Pastel;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Color = System.Drawing.Color;

namespace SammBotNET.Services
{
    public class StartupService
    {
        public readonly IServiceProvider ServiceProvider;
        public readonly DiscordSocketClient SocketClient;
        public readonly CommandService CommandsService;
        public readonly Logger BotLogger;

        public Timer StatusTimer;
        public Random Rand = new Random();

        private bool UseRotatingStatus = true;

        public StartupService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands,
                                Logger logger)
        {
            ServiceProvider = provider;
            SocketClient = discord;
            CommandsService = commands;
            BotLogger = logger;
        }

        public Task Ready()
        {
            if (UseRotatingStatus && GlobalConfig.Instance.LoadedConfig.RotatingStatus)
            {
                StatusTimer = new Timer(async _ =>
                {
                    BotStatus status = GlobalConfig.Instance.StatusList[Rand.Next(GlobalConfig.Instance.StatusList.Count)];
                    await SocketClient.SetGameAsync(status.Content,
                        status.Type == 1 ? GlobalConfig.Instance.LoadedConfig.TwitchUrl : null, (ActivityType)status.Type);
                }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(25));
            }

            return Task.CompletedTask;
        }

        public async Task StartAsync()
        {
            Console.WriteLine($"Loading {GlobalConfig.Instance.ConfigFile}...".Pastel("#3d9785"));
            if(!GlobalConfig.Instance.LoadConfiguration())
            {
                Console.WriteLine($"FATAL! Could not load {GlobalConfig.Instance.ConfigFile} correctly!".Pastel(Color.Red));
                Environment.Exit(1);
            }

            Console.WriteLine("Logging in as bot...".Pastel("#3d9785"));
            await SocketClient.LoginAsync(TokenType.Bot, GlobalConfig.Instance.LoadedConfig.BotToken);
            await SocketClient.StartAsync();
            Console.WriteLine("Connected to websocket.".Pastel("#3d9785"));

            if (GlobalConfig.Instance.LoadedConfig.RotatingStatus)
            {
                Console.WriteLine("Loading rotating statuses...".Pastel("#3d9785"));
                if (!GlobalConfig.Instance.LoadStatuses())
                {
                    Console.WriteLine($"Could not load {GlobalConfig.Instance.StatusFile} correctly.".Pastel(Color.IndianRed));
                    UseRotatingStatus = false;
                }
            }

            SocketClient.Ready += Ready;
            Console.Clear();

            await CommandsService.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceProvider);
            Console.WriteLine(FiggleFonts.Standard.Render(GlobalConfig.Instance.LoadedConfig.BotName.ToUpper()).Pastel("#77b6a9"));
            Console.WriteLine($"Source code {GlobalConfig.Instance.LoadedConfig.BotVersion}, Discord.NET 2.2.0".Pastel(Color.DarkGoldenrod));
        }
    }
}
