using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pastel;
using System;
using System.Threading.Tasks;
using Color = System.Drawing.Color;

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
            string time = DateTime.Now.ToString("dd/MM/yy HH:mm:ss");
            if (message.Exception is CommandException cmdException)
            {
                string formattedException = string.Format("in {0}, at channel {1}.\n{2}",
                    cmdException.Command.Aliases[0], cmdException.Context.Channel, cmdException);

                Console.WriteLine($"{time} EXCEPTION >> ".Pastel(Color.Red) + formattedException.Pastel(Color.Yellow));
            }
            else
            {
                Console.WriteLine($"{time} >> ".Pastel(Color.LavenderBlush) + message.Message.Pastel(Color.LimeGreen));
            }

            return Task.CompletedTask;
        }

        public void Log(string msg)
        {
            string time = DateTime.Now.ToString("dd/MM/yy HH:mm:ss");
            Console.WriteLine($"{time} >> ".Pastel(Color.LavenderBlush) + msg);
        }
    }
}
