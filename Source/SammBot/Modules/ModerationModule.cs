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

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using SammBot.Library;
using SammBot.Library.Attributes;
using SammBot.Library.Extensions;
using SammBot.Library.Models;
using SammBot.Library.Models.Database;
using SammBot.Library.Preconditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SammBot.Services;

namespace SammBot.Modules;

[PrettyName("Moderation")]
[Group("mod", "Moderation commands like kick, ban, mute, etc.")]
[ModuleEmoji("\U0001f9d1\u200D\u2696\uFE0F")]
public class ModerationModule : InteractionModuleBase<ShardedInteractionContext>
{
    [SlashCommand("ban", "Bans a user with a reason.")]
    [DetailedDescription("Bans a user from the server with the set reason.")]
    [RateLimit(1, 2)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    [RequireUserPermission(GuildPermission.BanMembers)]
    public async Task<RuntimeResult> BanUserAsync
    (
        [Summary("User", "The user you want to ban.")]
        SocketGuildUser targetUser,
        [Summary("PruneDays", "The amount of days the bot will delete.")]
        int pruneDays,
        [Summary("Reason", "The reason of the ban.")]
        string? reason = null
    )
    {
        string banReason = reason ?? "No reason specified.";

        await Context.Guild.AddBanAsync(targetUser, pruneDays, banReason);

        EmbedBuilder replyEmbed = new EmbedBuilder().BuildSuccessEmbed(Context)
                                                    .WithDescription($"Successfully banned user `{targetUser.GetFullUsername()}`.");

        replyEmbed.AddField("\U0001f914 Reason", banReason);
        replyEmbed.AddField("\U0001f5d3\uFE0F Prune Days", $"{pruneDays} day(s).");

        await RespondAsync(embed: replyEmbed.Build(), allowedMentions: Constants.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    [SlashCommand("kick", "Kicks a user with a reason.")]
    [DetailedDescription("Kicks a user from the server with the set reason.")]
    [RateLimit(1, 2)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(GuildPermission.KickMembers)]
    [RequireUserPermission(GuildPermission.KickMembers)]
    public async Task<RuntimeResult> KickUserAsync
    (
        [Summary("User", "The user you want to kick.")]
        SocketGuildUser targetUser,
        [Summary("Reason", "The reason of the kick.")]
        string? reason = null
    )
    {
        string kickReason = reason ?? "No reason specified.";

        await targetUser.KickAsync(kickReason);

        EmbedBuilder replyEmbed = new EmbedBuilder().BuildSuccessEmbed(Context)
                                                    .WithDescription($"Successfully kicked user `{targetUser.GetFullUsername()}`.");

        replyEmbed.AddField("\U0001f914 Reason", kickReason);

        await RespondAsync(embed: replyEmbed.Build(), allowedMentions: Constants.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    [SlashCommand("warn", "Warns a user with a reason.")]
    [DetailedDescription("Warns a user with a reason. Warnings will be stored in the bot's database, and you will be able to list them afterwards.")]
    [RateLimit(1, 2)]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.KickMembers)]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.KickMembers)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    public async Task<RuntimeResult> WarnUserAsync
    (
        [Summary("User", "The user you want to warn.")]
        SocketGuildUser targetUser,
        [Summary("Reason", "The reason of the warn.")]
        string reason
    )
    {
        if (reason.Length > 32)
            return ExecutionResult.FromError("Warning reason must not exceed 32 characters.");

        await DeferAsync();

        using (DatabaseService databaseService = new DatabaseService())
        {
            UserWarning newWarning = new UserWarning
            {
                UserId = targetUser.Id,
                GuildId = Context.Guild.Id,
                Reason = reason,
                Date = Context.Interaction.CreatedAt.ToUnixTimeSeconds()
            };

            await databaseService.UserWarnings.AddAsync(newWarning);
            await databaseService.SaveChangesAsync();

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildSuccessEmbed(Context);

            replyEmbed.Description = $"Successfully warned user {targetUser.Mention}.";
            
            replyEmbed.AddField("\U0001f914 Reason", reason);
            replyEmbed.AddField("\U0001f6c2 Warn ID", newWarning.Id);

            //DM the user about it.
            try
            {
                EmbedBuilder directMessageEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

                directMessageEmbed.Title = $"{Constants.WARNING_EMOJI} You have been warned";
                directMessageEmbed.Description = "You may see all of your warnings with the `/mod warns` command in the server.";
                directMessageEmbed.Color = Constants.BadColor;

                directMessageEmbed.AddField("\U0001faaa Server", Context.Guild.Name);
                directMessageEmbed.AddField("\U0001f914 Reason", reason);
                directMessageEmbed.AddField("\U0001f6c2 Warn ID", newWarning.Id);

                await targetUser.SendMessageAsync(embed: directMessageEmbed.Build());
            }
            catch (Exception)
            {
                replyEmbed.Description += "\nI could not message the user about this warning.";
            }

            await FollowupAsync(embed: replyEmbed.Build(), allowedMentions: Constants.AllowOnlyUsers);
        }

        return ExecutionResult.Succesful();
    }

    [SlashCommand("unwarn", "Removes a warn from a user.")]
    [DetailedDescription("Removes the warning with the specified ID.")]
    [RateLimit(1, 2)]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.KickMembers)]
    [RequireUserPermission(GuildPermission.BanMembers)]
    public async Task<RuntimeResult> RemoveWarnAsync
    (
        [Summary("WarnId", "The ID of the warn you want to remove.")]
        int warnId
    )
    {
        await DeferAsync(true);

        using (DatabaseService databaseService = new DatabaseService())
        {
            UserWarning? specificWarning = await databaseService.UserWarnings.SingleOrDefaultAsync(x => x.Id == warnId && x.GuildId == Context.Guild.Id);

            if (specificWarning == default(UserWarning))
                return ExecutionResult.FromError("There are no warnings with the specified ID.");

            databaseService.UserWarnings.Remove(specificWarning);

            await databaseService.SaveChangesAsync();

            await FollowupAsync($":white_check_mark: Removed warning \"{warnId}\" from user <@{specificWarning.UserId}>.",
                                allowedMentions: Constants.AllowOnlyUsers);
        }

        return ExecutionResult.Succesful();
    }

