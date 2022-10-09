using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace SammBotNET.Core
{
    public class MainProgram
    {
        private DiscordShardedClient _ShardedClient;
        private CommandService _CommandService;

        public static void Main()
            => new MainProgram().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            Settings.Instance.StartupStopwatch.Start();

            BootLogger bootLogger = new BootLogger();

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            bootLogger.Log($"Loading {Settings.CONFIG_FILE}...", LogSeverity.Information);
            if (!Settings.Instance.LoadConfiguration())
            {
                string fullPath = Settings.Instance.BotDataDirectory;
                Task writeTask = File.WriteAllTextAsync(Path.Combine(fullPath, Settings.CONFIG_FILE),
                    JsonConvert.SerializeObject(Settings.Instance.LoadedConfig, Formatting.Indented));

                bootLogger.Log($"Could not load {Settings.CONFIG_FILE} correctly! Make sure the path \"{fullPath}\" exists.\n" +
                    $"{Settings.BOT_NAME} has attempted to write the default {Settings.CONFIG_FILE} file to that path.", LogSeverity.Fatal);

                await writeTask;

                bootLogger.Log("Press any key to exit...", LogSeverity.Information);
                Console.ReadKey();

                Environment.Exit(1);
            }

            bootLogger.Log("Loaded configuration successfully.", LogSeverity.Success);

            string logsDirectory = Path.Combine(Settings.Instance.BotDataDirectory, "Logs");

            if (!Directory.Exists(logsDirectory))
            {
                bootLogger.Log("Logs folder did not exist. Creating...", LogSeverity.Warning);

                try
                {
                    Directory.CreateDirectory(logsDirectory);
                    bootLogger.Log("Created Logs folder successfully.", LogSeverity.Success);
                }
                catch (Exception ex)
                {
                    bootLogger.Log("Could not create Logs folder. Running the bot without file logging has yet to be implemented.\n" +
                        $"Exception Message: {ex.Message}", LogSeverity.Error);

                    bootLogger.Log("Press any key to exit...", LogSeverity.Information);
                    Console.ReadKey();

                    Environment.Exit(1);
                }
            }

            string avatarsDirectory = Path.Combine(Settings.Instance.BotDataDirectory, "Avatars");

            if (!Directory.Exists(avatarsDirectory))
            {
                bootLogger.Log("Avatars folder did not exist. Creating...", LogSeverity.Warning);

                try
                {
                    Directory.CreateDirectory(avatarsDirectory);
                    bootLogger.Log("Created Avatars folder successfully.", LogSeverity.Success);
                }
                catch (Exception ex)
                {
                    bootLogger.Log("Could not create Avatars folder. Rotating avatars will not be available.\n" +
                        $"Exception Message: {ex.Message}", LogSeverity.Error);

                    bootLogger.Log("Press any key to continue...", LogSeverity.Information);
                    Console.ReadKey();
                }
            }

            bootLogger.Log("Creating Discord client...", LogSeverity.Information);

            _ShardedClient = new DiscordShardedClient(new DiscordSocketConfig
            {
                LogLevel = Discord.LogSeverity.Warning,
                MessageCacheSize = Settings.Instance.LoadedConfig.MessageCacheSize,
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.All,
                LogGatewayIntentWarnings = false
            });
            _CommandService = new CommandService(new CommandServiceConfig
            {
                LogLevel = Discord.LogSeverity.Info,
                DefaultRunMode = RunMode.Async,
            });

            bootLogger.Log("Created Discord client successfully.", LogSeverity.Success);

            bootLogger.Log("Configuring service provider...", LogSeverity.Information);

            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.GetRequiredService<Logger>();
            serviceProvider.GetRequiredService<CommandHandler>();
            serviceProvider.GetRequiredService<RandomService>();
            serviceProvider.GetRequiredService<AdminService>();
            serviceProvider.GetRequiredService<FunService>();
            serviceProvider.GetRequiredService<UtilsService>();

            bootLogger.Log("Configured service provider successfully.", LogSeverity.Success);

            bootLogger.Log("Starting the startup service...", LogSeverity.Information);
            await serviceProvider.GetRequiredService<StartupService>().StartAsync();

            await Task.Delay(-1);
        }

        private void ConfigureServices(IServiceCollection Services)
        {
            Services.AddSingleton(_ShardedClient)
                .AddSingleton(_CommandService)
                .AddSingleton<CommandHandler>()
                .AddSingleton<StartupService>()
                .AddSingleton<Logger>()
                .AddSingleton<RandomService>()
                .AddSingleton<AdminService>()
                .AddSingleton<FunService>()
                .AddSingleton<UtilsService>();
        }
    }
}
