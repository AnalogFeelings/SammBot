#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2023 AestheticalZ
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
#endregion

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