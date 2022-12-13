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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SammBot.Bot.Core;
using SammBot.Bot.Database;
using SammBot.Bot.Extensions;

namespace SammBot.Bot.Services;

public class EventLoggingService
{
    private readonly Color GoodColor = new Color(119, 178, 85); // Green
    private readonly Color BadColor = new Color(255, 205, 77); // Yellow
    private readonly Color VeryBadColor = new Color(221, 46, 68); // Red
        
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
                    replyEmbed.WithColor(GoodColor);

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
                    replyEmbed.WithColor(VeryBadColor);

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
        if (CachedMessage.Value.Author.IsBot) return;
            
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
                    replyEmbed.WithColor(VeryBadColor);

                    string messageContent = string.Empty;
                    string attachments = string.Empty;

                    if (CachedMessage.Value.Attachments.Count > 0)
                    {
                        attachments = "\n";
                            
                        foreach (IAttachment attachment in CachedMessage.Value.Attachments)
                        {
                            attachments += attachment.Url + "\n";
                        }
                    }
                    if (!string.IsNullOrEmpty(CachedMessage.Value.Content))
                    {
                        string sanitizedContent = Format.Sanitize(CachedMessage.Value.Content);
                            
                        messageContent = sanitizedContent.Truncate(1021); // TODO: Make Truncate take in account the final "..." when using substring.
                    }

                    replyEmbed.AddField("\U0001f464 Author", CachedMessage.Value.Author.Mention, true);
                    replyEmbed.AddField("\U0001faaa Author ID", CachedMessage.Value.Author.Id, true);
                    replyEmbed.AddField("\u2709\uFE0F Message Content", messageContent + attachments);
                    replyEmbed.AddField("\U0001f4e2 Message Channel", $"<#{CachedChannel.Value.Id}>", true);
                    replyEmbed.AddField("\U0001f4c5 Send Date", CachedMessage.Value.CreatedAt.ToString(), true);

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
        
