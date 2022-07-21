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
using System.Text.RegularExpressions;

namespace SammBotNET.Core
{
	public class Settings
	{
		public string ConfigFile = "config.json";
		public string StatusFile = "status.json";

		public Random GlobalRng = new Random(Guid.NewGuid().GetHashCode());
		public Stopwatch StartupStopwatch = new Stopwatch();
		public Stopwatch RuntimeStopwatch = new Stopwatch();

		public JsonConfig LoadedConfig = new JsonConfig();
		public List<BotStatus> StatusList = new List<BotStatus>();

		public Regex UrlRegex;

		public bool LoadConfiguration()
		{
			if (!File.Exists(ConfigFile)) return false;

			string ConfigContent = File.ReadAllText(ConfigFile);
			LoadedConfig = JsonConvert.DeserializeObject<JsonConfig>(ConfigContent);
			UrlRegex = new Regex(LoadedConfig.UrlRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

		public bool LoadStatuses()
		{
			if (!File.Exists(StatusFile)) return false;

			string StatusContent = File.ReadAllText(StatusFile);
			StatusList = JsonConvert.DeserializeObject<List<BotStatus>>(StatusContent);

			return true;
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
		public string BotName { get; set; } = "Samm-Bot";
		public string BotVersion { get; set; } = "v0.1";
		public string BotPrefix { get; set; } = "s.";

		public ulong OwnerUserId { get; set; } = 337950448130719754; //AKA AestheticalZ's discord account.
		public int Rule34Threshold { get; set; } = 30;
		public int QueueWaitTime { get; set; } = 1000;
		public int TagDistance { get; set; } = 3;

		public List<string> BannedPrefixes { get; set; } = null;
		public List<string> MagicBallAnswers { get; set; } = null;
		public List<string> HugKaomojis { get; set; } = null;
		public List<string> KillMessages { get; set; } = null;

		public string ShipBarStartEmpty { get; set; } = null;
		public string ShipBarStartFull { get; set; } = null;
		public string ShipBarHalfEmpty { get; set; } = null;
		public string ShipBarHalfFull { get; set; } = null;
		public string ShipBarEndEmpty { get; set; } = null;
		public string ShipBarEndFull { get; set; } = null;

		[NotModifiable] public string AnsiRegex { get; set; } = "";
		[NotModifiable] public string UrlRegex { get; set; } = "";
		[NotModifiable] public bool RotatingStatus { get; set; } = true;
		[NotModifiable] public bool RotatingAvatar { get; set; } = true;
		[NotModifiable] public string BotToken { get; set; } = "";
		[NotModifiable] public string CatKey { get; set; } = "";
		[NotModifiable] public string DogKey { get; set; } = "";
		[NotModifiable] public string OpenWeatherKey { get; set; } = "";
		[NotModifiable] public string TwitchUrl { get; set; } = "https://www.twitch.tv/coreaesthetics";
		[NotModifiable] public string CommandLogFormat { get; set; } = "Executing command {0}!";
		[NotModifiable] public string LogFolder { get; set; } = "Logs";
		[NotModifiable] public int RngResetTime { get; set; } = 25;
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
