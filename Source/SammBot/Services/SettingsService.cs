#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021 Analog Feelings
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
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using AnalogFeelings.Matcha;
using SammBot.Library;

namespace SammBot.Services;

/// <summary>
/// Contains methods to get configuration from JSON files.
/// </summary>
public class SettingsService
{
    private readonly MatchaLogger _matchaLogger;

    private readonly Dictionary<Type, object> _settingsCache  = new Dictionary<Type, object>();
    
    /// <summary>
    /// Initializes a new instance of <see cref="SettingsService"/>.
    /// </summary>
    /// <returns>True if the configuration was loaded successfully.</returns>
    public SettingsService(MatchaLogger logger)
    {
        _matchaLogger = logger;
    }
    
    /// <summary>
    /// Fetches the settings data for the specified settings container type.
    /// </summary>
    /// <typeparam name="T">The settings container's type.</typeparam>
    /// <returns>A populated container, or null if no data was found.</returns>
    public T? GetSettings<T>() where T : class
    {
        Type settingsType = typeof(T);
        
        if(_settingsCache.TryGetValue(settingsType, out object? value))
            return value as T;
        
        string @namespace = settingsType.Namespace!;
        string directory = Path.Combine(Constants.BotConfigPath, @namespace);;
        string file = Path.Combine(directory, settingsType.Name + ".json");

        try
        {
            Directory.CreateDirectory(directory);
        }
        catch (Exception e)
        {
            _matchaLogger.Log(LogSeverity.Error, $"Could not create settings directory \"{directory}\". Check your filesystem permissions.");

            return null;
        }

        if (!File.Exists(file))
        {
            _matchaLogger.Log(LogSeverity.Error, $"Could not find settings file \"{file}\". Creating default settings.");

            T defaultSettings = Activator.CreateInstance<T>();
            string serializedDefaultSettings = JsonSerializer.Serialize(defaultSettings, Constants.JsonSettings);
            
            File.WriteAllText(file, serializedDefaultSettings);
            
            return null;
        }

        string serializedSettings = File.ReadAllText(file);
        T? deserializedSettings = JsonSerializer.Deserialize<T>(serializedSettings, Constants.JsonSettings);

        if (deserializedSettings == null)
        {
            _matchaLogger.Log(LogSeverity.Error, $"Could not deserialize settings file \"{directory}\".");
            
            return null;
        }
        
        _settingsCache[settingsType] = deserializedSettings;
        
        return deserializedSettings;
    }
}