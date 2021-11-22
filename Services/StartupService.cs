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
        public IServiceProvider ServiceProvider;
        public DiscordSocketClient SocketClient { get; set; }
        public CommandService CommandsService { get; set; }
        public Logger BotLogger { get; set; }

        public Timer StatusTimer;
        public Timer RngResetTimer;

        public StartupService(IServiceProvider provider, DiscordSocketClient client, CommandService command, Logger logger)
        {
            ServiceProvider = provider;
            SocketClient = client;
            CommandsService = command;
            BotLogger = logger;
        }

        public Task Ready()
        {
            if (BotCore.Instance.StatusList.Count > 0 && BotCore.Instance.LoadedConfig.RotatingStatus)
            {
                StatusTimer = new Timer(async _ =>
                {
                    BotStatus status = BotCore.Instance.StatusList[BotCore.Instance.GlobalRng.Next(BotCore.Instance.StatusList.Count)];

                    await SocketClient.SetGameAsync(status.Content,
                        status.Type == 1 ? BotCore.Instance.LoadedConfig.TwitchUrl : null, (ActivityType)status.Type);
                }, null, TimeSpan.Zero, TimeSpan.FromSeconds(20));
            }

            RngResetTimer = new Timer(_ =>
            {
                int hash = Guid.NewGuid().GetHashCode();

                BotCore.Instance.GlobalRng = new(hash);
                BotLogger.Log(LogLevel.Message, $"Regenerated RNG instance with hash {hash}.");

            }, null, TimeSpan.FromMinutes(BotCore.Instance.LoadedConfig.RngResetTime),
                     TimeSpan.FromMinutes(BotCore.Instance.LoadedConfig.RngResetTime));

            return Task.CompletedTask;
        }

        public async Task StartAsync()
        {
            Console.WriteLine("Logging in as bot...".Pastel("#3d9785"));
            await SocketClient.LoginAsync(TokenType.Bot, BotCore.Instance.LoadedConfig.BotToken);
            await SocketClient.StartAsync();
            Console.WriteLine("Connected to websocket.".Pastel("#3d9785"));

            if (BotCore.Instance.LoadedConfig.RotatingStatus)
            {
                Console.WriteLine("Loading rotating statuses...".Pastel("#3d9785"));
                if (!BotCore.Instance.LoadStatuses())
                {
                    Console.WriteLine($"Could not load {BotCore.Instance.StatusFile} correctly.".Pastel(Color.IndianRed));
                }
            }

            SocketClient.Ready += Ready;
            Console.Clear();

            await CommandsService.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceProvider);
            BotCore.Instance.StartupStopwatch.Stop();

            string discordNetVersion = Assembly.GetAssembly(typeof(Discord.SessionStartLimit)).GetName().Version.ToString();

            Console.Write(FiggleFonts.Slant.Render(BotCore.Instance.LoadedConfig.BotName).Pastel("#77b6a9"));
            Console.WriteLine($"----------Source code {BotCore.Instance.LoadedConfig.BotVersion}, Discord.NET {discordNetVersion}----------".Pastel(Color.CornflowerBlue));
            Console.WriteLine();

            BotCore.Instance.RuntimeStopwatch.Start();

            BotLogger.Log(LogLevel.Message, $"{BotCore.Instance.LoadedConfig.BotName} took" +
                $" {BotCore.Instance.StartupStopwatch.ElapsedMilliseconds}ms to boot.");
        }
    }
}
