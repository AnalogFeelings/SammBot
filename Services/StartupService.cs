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
		public IServiceProvider ServiceProvider;
		public DiscordSocketClient SocketClient { get; set; }
		public CommandService CommandsService { get; set; }
		public Logger BotLogger { get; set; }

		public Timer StatusTimer;
		public Timer RngResetTimer;
		public Timer AvatarTimer;

		public AutodeqList<string> RecentAvatars;

		public StartupService(IServiceProvider provider, DiscordSocketClient client, CommandService command, Logger logger)
		{
			ServiceProvider = provider;
			SocketClient = client;
			CommandsService = command;
			BotLogger = logger;

			RecentAvatars = new AutodeqList<string>(Settings.Instance.LoadedConfig.AvatarRecentQueueSize);
		}

		public async Task StartAsync()
		{
			BotLogger.Log("Logging in as a bot...", LogSeverity.Information);
			await SocketClient.LoginAsync(TokenType.Bot, Settings.Instance.LoadedConfig.BotToken);
			await SocketClient.StartAsync();
			BotLogger.Log("Succesfully connected to web socket.", LogSeverity.Information);

			if (Settings.Instance.LoadedConfig.RotatingStatus)
			{
				BotLogger.Log("Loading rotating status list...", LogSeverity.Information);
				if (!Settings.Instance.LoadStatuses())
				{
					BotLogger.Log($"Could not load {Settings.Instance.StatusFile} correctly.", LogSeverity.Error);
				}
			}

			SocketClient.Connected += OnConnected;
			SocketClient.Ready += OnReady;
			SocketClient.Disconnected += OnDisconnect;
			Console.Clear();

			await CommandsService.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceProvider);
			Settings.Instance.StartupStopwatch.Stop();

			Console.Title = $"Samm-Bot {Settings.Instance.LoadedConfig.BotVersion}";

			string DiscordNetVersion = Assembly.GetAssembly(typeof(SessionStartLimit)).GetName().Version.ToString(3);
			string MatchaVersion = Assembly.GetAssembly(typeof(MatchaLogger)).GetName().Version.ToString(3);

			Console.Write(FiggleFonts.Slant.Render(Settings.Instance.LoadedConfig.BotName).Pastel(Color.SkyBlue));
			Console.Write("===========".Pastel(Color.CadetBlue));
			Console.Write($"Source code {Settings.Instance.LoadedConfig.BotVersion}, Discord.NET {DiscordNetVersion}".Pastel(Color.LightCyan));
			Console.WriteLine("===========".Pastel(Color.CadetBlue));
			Console.WriteLine();

			Settings.Instance.RuntimeStopwatch.Start();

			BotLogger.Log($"Using MatchaLogger {MatchaVersion}.", LogSeverity.Information);

			BotLogger.Log($"{Settings.Instance.LoadedConfig.BotName} took" +
				$" {Settings.Instance.StartupStopwatch.ElapsedMilliseconds}ms to boot.", LogSeverity.Information);
		}

		private Task OnConnected()
		{
			BotLogger.Log("Connected to gateway.", LogSeverity.Information);

			return Task.CompletedTask;
		}

		public Task OnReady()
		{
			if (Settings.Instance.StatusList.Count > 0 && Settings.Instance.LoadedConfig.RotatingStatus)
			{
				StatusTimer = new Timer(async _ =>
				{
					BotStatus ChosenStatus = Settings.Instance.StatusList.PickRandom();

					await SocketClient.SetGameAsync(ChosenStatus.Content,
						ChosenStatus.Type == 1 ? Settings.Instance.LoadedConfig.TwitchUrl : null, (ActivityType)ChosenStatus.Type);
				}, null, TimeSpan.Zero, TimeSpan.FromSeconds(20));
			}

			if (Settings.Instance.LoadedConfig.RotatingAvatar)
			{
				AvatarTimer = new Timer(async _ =>
				{
					List<string> AvatarList = Directory.EnumerateFiles("Avatars").ToList();
					if (AvatarList.Count < 2) return;

					List<string> FilteredList = AvatarList.Except(RecentAvatars).ToList();

					string ChosenAvatar = FilteredList.PickRandom();
					BotLogger.Log($"Setting bot avatar to \"{Path.GetFileName(ChosenAvatar)}\".", LogSeverity.Information);

					using (FileStream AvatarStream = new(ChosenAvatar, FileMode.Open))
					{
						Image LoadedAvatar = new Image(AvatarStream);

						await SocketClient.CurrentUser.ModifyAsync(x => x.Avatar = LoadedAvatar);
					}

					RecentAvatars.Push(ChosenAvatar);
				}, null, TimeSpan.FromHours(Settings.Instance.LoadedConfig.AvatarRotationTime), TimeSpan.FromHours(Settings.Instance.LoadedConfig.AvatarRotationTime));
			}

			RngResetTimer = new Timer(_ =>
			{
				int NewHash = Guid.NewGuid().GetHashCode();

				Settings.Instance.GlobalRng = new Random(NewHash);
				BotLogger.Log($"Regenerated RNG instance with hash {NewHash}.", LogSeverity.Information);

			}, null, TimeSpan.FromMinutes(Settings.Instance.LoadedConfig.RngResetTime),
					 TimeSpan.FromMinutes(Settings.Instance.LoadedConfig.RngResetTime));

			BotLogger.Log($"{Settings.Instance.LoadedConfig.BotName} is ready to run.", LogSeverity.Information);

			return Task.CompletedTask;
		}

		public Task OnDisconnect(Exception IncludedException)
		{
			BotLogger.Log("Client has disconnected from the gateway! Exception details below.\n" + IncludedException.ToString(), LogSeverity.Warning);

			return Task.CompletedTask;
		}
	}
}
