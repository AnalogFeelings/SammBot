using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Pastel;
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

			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

			Console.WriteLine($"Loading {Settings.Instance.ConfigFile}...".Pastel("#3d9785"));
			if (!Settings.Instance.LoadConfiguration())
			{
				string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				string FullPath = Path.Combine(AppData, Settings.BOT_NAME);

				Console.WriteLine($"FATAL! Could not load {Settings.Instance.ConfigFile} correctly!\n".Pastel(Color.Red) +
					$"Make sure the path \"{FullPath}\" exists.\n".Pastel(Color.Red) +
					$"Either way, the program has attempted to write the default config.json file to that path.".Pastel(Color.Red));

				File.WriteAllText(Path.Combine(FullPath, Settings.Instance.ConfigFile),
					JsonConvert.SerializeObject(Settings.Instance.LoadedConfig, Formatting.Indented));

				Environment.Exit(1);
			}

			if (!Directory.Exists(Settings.Instance.LoadedConfig.LogFolder))
			{
				Console.WriteLine($"{Settings.Instance.LoadedConfig.LogFolder} did not exist. Creating...".Pastel("#3d9785"));
				Directory.CreateDirectory(Settings.Instance.LoadedConfig.LogFolder);
			}
			if (!Directory.Exists("Avatars"))
			{
				Console.WriteLine($"Avatars folder did not exist. Creating...".Pastel("#3d9785"));
				Directory.CreateDirectory("Avatars");
			}

			Console.WriteLine("Starting Socket Client...".Pastel("#3d9785"));

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

			Console.WriteLine("Configuring Services...".Pastel("#3d9785"));

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

			Console.WriteLine("Starting Startup Service...".Pastel("#3d9785"));
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
