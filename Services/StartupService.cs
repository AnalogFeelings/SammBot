using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Figgle;
using Pastel;
using SammBotNET.Core;
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

        public StartupService(IServiceProvider provider) => ServiceProvider = provider;

        public Task Ready()
        {
            if (GlobalConfig.Instance.StatusList.Count > 0 && GlobalConfig.Instance.LoadedConfig.RotatingStatus)
            {
                StatusTimer = new Timer(async _ =>
                {
                    BotStatus status = GlobalConfig.Instance.StatusList[GlobalConfig.Instance.GlobalRng.Next(GlobalConfig.Instance.StatusList.Count)];

                    await SocketClient.SetGameAsync(status.Content,
                        status.Type == 1 ? GlobalConfig.Instance.LoadedConfig.TwitchUrl : null, (ActivityType)status.Type);
                }, null, TimeSpan.Zero, TimeSpan.FromSeconds(20));
            }

            RngResetTimer = new Timer(_ =>
            {
                int hash = Guid.NewGuid().GetHashCode();

                GlobalConfig.Instance.GlobalRng = new(hash);
                BotLogger.Log(LogLevel.Message, $"Regenerated RNG instance with hash {hash}.");

            }, null, TimeSpan.FromMinutes(GlobalConfig.Instance.LoadedConfig.RngResetTime),
                     TimeSpan.FromMinutes(GlobalConfig.Instance.LoadedConfig.RngResetTime));

            return Task.CompletedTask;
        }

        public async Task StartAsync()
        {
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
                }
            }

            SocketClient.Ready += Ready;
            Console.Clear();

            await CommandsService.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceProvider);
            GlobalConfig.Instance.StartupStopwatch.Stop();

            string discordNetVersion = Assembly.GetAssembly(typeof(Discord.SessionStartLimit)).GetName().Version.ToString();

            Console.Write(FiggleFonts.Slant.Render(GlobalConfig.Instance.LoadedConfig.BotName).Pastel("#77b6a9"));
            Console.WriteLine($"----------Source code {GlobalConfig.Instance.LoadedConfig.BotVersion}, Discord.NET {discordNetVersion}----------".Pastel(Color.CornflowerBlue));
            Console.WriteLine();

            GlobalConfig.Instance.RuntimeStopwatch.Start();

            BotLogger.Log(LogLevel.Message, $"{GlobalConfig.Instance.LoadedConfig.BotName} took" +
                $" {GlobalConfig.Instance.StartupStopwatch.ElapsedMilliseconds}ms to boot.");
        }
    }
}
