using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Moderation")]
    [Group("mod")]
    [Summary("Moderation commands like kick, ban, mute, etc.")]
    [ModuleEmoji("\U0001f9d1\u200D\u2696\uFE0F")]
    public class ModerationModule : ModuleBase<SocketCommandContext>
    {
        [Command("ban")]
        [Alias("toss", "bonk")]
        [Summary("Bans a user with a reason.")]
        [FullDescription("Bans a user from the server with the set reason.")]
        [RateLimit(1, 2)]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task<RuntimeResult> BanUserAsync([Summary("The user you want to ban.")] SocketGuildUser TargetUser,
                                                      [Summary("The amount of days the bot will delete.")] int PruneDays,
                                                      [Summary("The reason of the ban.")] string Reason = null)
        {
            string banReason = Reason ?? "No reason specified.";

            await Context.Guild.AddBanAsync(TargetUser, PruneDays, banReason);

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context)
                .WithTitle($"🔨 Banned user {TargetUser.Username}.").WithColor(new Color(255, 0, 0));

            replyEmbed.Description = $"**Reason**: {banReason}\n";
            replyEmbed.Description += $"**Prune Days**: {PruneDays} day(s).";

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync(null, embed: replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("kick")]
        [Alias("boot", "exile")]
        [Summary("Kicks a user with a reason.")]
        [FullDescription("Kicks a user from the server with the set reason.")]
        [RateLimit(1, 2)]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task<RuntimeResult> KickUserAsync([Summary("The user you want to kick.")] SocketGuildUser TargetUser,
                                                       [Summary("The reason of the kick.")] string Reason = null)
        {
            string kickReason = Reason ?? "No reason specified.";

            await TargetUser.KickAsync(kickReason);

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context)
                    .WithTitle($"👢 Kicked user {TargetUser.Username}.").WithColor(new Color(255, 255, 0));

            replyEmbed.Description = $"**Reason**: {kickReason}";

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync(null, embed: replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("warn")]
        [Summary("Warns a user with a reason.")]
        [FullDescription("Warns a user with a reason. Warnings will be stored in the bot's database, and you will be able to list them afterwards.")]
        [RateLimit(1, 2)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task<RuntimeResult> WarnUserAsync([Summary("The user you want to warn.")] SocketGuildUser TargetUser,
                                                       [Summary("The reason of the warn.")] [Remainder] string Reason)
        {
            if (Reason.Length > 512)
                return ExecutionResult.FromError("Warning reason must not exceed 512 characters.");

            using (BotDatabase botDatabase = new BotDatabase())
            {
                Guid newGuid = Guid.NewGuid();

                await botDatabase.UserWarnings.AddAsync(new UserWarning
                {
                    Id = newGuid.ToString(),
                    UserId = TargetUser.Id,
                    GuildId = Context.Guild.Id,
                    Reason = Reason,
                    Date = Context.Message.Timestamp.ToUnixTimeSeconds()
                });

                await botDatabase.SaveChangesAsync();

                EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context)
                    .WithTitle($"⚠️ Warning Issued").WithColor(new Color(255, 205, 77));

                replyEmbed.Description = $"User <@{TargetUser.Id}> has been warned successfully. Details below.\n\n";

                replyEmbed.Description += $"**Reason**: {Reason}\n";
                replyEmbed.Description += $"**Warn ID**: {newGuid}\n\n";

                MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
                AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

                //DM the user about it.
                try
                {
                    EmbedBuilder directMessageEmbed = new EmbedBuilder().BuildDefaultEmbed(Context)
                        .WithTitle($"⚠️ You have been warned in \"{Context.Guild.Name}\".").WithColor(new Color(255, 205, 77));

                    directMessageEmbed.Description = $"**Reason**: {Reason}\n";
                    directMessageEmbed.Description += $"**Warn ID**: {newGuid}\n\n";
                    directMessageEmbed.Description += $"You may see all of your warnings with the `{Settings.Instance.LoadedConfig.BotPrefix}mod warns` command.";

                    await TargetUser.SendMessageAsync(null, embed: directMessageEmbed.Build());
                }
                catch (Exception)
                {
                    replyEmbed.Description += "I could not DM the user about this warning.";
                }

                await ReplyAsync(null, embed: replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);
            }

            return ExecutionResult.Succesful();
        }

        [Command("unwarn")]
        [Summary("Removes a warn from a user.")]
        [FullDescription("Removes the warning with the specified ID.")]
        [RateLimit(1, 2)]
        [RequireContext(ContextType.Guild)]
        public async Task<RuntimeResult> RemoveWarnAsync([Summary("The ID of the warn you want to remove.")] [Remainder] string WarningId)
        {
            using (BotDatabase botDatabase = new BotDatabase())
            {
                List<UserWarning> userWarnings = await botDatabase.UserWarnings.ToListAsync();
                UserWarning specificWarning = userWarnings.SingleOrDefault(x => x.Id == WarningId && x.GuildId == Context.Guild.Id);

                if (specificWarning == default(UserWarning))
                    return ExecutionResult.FromError("There are no warnings with the specified ID.");

                botDatabase.UserWarnings.Remove(specificWarning);

                await botDatabase.SaveChangesAsync();

                MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
                AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
                await ReplyAsync($":white_check_mark: Removed warning \"{WarningId}\" from user <@{specificWarning.UserId}>.", allowedMentions: allowedMentions, messageReference: messageReference);
            }

            return ExecutionResult.Succesful();
        }

        [Command("warns")]
        [Alias("warnlist")]
        [Summary("Lists all of the warns given to a user.")]
        [FullDescription("Replies with a list of warnings given to the specified user.")]
        [RateLimit(2, 1)]
        [RequireContext(ContextType.Guild)]
        public async Task<RuntimeResult> ListWarnsAsync([Summary("The user you want to list the warns for.")] SocketGuildUser TargetUser)
        {
            using (BotDatabase botDatabase = new BotDatabase())
            {
                List<UserWarning> userWarnings = await botDatabase.UserWarnings.ToListAsync();
                List<UserWarning> filteredWarnings = userWarnings.Where(x => x.UserId == TargetUser.Id && x.GuildId == Context.Guild.Id).ToList();

                if (!filteredWarnings.Any())
                    return ExecutionResult.FromError("This user has no warnings.");

                EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle("📃 List of Warnings");

                replyEmbed.Description = "Reasons longer than 48 characters will be truncated.\n\n";

                foreach (UserWarning warning in filteredWarnings)
                {
                    replyEmbed.Description += $"⚠️ **ID**: `{warning.Id}`\n";
                    replyEmbed.Description += $"**· Creation Date**: <t:{warning.Date}:F>\n";
                    replyEmbed.Description += $"**· Reason**: {warning.Reason.Truncate(48)}\n\n";
                }

                MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
                AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
                await ReplyAsync(null, embed: replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);
            }

            return ExecutionResult.Succesful();
        }

        [Command("viewwarn")]
        [Alias("getwarn", "listwarn")]
        [Summary("Lists a specific warn.")]
        [FullDescription("Lists a warning, and the full reason.")]
        [RateLimit(2, 1)]
        [RequireContext(ContextType.Guild)]
        public async Task<RuntimeResult> ListWarnAsync([Summary("The ID of the warn you want to view.")] [Remainder] string WarningId)
        {
            using (BotDatabase botDatabase = new BotDatabase())
            {
                List<UserWarning> userWarnings = await botDatabase.UserWarnings.ToListAsync();
                UserWarning specificWarning = userWarnings.SingleOrDefault(x => x.Id == WarningId && x.GuildId == Context.Guild.Id);

                if (specificWarning == default(UserWarning))
                    return ExecutionResult.FromError("There are no warnings with the specified ID.");

                EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle("Warning Details");

                replyEmbed.Description = $"Details for the warning \"{specificWarning.Id}\".\n";

                replyEmbed.AddField("User", $"<@{specificWarning.UserId}> (ID: {specificWarning.UserId})");
                replyEmbed.AddField("Date", $"<t:{specificWarning.Date}:F>");
                replyEmbed.AddField("Reason", specificWarning.Reason);

                MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
                AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
                await ReplyAsync(null, embed: replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);
            }

            return ExecutionResult.Succesful();
        }

        [Command("mute")]
        [Alias("timeout", "shush", "shutup")]
        [Summary("Mutes a user for an amount of time with a reason.")]
        [FullDescription("Mutes the specified user for an amount of time with the specified reason. The reason is optional.")]
        [RateLimit(1, 2)]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.ModerateMembers)]
        [RequireUserPermission(GuildPermission.ModerateMembers)]
        public async Task<RuntimeResult> MuteUserAsync([Summary("The user you want to mute.")] SocketGuildUser TargetUser,
                                                       [Summary("The duration of the mute.")] TimeSpan Duration,
                                                       [Summary("The reason of the mute.")] string Reason = null)
        {
            string muteReason = Reason ?? "No reason specified.";

            if (Duration < TimeSpan.Zero)
                return ExecutionResult.FromError("Mute duration must not be negative.");

            await TargetUser.SetTimeOutAsync(Duration, new RequestOptions() { AuditLogReason = muteReason });

            string days = Format.Bold(Duration.ToString("%d"));
            string hours = Format.Bold(Duration.ToString("%h"));
            string minutes = Format.Bold(Duration.ToString("%m"));
            string seconds = Format.Bold(Duration.ToString("%s"));

            long untilDate = (DateTimeOffset.Now + Duration).ToUnixTimeSeconds();

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context)
                    .WithTitle($"⏱️ Timed out user {TargetUser.Username}.").WithColor(Color.LightGrey);

            replyEmbed.Description = $"**Reason**: {Reason}\n";
            replyEmbed.Description += $"**Duration**: {days} day(s), {hours} hour(s), {minutes} minute(s) and {seconds} second(s).\n";
            replyEmbed.Description += $"**Expires in**: <t:{untilDate}:F>";

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync(null, embed: replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("purge")]
        [Alias("clean", "clear")]
        [Summary("Deletes an amount of messages.")]
        [FullDescription("Deletes the provided amount of messages.")]
        [RateLimit(2, 2)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task<RuntimeResult> PurgeMessagesAsync([Summary("The amount of messages you want to purge.")] int Count)
        {
            IEnumerable<IMessage> retrievedMessages = await Context.Message.Channel.GetMessagesAsync(Count + 1).FlattenAsync();

            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(retrievedMessages);

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            IUserMessage successMessage = await ReplyAsync($":white_check_mark: Cleared `{Count}` message/s.", allowedMentions: allowedMentions, messageReference: messageReference);

            await Task.Delay(3000);
            await successMessage.DeleteAsync();

            return ExecutionResult.Succesful();
        }
    }
}
