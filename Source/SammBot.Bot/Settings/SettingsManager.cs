#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021-2024 Analog Feelings
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using SammBot.Library;

namespace SammBot.Bot.Settings;

public class SettingsManager
{
    public BotConfig LoadedConfig = new BotConfig();

    public bool LoadConfiguration()
    {
        string configFilePath = Path.Combine(Constants.BotDataDirectory, Constants.CONFIG_FILE);

        try
        {
            Directory.CreateDirectory(Constants.BotDataDirectory);

            if (!File.Exists(configFilePath)) return false;
        }
        catch (Exception)
        {
            return false;
        }

        string configContent = File.ReadAllText(configFilePath);

        LoadedConfig = JsonSerializer.Deserialize<BotConfig>(configContent);

        return LoadedConfig != null;
    }

    public static string GetBotVersion()
    {
        Version botVersion = Assembly.GetEntryAssembly()!.GetName().Version!;

        return botVersion.ToString(2);
    }

    private static SettingsManager? _privateInstance;
    public static SettingsManager Instance
    {
        get
        {
            return _privateInstance ??= new SettingsManager();
        }
    }
}