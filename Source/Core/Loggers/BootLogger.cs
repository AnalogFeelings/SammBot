using Pastel;
using System;
using System.Drawing;

namespace SammBotNET.Core
{
    /// <summary>
    /// Simple logger for the pre-StartupService state of the bot for when Matcha isn't available.
    /// </summary>
    public class BootLogger
    {
        private object LogLock = new object();

        public void Log(string Message, LogSeverity Severity)
        {
            lock (LogLock)
            {
                string assembledMessage = "[".Pastel(Color.White);

                Color messageColor = Color.LightBlue;
                switch (Severity)
                {
                    case LogSeverity.Debug:
                        assembledMessage += "DBG".Pastel(Color.Teal);
                        messageColor = Color.LightSeaGreen;
                        break;
                    case LogSeverity.Information:
                        assembledMessage += "MSG".Pastel(Color.Cyan);
                        break;
                    case LogSeverity.Success:
                        assembledMessage += "SUC".Pastel(Color.Green);
                        messageColor = Color.LightGreen;
                        break;
                    case LogSeverity.Warning:
                        assembledMessage += "WRN".Pastel(Color.Yellow);
                        messageColor = Color.Gold;
                        break;
                    case LogSeverity.Error:
                        assembledMessage += "ERR".Pastel(Color.Red);
                        messageColor = Color.IndianRed;
                        break;
                    case LogSeverity.Fatal:
                        assembledMessage += "FTL".Pastel(Color.DarkRed);
                        messageColor = Color.DarkRed;
                        break;
                }

                assembledMessage += "] ".Pastel(Color.White);
                assembledMessage += Message.Pastel(messageColor);

                Console.WriteLine(assembledMessage);
            }
        }
    }
}
