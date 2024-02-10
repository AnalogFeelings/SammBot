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

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Matcha;
using SammBot.Bot.Settings;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SammBot.Bot.Logging;

public class Logger
{
    private readonly MatchaLogger _LoggerInstance;

    public Logger(DiscordShardedClient discordClient, InteractionService interactionService)
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

        discordClient.Log += LogAsync;
        interactionService.Log += LogAsync;
    }

    public void Log(string? message, LogSeverity severity)
    {
        if(message != null)
            _LoggerInstance.Log(message, severity);
    }

    public void LogException(Exception exception) =>
        Log(exception.ToString(), LogSeverity.Error);

    //Used by the client and the command handler.
    private Task LogAsync(LogMessage message)
    {
        switch (message.Severity)
        {
            case Discord.LogSeverity.Debug:
                Log(message.Message, LogSeverity.Debug);
                break;
            case Discord.LogSeverity.Critical:
                Log(message.Message, LogSeverity.Fatal);
                break;
            case Discord.LogSeverity.Error:
                Log(message.Message, LogSeverity.Error);
                break;
            case Discord.LogSeverity.Warning:
                Log(message.Message, LogSeverity.Warning);
                break;
            default:
                Log(message.Message, LogSeverity.Information);
                break;
        }

        return Task.CompletedTask;
    }
}