    [SlashCommand("warns", "Lists all of the warns given to a user.")]
    [DetailedDescription("Replies with a list of warnings given to the specified user.")]
    [RateLimit(2, 1)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> ListWarnsAsync
    (
        [Summary("User", "The user you want to list the warns for.")]
        SocketGuildUser targetUser
    )
    {
        await DeferAsync();

        using (DatabaseService databaseService = new DatabaseService())
        {
            List<UserWarning> filteredWarnings = databaseService.UserWarnings.Where(x => x.UserId == targetUser.Id && x.GuildId == Context.Guild.Id).ToList();

            if (!filteredWarnings.Any())
                return ExecutionResult.FromError("This user has no warnings.");

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

            replyEmbed.Title = "\U0001f4c3 List of Warnings";

            foreach (UserWarning warning in filteredWarnings)
            {
                replyEmbed.Description += $"{Constants.WARNING_EMOJI} **ID**: `{warning.Id}`\n";
                replyEmbed.Description += $"**· Creation Date**: <t:{warning.Date}:F>\n";
                replyEmbed.Description += $"**· Reason**: {warning.Reason.Truncate(48)}\n\n";
            }

            await FollowupAsync(embed: replyEmbed.Build(), allowedMentions: Constants.AllowOnlyUsers);
        }

        return ExecutionResult.Succesful();
    }

    [SlashCommand("viewwarn", "Lists a specific warn.")]
    [DetailedDescription("Lists a warning, and the full reason.")]
    [RateLimit(2, 1)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> ListWarnAsync
    (
        [Summary("WarnId", "The ID of the warn you want to view.")]
        int warnId
    )
    {
        await DeferAsync();

        using (DatabaseService databaseService = new DatabaseService())
        {
            UserWarning? specificWarning = await databaseService.UserWarnings.SingleOrDefaultAsync(x => x.Id == warnId && x.GuildId == Context.Guild.Id);

            if (specificWarning == default(UserWarning))
                return ExecutionResult.FromError("There are no warnings with the specified ID.");

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

            replyEmbed.Title = "Warning Details";
            replyEmbed.Description = $"Details for the warning \"{specificWarning.Id}\".\n";

            replyEmbed.AddField("User", $"<@{specificWarning.UserId}> (ID: {specificWarning.UserId})");
            replyEmbed.AddField("Date", $"<t:{specificWarning.Date}:F>");
            replyEmbed.AddField("Reason", specificWarning.Reason);

            await FollowupAsync(embed: replyEmbed.Build(), allowedMentions: Constants.AllowOnlyUsers);
        }

        return ExecutionResult.Succesful();
    }

    [SlashCommand("mute", "Mutes a user for an amount of time with a reason.")]
    [DetailedDescription("Mutes the specified user for an amount of time with the specified reason. The reason is optional.")]
    [RateLimit(1, 2)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(GuildPermission.ModerateMembers)]
    [RequireUserPermission(GuildPermission.ModerateMembers)]
    public async Task<RuntimeResult> MuteUserAsync
    (
        [Summary("User", "The user you want to mute.")]
        SocketGuildUser targetUser,
        [Summary("Duration", "The duration of the mute.")]
        TimeSpan duration,
        [Summary("Reason", "The reason of the mute.")]
        string? reason = null
    )
    {
        string muteReason = reason ?? "No reason specified.";

        if (duration < TimeSpan.Zero)
            return ExecutionResult.FromError("Mute duration must not be negative.");

        await targetUser.SetTimeOutAsync(duration, new RequestOptions() { AuditLogReason = muteReason });

        string days = Format.Bold(duration.ToString("%d"));
        string hours = Format.Bold(duration.ToString("%h"));
        string minutes = Format.Bold(duration.ToString("%m"));
        string seconds = Format.Bold(duration.ToString("%s"));

        long untilDate = (DateTimeOffset.Now + duration).ToUnixTimeSeconds();

        EmbedBuilder replyEmbed = new EmbedBuilder().BuildSuccessEmbed(Context);
        
        replyEmbed.Description = $"Successfully timed out user `{targetUser.GetFullUsername()}`.";

        replyEmbed.AddField("\U0001f914 Reason", muteReason);
        replyEmbed.AddField("\u23F1\uFE0F Duration", $"{days} day(s), {hours} hour(s), {minutes} minute(s) and {seconds} second(s).");
        replyEmbed.AddField("\u23F0 Expires In", $"<t:{untilDate}:F>");

        await RespondAsync(embed: replyEmbed.Build(), allowedMentions: Constants.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    [SlashCommand("purge", "Deletes an amount of messages.")]
    [DetailedDescription("Deletes the provided amount of messages.")]
    [RateLimit(2, 2)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(GuildPermission.ManageMessages)]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    public async Task<RuntimeResult> PurgeMessagesAsync
    (
        [Summary("Count", "The amount of messages you want to purge.")]
        int count
    )
    {
        IEnumerable<IMessage> retrievedMessages = await Context.Interaction.Channel.GetMessagesAsync(count + 1).FlattenAsync();

        await (Context.Channel as SocketTextChannel)!.DeleteMessagesAsync(retrievedMessages);

        await RespondAsync($":white_check_mark: Cleared `{count}` message/s.", ephemeral: true, allowedMentions: Constants.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }
}