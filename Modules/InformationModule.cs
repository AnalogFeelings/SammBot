using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Bot Information")]
    [Group("info")]
    public class InformationModule : ModuleBase<SocketCommandContext>
    {
        [Command("full", RunMode = RunMode.Async)]
        [Summary("Shows the FULL information of the bot.")]
        public async Task InformationFullAsync()
        {
            EmbedBuilder embed = new EmbedBuilder
            {
                Color = Color.DarkPurple,
                Title = "SAMM-BOT INFORMATION",
                Description = "This is all the information about the bot."
            };

            embed.AddField("Bot Version:", $"`{GlobalConfig.Instance.LoadedConfig.BotVersion}`", true);
            embed.AddField(".NET Version:", $"`{RuntimeInformation.FrameworkDescription}`", true);
            embed.AddField("Running on:", $"`{FriendlyOSName()}`", true);
            embed.AddField("Ping:", $"`{Context.Client.Latency}ms.`", true);
            embed.AddField("Im in:", $"`{Context.Client.Guilds.Count} server/s.`", true);
            embed.AddField("Hotel?", $"`Trivago.`", true);

            embed.WithAuthor(author => author.Name = "SAMM-BOT COMMANDS");
            embed.WithFooter(footer => footer.Text = "Samm-Bot");
            embed.WithCurrentTimestamp();

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("servers", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Shows a list of all the servers the bot is in.")]
        public async Task ServersAsync()
        {
            string builtMsg = "I am invited in the following servers:\n```md\n";
            string inside = string.Empty;

            int i = 1;
            foreach (SocketGuild guild in Context.Client.Guilds)
            {
                inside += $"{i}. < {guild.Name} > with <{guild.MemberCount}> members.\n";
                i++;
            }
            inside += "```";
            builtMsg += inside;
            await ReplyAsync(builtMsg);
        }

        public string FriendlyOSName()
        {
            Version version = Environment.OSVersion.Version;
            string os;
            switch (version.Major)
            {
                case 6:
                    switch (version.Minor)
                    {
                        case 1: os = $"Windows 7 (NT {version.Major}.{version.Minor})"; break;
                        case 2: os = $"Windows 8 (NT {version.Major}.{version.Minor})"; break;
                        case 3: os = $"Windows 8.1 (NT {version.Major}.{version.Minor})"; break;
                        default: os = "Unknown"; break;
                    }
                    break;
                case 10:
                    switch (version.Minor)
                    {
                        case 0: os = $"Windows 10 (NT {version.Major}.{version.Minor})"; break;
                        default: os = "Unknown"; break;
                    }
                    break;
                default:
                    os = "Unknown System";
                    break;
            }
            return os;
        }

    }
}