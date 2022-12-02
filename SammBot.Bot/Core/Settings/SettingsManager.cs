using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace SammBot.Bot.Core;

public class SettingsManager
{
    public BotConfig LoadedConfig = new BotConfig();

    public const string BOT_NAME = "Samm-Bot";
    public const string BOT_CONFIG_FOLDER = "Bot";
    public const string CONFIG_FILE = "config.json";

    public readonly string BotDataDirectory;

    private SettingsManager()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        BotDataDirectory = Path.Combine(appData, BOT_NAME, BOT_CONFIG_FOLDER);
    }

    public bool LoadConfiguration()
    {
        string configFilePath = Path.Combine(BotDataDirectory, CONFIG_FILE);

        try
        {
            Directory.CreateDirectory(BotDataDirectory);

            if (!File.Exists(configFilePath)) return false;
        }
        catch (Exception)
        {
            return false;
        }

        string configContent = File.ReadAllText(configFilePath);
        LoadedConfig = JsonConvert.DeserializeObject<BotConfig>(configContent);

        return true;
    }

    public static string GetBotVersion()
    {
        Version botVersion = Assembly.GetEntryAssembly()!.GetName().Version;

        return botVersion!.ToString(2);
    }

    private static SettingsManager _PrivateInstance;
    public static SettingsManager Instance
    {
        get
        {
            return _PrivateInstance ??= new SettingsManager();
        }
    }
}