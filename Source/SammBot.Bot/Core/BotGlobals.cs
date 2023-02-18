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

global using LogSeverity = Matcha.LogSeverity;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Discord;

namespace SammBot.Bot.Core;

public class BotGlobals
{
    public readonly Stopwatch StartupStopwatch = new Stopwatch();
    public readonly Stopwatch RuntimeStopwatch = new Stopwatch();

    public readonly AllowedMentions AllowOnlyUsers = new AllowedMentions(AllowedMentionTypes.Users);
        
    private BotGlobals() {}
        
    public static void RestartBot()
    {
        string timeoutCommand = $"/C timeout 3 && {Environment.ProcessPath}";
        string executableCommand = "cmd.exe";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            timeoutCommand = $"-c \"sleep 3s && {Environment.ProcessPath}\"";
            executableCommand = "bash";
        }

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            Arguments = timeoutCommand,
            FileName = executableCommand,
            CreateNoWindow = true
        };
        Process.Start(startInfo);

        Environment.Exit(0);
    }
        
    private static BotGlobals? _PrivateInstance;
    public static BotGlobals Instance
    {
        get
        {
            return _PrivateInstance ??= new BotGlobals();
        }
    }
}