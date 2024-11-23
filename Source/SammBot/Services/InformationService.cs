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
using System.Diagnostics;
using System.Reflection;

namespace SammBot.Services;

/// <summary>
/// Contains information about the bot.
/// </summary>
public class InformationService
{
    /// <summary>
    /// A stopwatch that keeps track of the bot's uptime.
    /// </summary>
    public Stopwatch Uptime { get; } = new Stopwatch();
    
    /// <summary>
    /// The bot's current version.
    /// </summary>
    public string Version
    {
        get
        {
            if (_version != null)
                return _version;
            
            Version botVersion = Assembly.GetEntryAssembly()!.GetName().Version!;

            _version = botVersion.ToString(2);

            return _version;
        }
    }

    private string? _version;
}