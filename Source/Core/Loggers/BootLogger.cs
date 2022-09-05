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
                string AssembledMessage = "[".Pastel(Color.White);

                Color MessageColor = Color.LightBlue;
                switch (Severity)
                {
                    case LogSeverity.Debug:
                        AssembledMessage += "DBG".Pastel(Color.Teal);
                        MessageColor = Color.LightSeaGreen;
                        break;
                    case LogSeverity.Information:
                        AssembledMessage += "MSG".Pastel(Color.Cyan);
                        break;
                    case LogSeverity.Success:
                        AssembledMessage += "SUC".Pastel(Color.Green);
                        MessageColor = Color.LightGreen;
                        break;
                    case LogSeverity.Warning:
                        AssembledMessage += "WRN".Pastel(Color.Yellow);
                        MessageColor = Color.Gold;
                        break;
                    case LogSeverity.Error:
                        AssembledMessage += "ERR".Pastel(Color.Red);
                        MessageColor = Color.IndianRed;
                        break;
                    case LogSeverity.Fatal:
                        AssembledMessage += "FTL".Pastel(Color.DarkRed);
                        MessageColor = Color.DarkRed;
                        break;
                }

                AssembledMessage += "] ".Pastel(Color.White);
                AssembledMessage += Message.Pastel(MessageColor);

                Console.WriteLine(AssembledMessage);
            }
        }
    }
}
