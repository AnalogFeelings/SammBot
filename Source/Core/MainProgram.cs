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
	public partial class MainProgram
	{
		public DiscordSocketClient SocketClient;
		public CommandService CommandService;

		public static void Main()
			=> new MainProgram().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			Settings.Instance.StartupStopwatch.Start();

			BootLogger BootLogger = new BootLogger();

			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

			BootLogger.Log($"Loading {Settings.CONFIG_FILE}...", LogSeverity.Information);
			if (!Settings.Instance.LoadConfiguration())
			{
				string FullPath = Settings.Instance.BotDataDirectory;

				BootLogger.Log($"Could not load {Settings.CONFIG_FILE} correctly! Make sure the path \"{FullPath}\" exists.\n" +
					$"Either way, the program has attempted to write the default {Settings.CONFIG_FILE} file to that path.", LogSeverity.Fatal);

				File.WriteAllText(Path.Combine(FullPath, Settings.CONFIG_FILE),
					JsonConvert.SerializeObject(Settings.Instance.LoadedConfig, Formatting.Indented));

				BootLogger.Log("Press any key to exit...", LogSeverity.Information);
				Console.ReadKey();

				Environment.Exit(1);
			}

			BootLogger.Log("Loaded configuration successfully.", LogSeverity.Success);

			string LogsDirectory = Path.Combine(Settings.Instance.BotDataDirectory, "Logs");

			if (!Directory.Exists(LogsDirectory))
			{
				BootLogger.Log("Logs folder did not exist. Creating...", LogSeverity.Warning);

				try
				{
					Directory.CreateDirectory(LogsDirectory);
					BootLogger.Log("Created Logs folder successfully.", LogSeverity.Success);
				}
				catch (Exception ex)
				{
					BootLogger.Log("Could not create Logs folder. Running the bot without file logging has yet to be implemented.\n" +
						$"Exception Message: {ex.Message}", LogSeverity.Error);

					BootLogger.Log("Press any key to exit...", LogSeverity.Information);
					Console.ReadKey();

					Environment.Exit(1);
				}
			}

			string AvatarsDirectory = Path.Combine(Settings.Instance.BotDataDirectory, "Avatars");

			if (!Directory.Exists(AvatarsDirectory))
			{
				BootLogger.Log("Avatars folder did not exist. Creating...", LogSeverity.Warning);

				try
				{
					Directory.CreateDirectory(AvatarsDirectory);
					BootLogger.Log("Created Avatars folder successfully.", LogSeverity.Success);
				}
				catch (Exception ex)
				{
					BootLogger.Log("Could not create Avatars folder. Rotating avatars will not be available.\n" +
						$"Exception Message: {ex.Message}", LogSeverity.Error);

					BootLogger.Log("Press any key to continue...", LogSeverity.Information);
					Console.ReadKey();
				}
			}

			BootLogger.Log("Creating Discord client...", LogSeverity.Information);

			SocketClient = new DiscordSocketClient(new DiscordSocketConfig
			{
				LogLevel = Discord.LogSeverity.Warning,
				MessageCacheSize = 2000,
				AlwaysDownloadUsers = true,
				GatewayIntents = GatewayIntents.All,
				LogGatewayIntentWarnings = false
			});
			CommandService = new CommandService(new CommandServiceConfig
			{
				LogLevel = Discord.LogSeverity.Info,
				DefaultRunMode = RunMode.Async,
			});

			BootLogger.Log("Created Discord client successfully.", LogSeverity.Success);

			BootLogger.Log("Configuring service provider...", LogSeverity.Information);

			ServiceCollection Services = new ServiceCollection();
			ConfigureServices(Services);

			ServiceProvider Provider = Services.BuildServiceProvider();
			Provider.GetRequiredService<Logger>();
			Provider.GetRequiredService<CommandHandler>();
			Provider.GetRequiredService<RandomService>();
			Provider.GetRequiredService<AdminService>();
			Provider.GetRequiredService<NsfwService>();
			Provider.GetRequiredService<FunService>();
			Provider.GetRequiredService<UtilsService>();

			BootLogger.Log("Configured service provider successfully.", LogSeverity.Success);

			BootLogger.Log("Starting the startup service...", LogSeverity.Information);
			await Provider.GetRequiredService<StartupService>().StartAsync();

			await Task.Delay(-1);
		}

		private void ConfigureServices(IServiceCollection Services)
		{
			Services.AddSingleton(SocketClient)
				.AddSingleton(CommandService)
				.AddSingleton<CommandHandler>()
				.AddSingleton<StartupService>()
				.AddSingleton<Logger>()
				.AddSingleton<RandomService>()
				.AddSingleton<AdminService>()
				.AddSingleton<FunService>()
				.AddSingleton<UtilsService>()
				.AddSingleton<NsfwService>();
		}
	}
}
