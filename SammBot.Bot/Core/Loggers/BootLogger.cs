#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2022 AestheticalZ
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

using Pastel;
using System;
using System.Drawing;

namespace SammBot.Bot.Core;

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