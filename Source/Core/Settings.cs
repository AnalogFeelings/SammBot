global using SammBotNET.Classes;
global using SammBotNET.Core;
global using SammBotNET.Database;
global using SammBotNET.Extensions;
global using SammBotNET.RestDefinitions;
global using SammBotNET.Services;
global using LogSeverity = Matcha.LogSeverity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace SammBotNET.Core
{
	public class Settings
	{
		public Stopwatch StartupStopwatch = new Stopwatch();
		public Stopwatch RuntimeStopwatch = new Stopwatch();

		public JsonConfig LoadedConfig = new JsonConfig();

		public const string BOT_NAME = "Samm-Bot";
		public const string CONFIG_FILE = "config.json";

		public readonly string BotDataDirectory;

		public Settings()
		{
			string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

			BotDataDirectory = Path.Combine(AppData, BOT_NAME);
		}

		public bool LoadConfiguration()
		{
			string ConfigFilePath = Path.Combine(BotDataDirectory, CONFIG_FILE);

			try
			{
				DirectoryInfo BotDirectory = Directory.CreateDirectory(BotDataDirectory);

				if (!File.Exists(ConfigFilePath)) return false;
			}
			catch(Exception)
			{
				return false;
			}

			string ConfigContent = File.ReadAllText(ConfigFilePath);
			LoadedConfig = JsonConvert.DeserializeObject<JsonConfig>(ConfigContent);

			return true;
		}

		public void RestartBot()
		{
			string TimeoutCommand = $"/C timeout 3 && {Environment.ProcessPath}";
			string ExecutableCommand = "cmd.exe";

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				TimeoutCommand = $"-c \"sleep 3s && {Environment.ProcessPath}\"";
				ExecutableCommand = "bash";
			}

			ProcessStartInfo StartInfo = new ProcessStartInfo()
			{
				Arguments = TimeoutCommand,
				FileName = ExecutableCommand,
				CreateNoWindow = true
			};
			Process.Start(StartInfo);

			Environment.Exit(0);
		}

		private static Settings PrivateInstance;
		public static Settings Instance
		{
			get
			{
				if (PrivateInstance == null) PrivateInstance = new Settings();
				return PrivateInstance;
			}
		}
	}

	public class JsonConfig
	{
		public string BotVersion { get; set; } = "v0.1";
		public string BotPrefix { get; set; } = "s.";

		public int Rule34Threshold { get; set; } = 30;
		public int TagDistance { get; set; } = 3;

		public List<string> MagicBallAnswers { get; set; } = null;
		public List<string> HugKaomojis { get; set; } = null;
		public List<string> KillMessages { get; set; } = null;
		public List<BotStatus> StatusList { get; set; } = null;

		public string ShipBarStartEmpty { get; set; } = null;
		public string ShipBarStartFull { get; set; } = null;
		public string ShipBarHalfEmpty { get; set; } = null;
		public string ShipBarHalfFull { get; set; } = null;
		public string ShipBarEndEmpty { get; set; } = null;
		public string ShipBarEndFull { get; set; } = null;

		[NotModifiable] public bool RotatingStatus { get; set; } = true;
		[NotModifiable] public bool RotatingAvatar { get; set; } = true;
		[NotModifiable] public string BotToken { get; set; } = "";
		[NotModifiable] public string CatKey { get; set; } = "";
		[NotModifiable] public string DogKey { get; set; } = "";
		[NotModifiable] public string OpenWeatherKey { get; set; } = "";
		[NotModifiable] public string TwitchUrl { get; set; } = "https://www.twitch.tv/coreaesthetics";
		[NotModifiable] public string CommandLogFormat { get; set; } = "Executing command \"{0}\". Channel: #{1}. User: @{2}.";
		[NotModifiable] public int AvatarRotationTime { get; set; } = 1;
		[NotModifiable] public int PeoneRecentQueueSize { get; set; } = 15;
		[NotModifiable] public int AvatarRecentQueueSize { get; set; } = 10;
	}

	public class BotStatus
	{
		public string Content { get; set; }
		public int Type { get; set; }
	}
}
