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
        
        public async Task OnMessageDeleted(Cacheable<IMessage, ulong> CachedMessage, Cacheable<IMessageChannel, ulong> CachedChannel)
        {
            if (!CachedMessage.HasValue || !CachedChannel.HasValue) return; // ??? why, if the message should contain a channel already?
            if (CachedChannel.Value is not SocketGuildChannel) return;
            
            SocketGuildChannel targetChannel = CachedChannel.Value as SocketGuildChannel;

            using (BotDatabase botDatabase = new BotDatabase())
            {
                GuildConfig serverConfig = botDatabase.GuildConfigs.FirstOrDefault(x => x.GuildId == targetChannel.Guild.Id);

                if (serverConfig == default(GuildConfig)) return;

                if (serverConfig.EnableLogging)
                {
                    ISocketMessageChannel loggingChannel = targetChannel.Guild.GetChannel(serverConfig.LogChannel) as ISocketMessageChannel;

                    if (loggingChannel != null)
                    {
                        EmbedBuilder replyEmbed = new EmbedBuilder();

                        replyEmbed.Title = "\u274C Message Deleted";
                        replyEmbed.Description = "A message has been deleted.";
                        replyEmbed.WithColor(221, 46, 68);

                        string sanitizedContent = Format.Sanitize(CachedMessage.Value.Content);
                        string trimmedContent = sanitizedContent.Truncate(1021); // TODO: Make Truncate take in account the final "..." when using substring.

                        replyEmbed.AddField("\U0001f464 Author", CachedMessage.Value.Author.Mention, true);
                        replyEmbed.AddField("\U0001faaa Author ID", CachedMessage.Value.Author.Id, true);
                        replyEmbed.AddField("\u2709\uFE0F Message Content", trimmedContent);
                        replyEmbed.AddField("\U0001f4c5 Send Date", CachedMessage.Value.CreatedAt.ToString());

                        replyEmbed.WithFooter(x =>
                        {
                            x.Text = $"Server ID: {targetChannel.Guild.Id}";
                            x.IconUrl = targetChannel.Guild.IconUrl;
                        });
                        replyEmbed.WithCurrentTimestamp();

                        await loggingChannel.SendMessageAsync(null, false, replyEmbed.Build());
                    }
                }
            }
        }
    }
}