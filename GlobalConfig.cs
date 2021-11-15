using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml;
using SammBotNET.Extensions;

namespace SammBotNET
{
    public class GlobalConfig
    {
        private static GlobalConfig PrivateInstance;

        public string ConfigFile = "config.json";
        public string StatusFile = "status.xml";

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

            XmlDocument document = new();
            document.Load(StatusFile);

            XmlNodeList cmdNodes = document.DocumentElement.SelectNodes("/Statuses/Status");

            foreach (XmlNode cmdNode in cmdNodes)
            {
                XmlElement element = (XmlElement)cmdNode;

                string status = element.GetAttribute("content");
                string unconvertedType = element.GetAttribute("type");
                int statusType = int.Parse(unconvertedType);

                StatusList.Add(new BotStatus(status, statusType));
                Debug.WriteLine($"Loaded status with content \"{status}\", type {statusType}");
            }
            return true;
        }

        public static GlobalConfig Instance
        {
            get
            {
                if (PrivateInstance == null) PrivateInstance = new GlobalConfig();
                return PrivateInstance;
            }
        }
    }

    public class JsonConfig
    {
        public string BotName { get; set; } = "Samm-Bot";
        public string BotVersion { get; set; } = "v0.1";
        public string BotPrefix { get; set; } = "s.";
        [NotModifiable] public bool RotatingStatus { get; set; } = true;
        [NotModifiable] public string BotToken { get; set; } = "";
        [NotModifiable] public string TwitchUrl { get; set; } = "https://www.twitch.tv/vanebrain";
        public ulong AestheticalUid { get; set; } = 337950448130719754;
        public ulong SkylerUid { get; set; } = 850874605434175500;
        [NotModifiable] public string CommandLogFormat { get; set; } = "Executing command {0}!";
        [NotModifiable] public string LogFolder { get; set; } = "Logs";
        public string UrlRegex { get; set; } = "";
        [NotModifiable] public int RngResetTime { get; set; } = 25;
        public List<string> BannedPrefixes { get; set; } = null;
        public int Rule34Threshold { get; set; } = 30;
        public List<string> MagicBallAnswers { get; set; } = null;
        public List<string> HugKaomojis { get; set; } = null;
        [NotModifiable] public int PeoneRecentQueueSize { get; set; } = 15;
    }

    public class BotStatus
    {
        public string Content;
        public int Type;

        public BotStatus(string content, int type)
        {
            this.Content = content;
            this.Type = type;
        }
    }
}
