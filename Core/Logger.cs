using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pastel;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Color = System.Drawing.Color;

namespace SammBotNET.Core
{
	public enum LogLevel
	{
		Message,
		Warning,
		Error
	};

	public class Logger
	{
		private readonly StreamWriter LogFileWriter;
		private readonly Regex AnsiRegex;

		public Logger(DiscordSocketClient Client, CommandService CommandService)
		{
			string LogFilename = DateTime.Now.ToString("yyy-M-d") + ".txt";
			string CombinedPath = Path.Combine("Logs", LogFilename);

			LogFileWriter = new StreamWriter(File.Open(CombinedPath, FileMode.Append, FileAccess.Write, FileShare.Read));

			AnsiRegex = new Regex(Settings.Instance.LoadedConfig.AnsiRegex, RegexOptions.Compiled);

			Client.Log += LogAsync;
			CommandService.Log += LogAsync;
		}

		public void Log(string Message, LogLevel Severity, bool LogToFile = true)
		{
			string AssembledMessage = "[".Pastel(Color.White) + DateTime.Now.ToString("g").Pastel(Color.LightGray) + " ";

			Color MessageColor = Color.LightBlue;
			switch (Severity)
			{
				case LogLevel.Warning:
					AssembledMessage += "WRN".Pastel(Color.Yellow);
					AssembledMessage += "] ".Pastel(Color.White);
					MessageColor = Color.Gold;
					break;
				case LogLevel.Error:
					AssembledMessage += "ERR".Pastel(Color.Red);
					AssembledMessage += "] ".Pastel(Color.White);
					MessageColor = Color.IndianRed;
					break;
				case LogLevel.Message:
					AssembledMessage += "MSG".Pastel(Color.Cyan);
					AssembledMessage += "] ".Pastel(Color.White);
					break;
			}

			AssembledMessage += Message.Pastel(MessageColor);

			string FilteredOutput = AnsiRegex.Replace(AssembledMessage, "");

			Console.WriteLine(AssembledMessage);
			if (LogToFile) WriteToLogFile(FilteredOutput);
		}

		public void LogException(Exception TargetException)
		{
			Log(TargetException.Message + TargetException.StackTrace, LogLevel.Error);
		}

		//Used by the client and the command handler.
		public Task LogAsync(LogMessage Message)
		{
			switch (Message.Severity)
			{
				case LogSeverity.Critical:
				case LogSeverity.Error:
					if (Message.Exception is CommandException cmdException)
					{
						string FormattedException = string.Format("Exception in command \"{0}{1}\", at channel #{2}.\n{3}",
								Settings.Instance.LoadedConfig.BotPrefix, cmdException.Command.Aliases[0], cmdException.Context.Channel, cmdException);

						Log(FormattedException, LogLevel.Error);
					}
					else Log(Message.Message, LogLevel.Error);
					break;
				case LogSeverity.Warning:
					Log(Message.Message, LogLevel.Warning);
					break;
				default:
					Log(Message.Message, LogLevel.Message);
					break;
			}

			return Task.CompletedTask;
		}

		private void WriteToLogFile(string Message)
		{
			if (LogFileWriter.BaseStream == null)
			{
				Log("Could not write to log file!", LogLevel.Error, false);
				return;
			}

			LogFileWriter.WriteLine(Message);
			LogFileWriter.Flush();
		}
	}
}
