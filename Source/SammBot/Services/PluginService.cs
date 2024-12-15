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
using System.Linq;
using System.Reflection;
using AnalogFeelings.Matcha;
using SammBot.Library.Models;
using SammBot.Library.Models.Data;

namespace SammBot.Services;

public class PluginService
{
    private readonly MatchaLogger _matchaLogger;
    
    private readonly BotConfig _botConfig;
    
    public readonly Dictionary<IPlugin, Assembly> Plugins = new Dictionary<IPlugin, Assembly>();
    
    public PluginService(SettingsService settingsService, MatchaLogger logger)
    {
        _matchaLogger = logger;
        
        _botConfig = settingsService.GetSettings<BotConfig>()!;
    }

    public void LoadPlugins()
    {
        _matchaLogger.Log(LogSeverity.Information, "Loading plugins...");
        
        string[] files = Directory.GetFiles("./Plugins");

        if (files.Length == 0)
        {
            _matchaLogger.Log(LogSeverity.Information, "No plugins found.");

            return;
        }
        
        foreach (string plugin in _botConfig.Plugins)
        {
            string path = Path.Combine("./Plugins", plugin);
            
            if (!File.Exists(path))
            {
                _matchaLogger.Log(LogSeverity.Error, $"Plugin \"{plugin}\" does not exist. Skipping.");

                continue;
            }

            try
            {
                string fullPath = Path.GetFullPath(path);
                Assembly assembly = Assembly.LoadFile(fullPath);

                List<Type> pluginTypes = assembly.GetTypes().Where(x => typeof(IPlugin).IsAssignableFrom(x)).ToList();

                if (pluginTypes.Count > 1)
                {
                    _matchaLogger.Log(LogSeverity.Error, $"Plugin \"{plugin}\" has more than one initializer. Skipping.");

                    continue;
                }
                
                IPlugin pluginInstance = (IPlugin)Activator.CreateInstance(pluginTypes[0])!;
                
                Plugins.Add(pluginInstance, assembly);
                
                _matchaLogger.Log(LogSeverity.Success, $"Plugin \"{plugin}\" loaded successfully.");
            }
            catch (Exception ex)
            {
                _matchaLogger.Log(LogSeverity.Error, $"Plugin \"{plugin}\" could not be loaded.\n{ex}");
            }
        }
    }
}