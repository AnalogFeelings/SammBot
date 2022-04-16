global using SammBotNET.Core;
global using SammBotNET.Database;
global using SammBotNET.Extensions;
global using SammBotNET.RestDefinitions;
global using SammBotNET.Services;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace SammBotNET.Core
{
    public class BotCore
    {
        public string ConfigFile = "config.json";
        public string StatusFile = "status.json";

        public Random GlobalRng = new(Guid.NewGuid().GetHashCode());
        public Stopwatch StartupStopwatch = new();
        public Stopwatch RuntimeStopwatch = new();

        public JsonConfig LoadedConfig = new();
        public List<BotStatus> StatusList = new();

        public Regex UrlRegex;

        public bool LoadConfiguration()
        {
            if (!File.Exists(ConfigFile)) return false;

            string configFileContent = File.ReadAllText(ConfigFile);
            LoadedConfig = JsonConvert.DeserializeObject<JsonConfig>(configFileContent);
            UrlRegex = new(LoadedConfig.UrlRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return true;
        }

        public void RestartBot()
        {
            string restartTimeoutCmd = $"/C timeout 3 && {Environment.ProcessPath}";
            string restartFileCmd = "cmd.exe";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                restartTimeoutCmd = $"-c \"sleep 3s && {Environment.ProcessPath}\"";
                restartFileCmd = "bash";
            }

            ProcessStartInfo startInfo = new()
            {
                Arguments = restartTimeoutCmd,
                FileName = restartFileCmd,
                CreateNoWindow = true
            };
            Process.Start(startInfo);
            Environment.Exit(0);
        }

        public bool LoadStatuses()
        {
            if (!File.Exists(StatusFile)) return false;

            string statusFileContent = File.ReadAllText(StatusFile);
            StatusList = JsonConvert.DeserializeObject<List<BotStatus>>(statusFileContent);

            return true;
        }

        private static BotCore PrivateInstance;
        public static BotCore Instance
        {
            get
            {
                if (PrivateInstance == null) PrivateInstance = new();
                return PrivateInstance;
            }
        }
    }

    public class JsonConfig
    {
        public string BotName { get; set; } = "Samm-Bot";
        public string BotVersion { get; set; } = "v0.1";
        public string BotPrefix { get; set; } = "s.";
        public string UrlRegex { get; set; } = "";

        public ulong AestheticalUid { get; set; } = 337950448130719754;
        public int Rule34Threshold { get; set; } = 30;
        public int QueueWaitTime { get; set; } = 1000;
        public int TagDistance { get; set; } = 3;

        public List<string> BannedPrefixes { get; set; } = null;
        public List<string> MagicBallAnswers { get; set; } = null;
        public List<string> HugKaomojis { get; set; } = null;
        public List<string> KillMessages { get; set; } = null;

        [NotModifiable] public bool RotatingStatus { get; set; } = true;
        [NotModifiable] public bool RotatingAvatar { get; set; } = true;
        [NotModifiable] public string BotToken { get; set; } = "";
        [NotModifiable] public string CatKey { get; set; } = "";
        [NotModifiable] public string DogKey { get; set; } = "";
        [NotModifiable] public string TwitchUrl { get; set; } = "https://www.twitch.tv/coreaesthetics";
        [NotModifiable] public string CommandLogFormat { get; set; } = "Executing command {0}!";
        [NotModifiable] public string LogFolder { get; set; } = "Logs";
        [NotModifiable] public int RngResetTime { get; set; } = 25;
        [NotModifiable] public int AvatarRotationTime { get; set; } = 1;
        [NotModifiable] public int PeoneRecentQueueSize { get; set; } = 15;
    }

    public class BotStatus
    {
        public string Content { get; set; }
        public int Type { get; set; }
    }
}
