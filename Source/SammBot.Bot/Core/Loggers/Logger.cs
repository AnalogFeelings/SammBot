#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2023 Analog Feelings
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

using Discord;
using Discord.WebSocket;
using Matcha;
using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Interactions;

namespace SammBot.Bot.Core;

public class Logger
{
    private readonly MatchaLogger _LoggerInstance;

    public Logger(DiscordShardedClient Client, InteractionService InteractionService)
    {
        //Default settings.
        MatchaLoggerSettings loggerSettings = new MatchaLoggerSettings()
        {
            LogFilePath = Path.Combine(SettingsManager.Instance.BotDataDirectory, "Logs"), 
#if !DEBUG
            AllowedSeverities = LogSeverity.Information | LogSeverity.Warning | LogSeverity.Error | LogSeverity.Fatal | LogSeverity.Success
#endif
        };

        _LoggerInstance = new MatchaLogger(loggerSettings);

        Client.Log += LogAsync;
        InteractionService.Log += LogAsync;
    }

    public void Log(string? Message, LogSeverity Severity)
    {
        if(Message != null)
            _LoggerInstance.Log(Message, Severity);
    }

    public void LogException(Exception TargetException) =>
        Log(TargetException.ToString(), LogSeverity.Error);

    //Used by the client and the command handler.
    private Task LogAsync(LogMessage Message)
    {
        switch (Message.Severity)
        {
            case Discord.LogSeverity.Debug:
                Log(Message.Message, LogSeverity.Debug);
                break;
            case Discord.LogSeverity.Critical:
                Log(Message.Message, LogSeverity.Fatal);
                break;
            case Discord.LogSeverity.Error:
                Log(Message.Message, LogSeverity.Error);
                break;
            case Discord.LogSeverity.Warning:
                Log(Message.Message, LogSeverity.Warning);
                break;
            default:
                Log(Message.Message, LogSeverity.Information);
                break;
        }

        return Task.CompletedTask;
    }
}