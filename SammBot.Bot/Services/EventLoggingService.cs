using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace SammBot.Bot.Services
{
    public class EventLoggingService
    {
        public async Task OnUserJoinedAsync(SocketGuildUser NewUser)
        {
            using (BotDatabase botDatabase = new BotDatabase())
            {
                SocketGuild currentGuild = NewUser.Guild;
                
                GuildConfig serverConfig = botDatabase.GuildConfigs.FirstOrDefault(x => x.GuildId == currentGuild.Id);

                if (serverConfig == default(GuildConfig)) return;

                if (serverConfig.EnableWelcome && !string.IsNullOrWhiteSpace(serverConfig.WelcomeMessage))
                {
                    ISocketMessageChannel welcomeChannel = currentGuild.GetChannel(serverConfig.WelcomeChannel) as ISocketMessageChannel;

                    if (welcomeChannel != null)
                    {
                        string formattedMessage = string.Format(serverConfig.WelcomeMessage, NewUser.Mention, Format.Bold(currentGuild.Name));

                        await welcomeChannel.SendMessageAsync(formattedMessage);
                    }
                }

                if (serverConfig.EnableLogging)
                {
                    ISocketMessageChannel loggingChannel = currentGuild.GetChannel(serverConfig.LogChannel) as ISocketMessageChannel;

                    if (loggingChannel != null)
                    {
                        EmbedBuilder replyEmbed = new EmbedBuilder();

                        replyEmbed.Title = "\U0001f44b User Joined";
                        replyEmbed.Description = "A new user has joined the server.";
                        replyEmbed.WithColor(119, 178, 85);

                        replyEmbed.AddField("\U0001f464 User", NewUser.Mention);
                        replyEmbed.AddField("\U0001faaa User ID", NewUser.Id);
                        replyEmbed.AddField("\U0001f4c5 Creation Date", $"<t:{NewUser.CreatedAt.ToUnixTimeSeconds()}:F>");

                        replyEmbed.WithFooter(x =>
                        {
                            x.Text = $"Server ID: {currentGuild.Id}";
                            x.IconUrl = currentGuild.IconUrl;
                        });
                        replyEmbed.WithCurrentTimestamp();

                        await loggingChannel.SendMessageAsync(null, false, replyEmbed.Build());
                    }
                }
            }
        }
        
        public async Task OnUserLeftAsync(SocketGuild CurrentGuild, SocketUser User)
        {
            using (BotDatabase botDatabase = new BotDatabase())
            {
                GuildConfig serverConfig = botDatabase.GuildConfigs.FirstOrDefault(x => x.GuildId == CurrentGuild.Id);

                if (serverConfig == default(GuildConfig)) return;

                if (serverConfig.EnableLogging)
                {
                    ISocketMessageChannel loggingChannel = CurrentGuild.GetChannel(serverConfig.LogChannel) as ISocketMessageChannel;

                    if (loggingChannel != null)
                    {
                        EmbedBuilder replyEmbed = new EmbedBuilder();

                        replyEmbed.Title = "\U0001f590\uFE0F User Left";
                        replyEmbed.Description = "A user has left the server.";
                        replyEmbed.WithColor(255, 205, 77);

                        replyEmbed.AddField("\U0001f464 User", User.Mention);
                        replyEmbed.AddField("\U0001faaa User ID", User.Id);
                        replyEmbed.AddField("\U0001f4c5 Creation Date", $"<t:{User.CreatedAt.ToUnixTimeSeconds()}:F>");

                        replyEmbed.WithFooter(x =>
                        {
                            x.Text = $"Server ID: {CurrentGuild.Id}";
                            x.IconUrl = CurrentGuild.IconUrl;
                        });
                        replyEmbed.WithCurrentTimestamp();

                        await loggingChannel.SendMessageAsync(null, false, replyEmbed.Build());
                    }
                }
            }
        }
    }
}