using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Matcha;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SammBotNET.Core
{
	public class Logger
	{
		private MatchaLogger LoggerInstance;

		public Logger(DiscordSocketClient Client, CommandService CommandService)
		{
			//Default settings.
			MatchaLoggerSettings LoggerSettings = new MatchaLoggerSettings()
			{
				LogFilePath = Path.Combine(Settings.Instance.BotDataDirectory, "Logs")
			};

			LoggerInstance = new MatchaLogger(LoggerSettings);

			Client.Log += LogAsync;
			CommandService.Log += LogAsync;
		}

		public void Log(string Message, LogSeverity Severity) =>
			LoggerInstance.Log(Message, Severity);

		public void LogException(Exception TargetException) =>
			Log(TargetException.ToString(), LogSeverity.Error);

		//Used by the client and the command handler.
		public Task LogAsync(LogMessage Message)
		{
			switch (Message.Severity)
			{
				case Discord.LogSeverity.Debug:
					Log(Message.Message, LogSeverity.Debug);
					break;
				case Discord.LogSeverity.Critical:
					if (Message.Exception is CommandException CriticalException)
					{
						string FormattedException = string.Format("Critical exception in command \"{0}{1}\", at channel #{2}.\n{3}",
								Settings.Instance.LoadedConfig.BotPrefix, CriticalException.Command.Aliases[0], CriticalException.Context.Channel, CriticalException);

						Log(FormattedException, LogSeverity.Fatal);
					}
					else Log(Message.Message, LogSeverity.Fatal);
					break;
				case Discord.LogSeverity.Error:
					if (Message.Exception is CommandException ErrorException)
					{
						string FormattedException = string.Format("Exception in command \"{0}{1}\", at channel #{2}.\n{3}",
								Settings.Instance.LoadedConfig.BotPrefix, ErrorException.Command.Aliases[0], ErrorException.Context.Channel, ErrorException);

						Log(FormattedException, LogSeverity.Error);
					}
					else Log(Message.Message, LogSeverity.Error);
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
