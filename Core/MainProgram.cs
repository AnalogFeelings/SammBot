using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Pastel;
using SammBotNET.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SammBotNET.Core
{
    public partial class MainProgram
    {
        public DiscordSocketClient SocketClient;
        public CommandService CommandService;

        public static void Main()
            => new MainProgram().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            GlobalConfig.Instance.StartupStopwatch.Start();

            Console.WriteLine($"Loading {GlobalConfig.Instance.ConfigFile}...".Pastel("#3d9785"));
            if (!GlobalConfig.Instance.LoadConfiguration())
            {
                Console.WriteLine($"FATAL! Could not load {GlobalConfig.Instance.ConfigFile} correctly!".Pastel(Color.Red));
                File.WriteAllText(GlobalConfig.Instance.ConfigFile,
                    JsonConvert.SerializeObject(GlobalConfig.Instance.LoadedConfig, Formatting.Indented));
                Environment.Exit(1);
            }

            if (!Directory.Exists(GlobalConfig.Instance.LoadedConfig.LogFolder))
            {
                Console.WriteLine($"{GlobalConfig.Instance.LoadedConfig.LogFolder} did not exist. Creating...".Pastel("#3d9785"));
                Directory.CreateDirectory(GlobalConfig.Instance.LoadedConfig.LogFolder);
            }

            Console.WriteLine("Starting Socket Client...".Pastel("#3d9785"));

            SocketClient = new(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Warning,
                MessageCacheSize = 2000,
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.All
            });
            CommandService = new(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Info,
                DefaultRunMode = RunMode.Async,
            });

            Console.WriteLine("Configuring Services...".Pastel("#3d9785"));

            ServiceCollection services = new();
            ConfigureServices(services);

            ServiceProvider provider = services.BuildServiceProvider();
            provider.GetRequiredService<Logger>();
            provider.GetRequiredService<CommandHandler>();
            provider.GetRequiredService<CustomCommandService>();
            provider.GetRequiredService<HelpService>();
            provider.GetRequiredService<QuoteService>();
            provider.GetRequiredService<RandomService>();
            provider.GetRequiredService<AdminService>();
            provider.GetRequiredService<NsfwService>();
            provider.GetRequiredService<FunService>();

            Console.WriteLine("Starting Startup Service...".Pastel("#3d9785"));
            await provider.GetRequiredService<StartupService>().StartAsync();

            await Task.Delay(-1);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(SocketClient)
            .AddSingleton(CommandService)
            .AddSingleton<CommandHandler>()
            .AddSingleton<StartupService>()
            .AddSingleton<Logger>()
            .AddSingleton<CustomCommandService>()
            .AddSingleton<HelpService>()
            .AddSingleton<Random>()
            .AddSingleton<QuoteService>()
            .AddSingleton<RandomService>()
            .AddSingleton<AdminService>()
            .AddSingleton<FunService>()
            .AddSingleton<NsfwService>();
        }
    }
}
