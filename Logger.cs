using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pastel;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET
{
    public class Logger
    {
        public Logger(DiscordSocketClient client, CommandService command)
        {
            client.Log += LogAsync;
            command.Log += LogAsync;
        }
        public Task LogAsync(LogMessage message)
        {
            if (message.Exception is CommandException cmdException)
            {
                string time = DateTime.Now.ToString("dd:MM::yy HH:mm:ss");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(string.Format("{0} Exception --> ", time));
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(string.Format("in {0}, at channel {1}. Exception: {2}",
                    cmdException.Command.Aliases.First(), cmdException.Context.Channel, cmdException));
                Console.ResetColor();
            }
            else
            {
                string time = DateTime.Now.ToString("dd/MM/yy HH:mm:ss");
                Console.Write(string.Format("{0} --> ", time).Pastel(System.Drawing.Color.LavenderBlush));
                Console.WriteLine(message.Message.Pastel(System.Drawing.Color.LimeGreen));
            }

            return Task.CompletedTask;
        }

        public void Log(string msg)
        {
            string time = DateTime.Now.ToString("dd/MM/yy HH:mm:ss");
            Console.Write(string.Format("{0} --> ", time).Pastel(System.Drawing.Color.Lavender));
            Console.WriteLine(msg);
        }
    }
}
