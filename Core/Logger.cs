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

        public Logger(DiscordSocketClient client, CommandService command)
        {
            string LogFilename = DateTime.Now.ToString("yyy-M-d") + ".txt";
            string CombinedPath = Path.Combine("Logs", LogFilename);

            LogFileWriter = new StreamWriter(File.Open(CombinedPath, FileMode.Append, FileAccess.Write, FileShare.Read));

            AnsiRegex = new Regex(Settings.Instance.LoadedConfig.AnsiRegex, RegexOptions.Compiled);

            client.Log += LogAsync;
            command.Log += LogAsync;
        }

        public void Log(string message, LogLevel severity, bool logToFile = true)
        {
            string assembledMessageCon = "[".Pastel(Color.White) + DateTime.Now.ToString("g").Pastel(Color.LightGray) + " ";

            Color messageColor = Color.CadetBlue;
            switch (severity)
            {
                case LogLevel.Warning:
                    assembledMessageCon += "WRN".Pastel(Color.Yellow);
                    assembledMessageCon += "] ".Pastel(Color.White);
                    messageColor = Color.Gold;
                    break;
                case LogLevel.Error:
                    assembledMessageCon += "ERR".Pastel(Color.Red);
                    assembledMessageCon += "] ".Pastel(Color.White);
                    messageColor = Color.IndianRed;
                    break;
                case LogLevel.Message:
                    assembledMessageCon += "MSG".Pastel(Color.Cyan);
                    assembledMessageCon += "] ".Pastel(Color.White);
                    break;
            }

            assembledMessageCon += message.Pastel(messageColor);

            string clearOutput = AnsiRegex.Replace(assembledMessageCon, "");

            Console.WriteLine(assembledMessageCon);
            if (logToFile) WriteToLogFile(clearOutput);
        }

        public void LogException(Exception exception)
        {
            Log(exception.Message + exception.StackTrace, LogLevel.Error);
        }

        //Used by the client and the command handler.
        public Task LogAsync(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    if (message.Exception is CommandException cmdException)
                    {
                        string formattedException = string.Format("Exception in command \"{0}{1}\", at channel #{2}.\n{3}",
                            Settings.Instance.LoadedConfig.BotPrefix, cmdException.Command.Aliases[0], cmdException.Context.Channel, cmdException);

                        Log(formattedException, LogLevel.Error);
                    }
                    else Log(message.Message, LogLevel.Error);
                    break;
                case LogSeverity.Warning:
                    Log(message.Message, LogLevel.Warning);
                    break;
                default:
                    Log(message.Message, LogLevel.Message);
                    break;
            }

            return Task.CompletedTask;
        }

        private void WriteToLogFile(string message)
        {
            if (LogFileWriter.BaseStream == null)
            {
                Log("Could not write to log file!", LogLevel.Error, false);
                return;
            }
            LogFileWriter.WriteLine(message);
            LogFileWriter.Flush();
        }
    }
}
