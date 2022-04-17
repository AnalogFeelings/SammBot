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

        public Task LogAsync(LogMessage message)
        {
            if (message.Exception is CommandException cmdException)
            {
                string formattedException = string.Format("Exception in command \"{0}{1}\", at channel #{2}.\n{3}",
                    Settings.Instance.LoadedConfig.BotPrefix, cmdException.Command.Aliases[0], cmdException.Context.Channel, cmdException);

                Log(LogLevel.Error, formattedException);
            }
            else Log(LogLevel.Message, message.Message);

            return Task.CompletedTask;
        }

        public void Log(LogLevel severity, string message, bool logToFile = true)
        {
            string assembledMessageCon = "[".Pastel(Color.White) + DateTime.Now.ToString("g").Pastel(Color.Gray);

            Color messageColor = Color.CadetBlue;

            switch (severity)
            {
                case LogLevel.Warning:
                    assembledMessageCon += "WRN".Pastel(Color.Yellow);
                    assembledMessageCon += "]".Pastel(Color.White);
                    messageColor = Color.Gold;
                    break;
                case LogLevel.Error:
                    assembledMessageCon += "ERR".Pastel(Color.Red);
                    assembledMessageCon += "]".Pastel(Color.White);
                    messageColor = Color.IndianRed;
                    break;
                case LogLevel.Message:
                    assembledMessageCon += "MSG".Pastel(Color.Cyan);
                    assembledMessageCon += "]".Pastel(Color.White);
                    break;
            }

            assembledMessageCon += message.Pastel(messageColor);

            string clearOutput = AnsiRegex.Replace(assembledMessageCon, "");

            Console.WriteLine(assembledMessageCon);
            if (logToFile) WriteToLogFile(clearOutput);
        }

        public void LogException(Exception exception)
        {
            Log(LogLevel.Error, exception.Message + exception.StackTrace);
        }

        public void DisposeStream()
        {
            LogFileWriter.Close();
            LogFileWriter.Dispose();
        }

        private void WriteToLogFile(string message)
        {
            if (LogFileWriter.BaseStream == null)
            {
                Log(LogLevel.Error, "Could not write to log file!", false);
                return;
            }
            LogFileWriter.WriteLine(message);
            LogFileWriter.Flush();
        }
    }
}
