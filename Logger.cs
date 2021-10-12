using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pastel;
using System;
using System.IO;
using System.Threading.Tasks;
using Color = System.Drawing.Color;

namespace SammBotNET
{
    public enum LogLevel
    {
        Message,
        Warning,
        Error
    };

    public class Logger
    {
        private StreamWriter LogFileWriter;

        public Logger(DiscordSocketClient client, CommandService command)
        {
            string LogFilename = DateTime.Now.ToString("d-M-yyy") + ".txt";
            LogFileWriter = new StreamWriter(File.Open(Path.Combine("Logs", LogFilename),
                                                        FileMode.Append, FileAccess.Write, FileShare.Read));

            client.Log += LogAsync;
            command.Log += LogAsync;
        }

        public Task LogAsync(LogMessage message)
        {
            if (message.Exception is CommandException cmdException)
            {
                string formattedException = string.Format("in {0}, at channel {1}.\n{2}",
                    cmdException.Command.Aliases[0], cmdException.Context.Channel, cmdException);

                Log(LogLevel.Error, formattedException);
            }
            else Log(LogLevel.Message, message.Message);

            return Task.CompletedTask;
        }

        public void Log(LogLevel severity, string message, bool logToFile = true)
        {
            string assembledMessageLog = $"{DateTime.Now.ToString("g")} ";
            string assembledMessageCon = assembledMessageLog.Pastel(Color.White);

            Color messageColor = Color.CadetBlue;

            switch (severity)
            {
                case LogLevel.Warning:
                    assembledMessageLog += "[WRN] ";
                    assembledMessageCon += "[WRN] ".Pastel(Color.Yellow);
                    messageColor = Color.Gold;
                    break;
                case LogLevel.Error:
                    assembledMessageLog += "[ERR] ";
                    assembledMessageCon += "[ERR] ".Pastel(Color.Red);
                    messageColor = Color.DarkRed;
                    break;
                case LogLevel.Message:
                    assembledMessageLog += "[MSG] ";
                    assembledMessageCon += "[MSG] ".Pastel(Color.Cyan);
                    break;
            }

            assembledMessageLog += message;
            assembledMessageCon += message.Pastel(messageColor);

            Console.WriteLine(assembledMessageCon);
            if (logToFile) WriteToLogFile(assembledMessageLog);
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