    public async Task OnMessagesBulkDeleted(IReadOnlyCollection<Cacheable<IMessage, ulong>> CachedMessages, Cacheable<IMessageChannel, ulong> CachedChannel)
    {
        if (!CachedChannel.HasValue) return; // ??? why, if the message should contain a channel already?
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

                    replyEmbed.Title = "\u274C Messages Bulk Deleted";
                    replyEmbed.Description = "Multiple messages have been deleted at once.";
                    replyEmbed.WithColor(VeryBadColor);

                    replyEmbed.AddField("\U0001f4e8 Message Count", CachedMessages.Count, true);
                    replyEmbed.AddField("\U0001f4e2 Channel", $"<#{CachedChannel.Value.Id}>", true);

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
        
    public async Task OnMessageUpdated(Cacheable<IMessage, ulong> CachedMessage, SocketMessage NewMessage, ISocketMessageChannel SourceChannel)
    {
        if (SourceChannel is not SocketGuildChannel) return;
        if (NewMessage.Author.IsBot) return;
            
        SocketGuildChannel sourceGuildChannel = SourceChannel as SocketGuildChannel;

        using (BotDatabase botDatabase = new BotDatabase())
        {
            GuildConfig serverConfig = botDatabase.GuildConfigs.FirstOrDefault(x => x.GuildId == sourceGuildChannel.Guild.Id);

            if (serverConfig == default(GuildConfig)) return;

            if (serverConfig.EnableLogging)
            {
                ISocketMessageChannel loggingChannel = sourceGuildChannel.Guild.GetChannel(serverConfig.LogChannel) as ISocketMessageChannel;

                if (loggingChannel != null)
                {
                    EmbedBuilder replyEmbed = new EmbedBuilder();

                    replyEmbed.Title = "\U0001f4dd Message Edited";
                    replyEmbed.Description = $"A message has been edited.\n" +
                                             $"The old content will be listed below if the message has been cached by {SettingsManager.BOT_NAME}.";
                    replyEmbed.WithColor(BadColor);

                    replyEmbed.AddField("\U0001f464 Author", NewMessage.Author.Mention, true);
                    replyEmbed.AddField("\U0001faaa Author ID", NewMessage.Author.Id, true);

                    if (CachedMessage.HasValue)
                    {
                        if (CachedMessage.Value.Content == NewMessage.Content) return;
                            
                        string oldSanitizedContent = Format.Sanitize(CachedMessage.Value.Content);
                        string oldTrimmedContent = oldSanitizedContent.Truncate(512);
                            
                        replyEmbed.AddField("\u2709\uFE0F Old Content", oldTrimmedContent);
                    }

                    string newSanitizedContent = Format.Sanitize(NewMessage.Content);
                    string newTrimmedContent = newSanitizedContent.Truncate(512);

                    replyEmbed.AddField("\U0001f4e9 New Content", newTrimmedContent);
                    replyEmbed.AddField("\U0001f4e2 Message Channel", $"<#{sourceGuildChannel.Id}>", true);
                    replyEmbed.AddField("\U0001f4c5 Send Date", NewMessage.CreatedAt.ToString(), true);

                    replyEmbed.WithFooter(x =>
                    {
                        x.Text = $"Server ID: {sourceGuildChannel.Guild.Id}";
                        x.IconUrl = sourceGuildChannel.Guild.IconUrl;
                    });
                    replyEmbed.WithCurrentTimestamp();

                    await loggingChannel.SendMessageAsync(null, false, replyEmbed.Build());
                }
            }
        }
    }
        
    public async Task OnRoleCreated(SocketRole NewRole)
    {
        using (BotDatabase botDatabase = new BotDatabase())
        {
            GuildConfig serverConfig = botDatabase.GuildConfigs.FirstOrDefault(x => x.GuildId == NewRole.Guild.Id);

            if (serverConfig == default(GuildConfig)) return;

            if (serverConfig.EnableLogging)
            {
                ISocketMessageChannel loggingChannel = NewRole.Guild.GetChannel(serverConfig.LogChannel) as ISocketMessageChannel;

                if (loggingChannel != null)
                {
                    EmbedBuilder replyEmbed = new EmbedBuilder();

                    replyEmbed.Title = "\U0001f4e6 Role Created";
                    replyEmbed.Description = "A new role has been created.";
                    replyEmbed.WithColor(GoodColor);
                        
                    replyEmbed.AddField("\U0001f465 Role", NewRole.Mention);
                    replyEmbed.AddField("\U0001faaa Role ID", NewRole.Id);
                    replyEmbed.AddField("\U0001f3a8 Role Color", $"RGB({NewRole.Color.R}, {NewRole.Color.G}, {NewRole.Color.B})");

                    replyEmbed.WithFooter(x =>
                    {
                        x.Text = $"Server ID: {NewRole.Guild.Id}";
                        x.IconUrl = NewRole.Guild.IconUrl;
                    });
                    replyEmbed.WithCurrentTimestamp();

                    await loggingChannel.SendMessageAsync(null, false, replyEmbed.Build());
                }
            }
        }
    }
        
    public async Task OnRoleUpdated(SocketRole OutdatedRole, SocketRole UpdatedRole)
    {
        if (OutdatedRole.Name == UpdatedRole.Name && OutdatedRole.Color == UpdatedRole.Color) return;
            
        SocketGuild currentGuild = UpdatedRole.Guild;
            
        using (BotDatabase botDatabase = new BotDatabase())
        {
            GuildConfig serverConfig = botDatabase.GuildConfigs.FirstOrDefault(x => x.GuildId == currentGuild.Id);

            if (serverConfig == default(GuildConfig)) return;

            if (serverConfig.EnableLogging)
            {
                ISocketMessageChannel loggingChannel = currentGuild.GetChannel(serverConfig.LogChannel) as ISocketMessageChannel;

                if (loggingChannel != null)
                {
                    EmbedBuilder replyEmbed = new EmbedBuilder();

                    replyEmbed.Title = "\U0001f4e6 Role Updated";
                    replyEmbed.Description = "A role has been updated.";
                    replyEmbed.WithColor(BadColor);

                    if (OutdatedRole.Name != UpdatedRole.Name)
                    {
                        replyEmbed.AddField("\U0001f4e4 Old Name", OutdatedRole.Name);
                        replyEmbed.AddField("\U0001f4e5 New Name", UpdatedRole.Name);
                    }
                    else
                    {
                        replyEmbed.AddField("\U0001f465 Role", UpdatedRole.Mention);
                    }

                    if (OutdatedRole.Color != UpdatedRole.Color)
                    {
                        replyEmbed.AddField("\U0001f3a8 Old Color", $"RGB({OutdatedRole.Color.R}, {OutdatedRole.Color.G}, {OutdatedRole.Color.B})", true);
                        replyEmbed.AddField("\U0001f3a8 New Color", $"RGB({UpdatedRole.Color.R}, {UpdatedRole.Color.G}, {UpdatedRole.Color.B})", true);
                    }
                    else
                    {
                        replyEmbed.AddField("\U0001f3a8 Role Color", $"RGB({UpdatedRole.Color.R}, {UpdatedRole.Color.G}, {UpdatedRole.Color.B})");
                    }
                        
                    replyEmbed.AddField("\U0001faaa Role ID", UpdatedRole.Id);

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

    public async Task OnUserBanned(SocketUser BannedUser, SocketGuild SourceGuild)
    {
        using (BotDatabase botDatabase = new BotDatabase())
        {
            GuildConfig serverConfig = botDatabase.GuildConfigs.FirstOrDefault(x => x.GuildId == SourceGuild.Id);

            if (serverConfig == default(GuildConfig)) return;

            if (serverConfig.EnableLogging)
            {
                ISocketMessageChannel loggingChannel = SourceGuild.GetChannel(serverConfig.LogChannel) as ISocketMessageChannel;

                if (loggingChannel != null)
                {
                    EmbedBuilder replyEmbed = new EmbedBuilder();

                    replyEmbed.Title = "\U0001f528 User Banned";
                    replyEmbed.Description = "A user has been banned from the server.";
                    replyEmbed.WithColor(VeryBadColor);
                        
                    replyEmbed.AddField("\U0001f464 User", BannedUser.GetFullUsername());
                    replyEmbed.AddField("\U0001faaa User ID", BannedUser.Id);
                    replyEmbed.AddField("\U0001f4c5 Creation Date", $"<t:{BannedUser.CreatedAt.ToUnixTimeSeconds()}:F>");

                    replyEmbed.WithFooter(x =>
                    {
                        x.Text = $"Server ID: {SourceGuild.Id}";
                        x.IconUrl = SourceGuild.IconUrl;
                    });
                    replyEmbed.WithCurrentTimestamp();

                    await loggingChannel.SendMessageAsync(null, false, replyEmbed.Build());
                }
            }
        }
    }
        
    public async Task OnUserUnbanned(SocketUser UnbannedUser, SocketGuild SourceGuild)
    {
        using (BotDatabase botDatabase = new BotDatabase())
        {
            GuildConfig serverConfig = botDatabase.GuildConfigs.FirstOrDefault(x => x.GuildId == SourceGuild.Id);

            if (serverConfig == default(GuildConfig)) return;

            if (serverConfig.EnableLogging)
            {
                ISocketMessageChannel loggingChannel = SourceGuild.GetChannel(serverConfig.LogChannel) as ISocketMessageChannel;

                if (loggingChannel != null)
                {
                    EmbedBuilder replyEmbed = new EmbedBuilder();

                    replyEmbed.Title = "\u2705 User Unbanned";
                    replyEmbed.Description = "A user has been unbanned from the server.";
                    replyEmbed.WithColor(GoodColor);
                        
                    replyEmbed.AddField("\U0001f464 User", UnbannedUser.GetFullUsername());
                    replyEmbed.AddField("\U0001faaa User ID", UnbannedUser.Id);
                    replyEmbed.AddField("\U0001f4c5 Creation Date", $"<t:{UnbannedUser.CreatedAt.ToUnixTimeSeconds()}:F>");

                    replyEmbed.WithFooter(x =>
                    {
                        x.Text = $"Server ID: {SourceGuild.Id}";
                        x.IconUrl = SourceGuild.IconUrl;
                    });
                    replyEmbed.WithCurrentTimestamp();

                    await loggingChannel.SendMessageAsync(null, false, replyEmbed.Build());
                }
            }
        }
    }
}