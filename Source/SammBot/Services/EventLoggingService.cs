#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021 Analog Feelings
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using Discord;
using Discord.WebSocket;
using SammBot.Library;
using SammBot.Library.Extensions;
using SammBot.Library.Models.Database;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SammBot.Library.Services;

namespace SammBot.Services;

/// <summary>
/// Provides event handlers for many Discord and bot events.
/// </summary>
public class EventLoggingService
{
    private readonly IServiceProvider _serviceProvider;
    
    public EventLoggingService(IServiceProvider services)
    {
        _serviceProvider = services;
    }
    
    /// <summary>
    /// Raised when a user joins a guild.
    /// Sends a welcome message and logs the event to the logs channel.
    /// </summary>
    /// <param name="newUser">The new user object.</param>
    public async Task OnUserJoinedAsync(SocketGuildUser newUser)
    {
        DatabaseService databaseService = _serviceProvider.GetRequiredService<DatabaseService>();
        SocketGuild currentGuild = newUser.Guild;

        GuildConfig? serverConfig = databaseService.GuildConfigs.FirstOrDefault(x => x.GuildId == currentGuild.Id);

        if (serverConfig == default(GuildConfig)) return;

        if (serverConfig.EnableWelcome && !string.IsNullOrWhiteSpace(serverConfig.WelcomeMessage))
        {
            ISocketMessageChannel? welcomeChannel = currentGuild.GetChannel(serverConfig.WelcomeChannel) as ISocketMessageChannel;

            if (welcomeChannel != null)
            {
                Dictionary<string, object?> templateDict = new Dictionary<string, object?>()
                {
                    ["usermention"] = newUser.Mention,
                    ["servername"] = Format.Bold(currentGuild.Name)
                };
                string formattedMessage = serverConfig.WelcomeMessage.TemplateReplace(templateDict);

                await welcomeChannel.SendMessageAsync(formattedMessage);
            }
        }

        if (serverConfig.EnableLogging)
        {
            ISocketMessageChannel? loggingChannel = currentGuild.GetChannel(serverConfig.LogChannel) as ISocketMessageChannel;

            if (loggingChannel != null)
            {
                EmbedBuilder replyEmbed = new EmbedBuilder();

                replyEmbed.Title = "\U0001f44b User Joined";
                replyEmbed.Description = "A new user has joined the server.";
                replyEmbed.WithColor(Constants.GoodColor);

                replyEmbed.AddField("\U0001f464 User", newUser.Mention);
                replyEmbed.AddField("\U0001faaa User ID", newUser.Id);
                replyEmbed.AddField("\U0001f4c5 Creation Date", $"<t:{newUser.CreatedAt.ToUnixTimeSeconds()}:F>");

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

    /// <summary>
    /// Raised when a user leaves a guild.
    /// Logs the event to the logs channel.
    /// </summary>
    /// <param name="currentGuild">The guild that raised the event.</param>
    /// <param name="user">The user that left.</param>
    public async Task OnUserLeftAsync(SocketGuild currentGuild, SocketUser user)
    {
        DatabaseService databaseService = _serviceProvider.GetRequiredService<DatabaseService>();
        GuildConfig? serverConfig = databaseService.GuildConfigs.FirstOrDefault(x => x.GuildId == currentGuild.Id);

        if (serverConfig == default(GuildConfig)) return;

        if (serverConfig.EnableLogging)
        {
            ISocketMessageChannel? loggingChannel = currentGuild.GetChannel(serverConfig.LogChannel) as ISocketMessageChannel;

            if (loggingChannel != null)
            {
                EmbedBuilder replyEmbed = new EmbedBuilder();

                replyEmbed.Title = "\U0001f590\uFE0F User Left";
                replyEmbed.Description = "A user has left the server.";
                replyEmbed.WithColor(Constants.VeryBadColor);

                replyEmbed.AddField("\U0001f464 User", user.Mention);
                replyEmbed.AddField("\U0001faaa User ID", user.Id);
                replyEmbed.AddField("\U0001f4c5 Creation Date", $"<t:{user.CreatedAt.ToUnixTimeSeconds()}:F>");

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

    /// <summary>
    /// Raised when a message is deleted.
    /// Logs the event to the logs channel.
    /// </summary>
    /// <param name="cachedMessage">The deleted message's cached data.</param>
    /// <param name="cachedChannel">The deleted message's cached channel.</param>
    public async Task OnMessageDeleted(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel)
    {
        if (!cachedMessage.HasValue || !cachedChannel.HasValue) return; // ??? why, if the message should contain a channel already?
        if (cachedChannel.Value is not SocketGuildChannel) return;
        if (cachedMessage.Value.Author.IsBot) return;

        SocketGuildChannel? targetChannel = cachedChannel.Value as SocketGuildChannel;

        DatabaseService databaseService = _serviceProvider.GetRequiredService<DatabaseService>();
        GuildConfig? serverConfig = databaseService.GuildConfigs.FirstOrDefault(x => x.GuildId == targetChannel!.Guild.Id);

        if (serverConfig == default(GuildConfig)) return;

        if (serverConfig.EnableLogging)
        {
            ISocketMessageChannel? loggingChannel = targetChannel!.Guild.GetChannel(serverConfig.LogChannel) as ISocketMessageChannel;

            if (loggingChannel != null)
            {
                EmbedBuilder replyEmbed = new EmbedBuilder();

                replyEmbed.Title = "\u274C Message Deleted";
                replyEmbed.Description = "A message has been deleted.";
                replyEmbed.WithColor(Constants.VeryBadColor);

                string messageContent = string.Empty;
                string attachments = string.Empty;

                if (cachedMessage.Value.Attachments.Count > 0)
                {
                    attachments = "\n";

                    foreach (IAttachment attachment in cachedMessage.Value.Attachments)
                    {
                        attachments += attachment.Url + "\n";
                    }
                }
                if (!string.IsNullOrEmpty(cachedMessage.Value.Content))
                {
                    string sanitizedContent = Format.Sanitize(cachedMessage.Value.Content);

                    messageContent = sanitizedContent.Truncate(1021); // TODO: Make Truncate take in account the final "..." when using substring.
                }

                replyEmbed.AddField("\U0001f464 Author", cachedMessage.Value.Author.Mention, true);
                replyEmbed.AddField("\U0001faaa Author ID", cachedMessage.Value.Author.Id, true);
                replyEmbed.AddField("\u2709\uFE0F Message Content", messageContent + attachments);
                replyEmbed.AddField("\U0001f4e2 Message Channel", $"<#{cachedChannel.Value.Id}>", true);
                replyEmbed.AddField("\U0001f4c5 Send Date", cachedMessage.Value.CreatedAt.ToString(), true);

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

    /// <summary>
    /// Raised when multiple messages are deleted at once.
    /// Logs the event to the logs channel.
    /// </summary>
    /// <param name="cachedMessages">A list of the deleted messages' cached data.</param>
    /// <param name="cachedChannel">The deleted messages' cached channel.</param>
    public async Task OnMessagesBulkDeleted(IReadOnlyCollection<Cacheable<IMessage, ulong>> cachedMessages, Cacheable<IMessageChannel, ulong> cachedChannel)
    {
        if (!cachedChannel.HasValue) return; // ??? why, if the message should contain a channel already?
        if (cachedChannel.Value is not SocketGuildChannel) return;

        SocketGuildChannel? targetChannel = cachedChannel.Value as SocketGuildChannel;

        DatabaseService databaseService = _serviceProvider.GetRequiredService<DatabaseService>();
        GuildConfig? serverConfig = databaseService.GuildConfigs.FirstOrDefault(x => x.GuildId == targetChannel!.Guild.Id);

        if (serverConfig == default(GuildConfig)) return;

        if (serverConfig.EnableLogging)
        {
            ISocketMessageChannel? loggingChannel = targetChannel!.Guild.GetChannel(serverConfig.LogChannel) as ISocketMessageChannel;

            if (loggingChannel != null)
            {
                EmbedBuilder replyEmbed = new EmbedBuilder();

                replyEmbed.Title = "\u274C Messages Bulk Deleted";
                replyEmbed.Description = "Multiple messages have been deleted at once.";
                replyEmbed.WithColor(Constants.VeryBadColor);

                replyEmbed.AddField("\U0001f4e8 Message Count", cachedMessages.Count, true);
                replyEmbed.AddField("\U0001f4e2 Channel", $"<#{cachedChannel.Value.Id}>", true);

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

    /// <summary>
    /// Raised when a role is created in a guild.
    /// Logs the event to the logs channel.
    /// </summary>
    /// <param name="newRole">The new role data.</param>
    public async Task OnRoleCreated(SocketRole newRole)
    {
        DatabaseService databaseService = _serviceProvider.GetRequiredService<DatabaseService>();
        GuildConfig? serverConfig = databaseService.GuildConfigs.FirstOrDefault(x => x.GuildId == newRole.Guild.Id);

        if (serverConfig == default(GuildConfig)) return;

        if (serverConfig.EnableLogging)
        {
            ISocketMessageChannel? loggingChannel = newRole.Guild.GetChannel(serverConfig.LogChannel) as ISocketMessageChannel;

            if (loggingChannel != null)
            {
                EmbedBuilder replyEmbed = new EmbedBuilder();

                replyEmbed.Title = "\U0001f4e6 Role Created";
                replyEmbed.Description = "A new role has been created.";
                replyEmbed.WithColor(Constants.GoodColor);

                replyEmbed.AddField("\U0001f465 Role", newRole.Mention);
                replyEmbed.AddField("\U0001faaa Role ID", newRole.Id);
                replyEmbed.AddField("\U0001f3a8 Role Color", $"RGB({newRole.Color.R}, {newRole.Color.G}, {newRole.Color.B})");

                replyEmbed.WithFooter(x =>
                {
                    x.Text = $"Server ID: {newRole.Guild.Id}";
                    x.IconUrl = newRole.Guild.IconUrl;
                });
                replyEmbed.WithCurrentTimestamp();

                await loggingChannel.SendMessageAsync(null, false, replyEmbed.Build());
            }
        }
    }

    /// <summary>
    /// Raised when a role's properties are changed.
    /// Logs the event to the logs channel.
    /// </summary>
    /// <param name="outdatedRole">The role's previous properties.</param>
    /// <param name="updatedRole">The role's new properties.</param>
    public async Task OnRoleUpdated(SocketRole outdatedRole, SocketRole updatedRole)
    {
        if (outdatedRole.Name == updatedRole.Name && outdatedRole.Color == updatedRole.Color) return;

        SocketGuild currentGuild = updatedRole.Guild;

        DatabaseService databaseService = _serviceProvider.GetRequiredService<DatabaseService>();
        GuildConfig? serverConfig = databaseService.GuildConfigs.FirstOrDefault(x => x.GuildId == currentGuild.Id);

        if (serverConfig == default(GuildConfig)) return;

        if (serverConfig.EnableLogging)
        {
            ISocketMessageChannel? loggingChannel = currentGuild.GetChannel(serverConfig.LogChannel) as ISocketMessageChannel;

            if (loggingChannel != null)
            {
                EmbedBuilder replyEmbed = new EmbedBuilder();

                replyEmbed.Title = "\U0001f4e6 Role Updated";
                replyEmbed.Description = "A role has been updated.";
                replyEmbed.WithColor(Constants.BadColor);

                if (outdatedRole.Name != updatedRole.Name)
                {
                    replyEmbed.AddField("\U0001f4e4 Old Name", outdatedRole.Name);
                    replyEmbed.AddField("\U0001f4e5 New Name", updatedRole.Name);
                }
                else
                {
                    replyEmbed.AddField("\U0001f465 Role", updatedRole.Mention);
                }

                if (outdatedRole.Color != updatedRole.Color)
                {
                    replyEmbed.AddField("\U0001f3a8 Old Color", $"RGB({outdatedRole.Color.R}, {outdatedRole.Color.G}, {outdatedRole.Color.B})", true);
                    replyEmbed.AddField("\U0001f3a8 New Color", $"RGB({updatedRole.Color.R}, {updatedRole.Color.G}, {updatedRole.Color.B})", true);
                }
                else
                {
                    replyEmbed.AddField("\U0001f3a8 Role Color", $"RGB({updatedRole.Color.R}, {updatedRole.Color.G}, {updatedRole.Color.B})");
                }

                replyEmbed.AddField("\U0001faaa Role ID", updatedRole.Id);

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

    /// <summary>
    /// Raised when a user is banned from a guild.
    /// Logs the event to the logs channel.
    /// </summary>
    /// <param name="bannedUser">The banned user's data.</param>
    /// <param name="sourceGuild">The guild where the ban happened.</param>
    public async Task OnUserBanned(SocketUser bannedUser, SocketGuild sourceGuild)
    {
        DatabaseService databaseService = _serviceProvider.GetRequiredService<DatabaseService>();
        GuildConfig? serverConfig = databaseService.GuildConfigs.FirstOrDefault(x => x.GuildId == sourceGuild.Id);

        if (serverConfig == default(GuildConfig)) return;

        if (serverConfig.EnableLogging)
        {
            ISocketMessageChannel? loggingChannel = sourceGuild.GetChannel(serverConfig.LogChannel) as ISocketMessageChannel;

            if (loggingChannel != null)
            {
                EmbedBuilder replyEmbed = new EmbedBuilder();

                replyEmbed.Title = "\U0001f528 User Banned";
                replyEmbed.Description = "A user has been banned from the server.";
                replyEmbed.WithColor(Constants.VeryBadColor);

                replyEmbed.AddField("\U0001f464 User", bannedUser.GetFullUsername());
                replyEmbed.AddField("\U0001faaa User ID", bannedUser.Id);
                replyEmbed.AddField("\U0001f4c5 Creation Date", $"<t:{bannedUser.CreatedAt.ToUnixTimeSeconds()}:F>");

                replyEmbed.WithFooter(x =>
                {
                    x.Text = $"Server ID: {sourceGuild.Id}";
                    x.IconUrl = sourceGuild.IconUrl;
                });
                replyEmbed.WithCurrentTimestamp();

                await loggingChannel.SendMessageAsync(null, false, replyEmbed.Build());
            }
        }
    }

    /// <summary>
    /// Raised when a user is unbanned from a guild.
    /// Logs the event to the logs channel.
    /// </summary>
    /// <param name="unbannedUser">The unbanned user's data.</param>
    /// <param name="sourceGuild">The guild where the unban happened.</param>
    public async Task OnUserUnbanned(SocketUser unbannedUser, SocketGuild sourceGuild)
    {
        DatabaseService databaseService = _serviceProvider.GetRequiredService<DatabaseService>();
        GuildConfig? serverConfig = databaseService.GuildConfigs.FirstOrDefault(x => x.GuildId == sourceGuild.Id);

        if (serverConfig == default(GuildConfig)) return;

        if (serverConfig.EnableLogging)
        {
            ISocketMessageChannel? loggingChannel = sourceGuild.GetChannel(serverConfig.LogChannel) as ISocketMessageChannel;

            if (loggingChannel != null)
            {
                EmbedBuilder replyEmbed = new EmbedBuilder();

                replyEmbed.Title = "\u2705 User Unbanned";
                replyEmbed.Description = "A user has been unbanned from the server.";
                replyEmbed.WithColor(Constants.GoodColor);

                replyEmbed.AddField("\U0001f464 User", unbannedUser.GetFullUsername());
                replyEmbed.AddField("\U0001faaa User ID", unbannedUser.Id);
                replyEmbed.AddField("\U0001f4c5 Creation Date", $"<t:{unbannedUser.CreatedAt.ToUnixTimeSeconds()}:F>");

                replyEmbed.WithFooter(x =>
                {
                    x.Text = $"Server ID: {sourceGuild.Id}";
                    x.IconUrl = sourceGuild.IconUrl;
                });
                replyEmbed.WithCurrentTimestamp();

                await loggingChannel.SendMessageAsync(null, false, replyEmbed.Build());
            }
        }
    }
}