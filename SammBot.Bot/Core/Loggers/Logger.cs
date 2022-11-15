using Discord;
using Discord.WebSocket;
using Matcha;
using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Interactions;

namespace SammBot.Bot.Core
{
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

        public void Log(string Message, LogSeverity Severity)
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
}
