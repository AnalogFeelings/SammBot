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
        public Random GlobalRng = new Random(Guid.NewGuid().GetHashCode());

        public JsonConfig LoadedConfig = new JsonConfig();
        public List<BotStatus> StatusList = new List<BotStatus>();

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

            XmlDocument document = new XmlDocument();
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
        [DefaultValue("Samm-Bot")]
        public string BotName { get; set; }
        [DefaultValue("v0.1")]
        public string BotVersion { get; set; }
        [DefaultValue("s.")]
        public string BotPrefix { get; set; }
        [DefaultValue(true)]
        public bool RotatingStatus { get; set; }
        [DefaultValue("")]
        public string BotToken { get; set; }
        [DefaultValue("https://www.twitch.tv/vanebrain")]
        public string TwitchUrl { get; set; }
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
