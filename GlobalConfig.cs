using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace SammBotNET
{
    public class GlobalConfig
    {
        private static GlobalConfig PrivateInstance;

        public string ConfigFile = "config.json";
        public string StatusFile = "status.xml";
        public Random GlobalRng = new(Guid.NewGuid().GetHashCode());

        public JsonConfig LoadedConfig = new();
        public List<BotStatus> StatusList = new();

        public bool LoadConfiguration()
        {
            if (!File.Exists(ConfigFile)) return false;

            string configFileContent = File.ReadAllText(ConfigFile);
            LoadedConfig = JsonConvert.DeserializeObject<JsonConfig>(configFileContent);

            return true;
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
        public bool RotatingStatus { get; set; } = true;
        public string BotToken { get; set; } = "";
        public string TwitchUrl { get; set; } = "https://www.twitch.tv/vanebrain";
        public ulong AestheticalUid { get; set; } = 337950448130719754;
        public ulong NeveahUid { get; set; } = 850874605434175500;
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
