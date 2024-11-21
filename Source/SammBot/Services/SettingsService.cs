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
// along with this program.  If not, see &lt;https://www.gnu.org/licenses/&gt;.
#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using SammBot.Library;
using SammBot.Library.Models.Data;

namespace SammBot.Services;

/// <summary>
/// Contains the bot's settings and functions to handle them.
/// </summary>
public class SettingsService
{
    /// <summary>
    /// The loaded settings in memory.
    /// </summary>
    public BotConfig Settings { get; private set; } = null!;
    
    /// <summary>
    /// Loads the configuration from the bot's data directory into memory.
    /// </summary>
    /// <returns>True if the configuration was loaded successfully.</returns>
    [MemberNotNullWhen(true, nameof(Settings))]
    public bool LoadConfiguration()
    {
        string configFilePath = Path.Combine(Constants.BotDataDirectory, Constants.CONFIG_FILE);

        try
        {
            Directory.CreateDirectory(Constants.BotDataDirectory);

            if (!File.Exists(configFilePath)) 
                return false;
            
            string configContent = File.ReadAllText(configFilePath);
            BotConfig? config = JsonSerializer.Deserialize<BotConfig>(configContent, Constants.JsonSettings);

            if (config == default)
                return false;
            
            Settings = config;

            return true;

        }
        catch (Exception)
        {
            return false;
        }
    }
}