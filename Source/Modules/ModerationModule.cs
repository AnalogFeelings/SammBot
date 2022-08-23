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
	[ModuleEmoji("🧑‍⚖️")]
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
		public async Task<RuntimeResult> BanUserAsync(SocketGuildUser TargetUser, int PruneDays, string Reason = null)
		{
			string BanReason = Reason ?? "No reason specified.";

			await Context.Guild.AddBanAsync(TargetUser, PruneDays, BanReason);

			EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context)
				.WithTitle($"🔨 Banned user {TargetUser.Username}.").WithColor(new Color(255, 0, 0));

			ReplyEmbed.Description = $"**Reason**: {BanReason}\n";
			ReplyEmbed.Description += $"**Prune Days**: {PruneDays} day(s).";

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync(null, embed: ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);

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
		public async Task<RuntimeResult> KickUserAsync(SocketGuildUser TargetUser, string Reason = null)
		{
			string KickReason = Reason ?? "No reason specified.";

			await TargetUser.KickAsync(KickReason);

			EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context)
					.WithTitle($"👢 Kicked user {TargetUser.Username}.").WithColor(new Color(255, 255, 0));

			ReplyEmbed.Description = $"**Reason**: {KickReason}";

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync(null, embed: ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("warn")]
		[Summary("Warns a user with a reason.")]
		[FullDescription("Warns a user with a reason. Warnings will be stored in the bot's database, and you will be able to list them afterwards.")]
		[RateLimit(1, 2)]
		[RequireContext(ContextType.Guild)]
		[RequireUserPermission(GuildPermission.KickMembers)]
		public async Task<RuntimeResult> WarnUserAsync(SocketGuildUser TargetUser, [Remainder] string Reason)
		{
			if (Reason.Length > 512)
				return ExecutionResult.FromError("Warning reason must not exceed 512 characters.");

			using (BotDatabase BotDatabase = new BotDatabase())
			{
				Guid NewId = Guid.NewGuid();

				await BotDatabase.UserWarnings.AddAsync(new UserWarning
				{
					Id = NewId.ToString(),
					UserId = TargetUser.Id,
					GuildId = Context.Guild.Id,
					Reason = Reason,
					Date = Context.Message.Timestamp.ToUnixTimeSeconds()
				});

				await BotDatabase.SaveChangesAsync();

				EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context)
					.WithTitle($"⚠️ Warning Issued").WithColor(new Color(255, 205, 77));

				ReplyEmbed.Description = $"User <@{TargetUser.Id}> has been warned successfully. Details below.\n\n";

				ReplyEmbed.Description += $"**Reason**: {Reason}\n";
				ReplyEmbed.Description += $"**Warn ID**: {NewId}\n\n";

				MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
				AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

				//DM the user about it.
				try
				{
					EmbedBuilder DirectMessageEmbed = new EmbedBuilder().BuildDefaultEmbed(Context)
						.WithTitle($"⚠️ You have been warned in \"{Context.Guild.Name}\".").WithColor(new Color(255, 205, 77));

					DirectMessageEmbed.Description = $"**Reason**: {Reason}\n";
					DirectMessageEmbed.Description += $"**Warn ID**: {NewId}\n\n";
					DirectMessageEmbed.Description += $"You may see all of your warnings with the `{Settings.Instance.LoadedConfig.BotPrefix}mod warns` command.";

					await TargetUser.SendMessageAsync(null, embed: DirectMessageEmbed.Build());
				}
				catch(Exception)
				{
					ReplyEmbed.Description += "I could not DM the user about this warning.";
				}

				await ReplyAsync(null, embed: ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);
			}

			return ExecutionResult.Succesful();
		}

		[Command("unwarn")]
		[Summary("Removes a warn from a user.")]
		[FullDescription("Removes the warning with the specified ID.")]
		[RateLimit(1, 2)]
		[RequireContext(ContextType.Guild)]
		public async Task<RuntimeResult> RemoveWarnAsync([Remainder] string WarningId)
		{
			using (BotDatabase BotDatabase = new BotDatabase())
			{
				List<UserWarning> Warnings = await BotDatabase.UserWarnings.ToListAsync();
				UserWarning SpecificWarning = Warnings.SingleOrDefault(x => x.Id == WarningId && x.GuildId == Context.Guild.Id);

				if (SpecificWarning == default(UserWarning))
					return ExecutionResult.FromError("There are no warnings with the specified ID.");

				BotDatabase.UserWarnings.Remove(SpecificWarning);

				await BotDatabase.SaveChangesAsync();

				MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
				AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
				await ReplyAsync($":white_check_mark: Removed warning \"{WarningId}\" from user <@{SpecificWarning.UserId}>.", allowedMentions: AllowedMentions, messageReference: Reference);
			}

			return ExecutionResult.Succesful();
		}

		[Command("warns")]
		[Alias("warnlist")]
		[Summary("Lists all of the warns given to a user.")]
		[FullDescription("Replies with a list of warnings given to the specified user.")]
		[RateLimit(2, 1)]
		[RequireContext(ContextType.Guild)]
		public async Task<RuntimeResult> ListWarnsAsync(SocketGuildUser TargetUser)
		{
			using (BotDatabase BotDatabase = new BotDatabase())
			{
				List<UserWarning> Warnings = await BotDatabase.UserWarnings.ToListAsync();
				List<UserWarning> FilteredWarnings = Warnings.Where(x => x.UserId == TargetUser.Id && x.GuildId == Context.Guild.Id).ToList();

				if (!FilteredWarnings.Any())
					return ExecutionResult.FromError("This user has no warnings.");

				EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle("📃 List of Warnings");

				ReplyEmbed.Description = "Reasons longer than 48 characters will be truncated.\n\n";

				foreach (UserWarning Warning in FilteredWarnings)
				{
					ReplyEmbed.Description += $"⚠️ **ID**: `{Warning.Id}`\n";
					ReplyEmbed.Description += $"**· Creation Date**: <t:{Warning.Date}:F>\n";
					ReplyEmbed.Description += $"**· Reason**: {Warning.Reason.Truncate(48)}\n\n";
				}

				MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
				AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
				await ReplyAsync(null, embed: ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);
			}

			return ExecutionResult.Succesful();
		}

		[Command("viewwarn")]
		[Alias("getwarn", "listwarn")]
		[Summary("Lists a specific warn.")]
		[FullDescription("Lists a warning, and the full reason.")]
		[RateLimit(2, 1)]
		[RequireContext(ContextType.Guild)]
		public async Task<RuntimeResult> ListWarnAsync([Remainder] string WarningId)
		{
			using (BotDatabase BotDatabase = new BotDatabase())
			{
				List<UserWarning> Warnings = await BotDatabase.UserWarnings.ToListAsync();
				UserWarning SpecificWarning = Warnings.SingleOrDefault(x => x.Id == WarningId && x.GuildId == Context.Guild.Id);

				if (SpecificWarning == default(UserWarning))
					return ExecutionResult.FromError("There are no warnings with the specified ID.");

				EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle("Warning Details");

				ReplyEmbed.Description = $"Details for the warning \"{SpecificWarning.Id}\".\n";

				ReplyEmbed.AddField("User", $"<@{SpecificWarning.UserId}> (ID: {SpecificWarning.UserId})");
				ReplyEmbed.AddField("Date", $"<t:{SpecificWarning.Date}:F>");
				ReplyEmbed.AddField("Reason", SpecificWarning.Reason);

				MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
				AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
				await ReplyAsync(null, embed: ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);
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
		public async Task<RuntimeResult> MuteUserAsync(SocketGuildUser TargetUser, TimeSpan Duration, string Reason = null)
		{
			string MuteReason = Reason ?? "No reason specified.";

			if (Duration < TimeSpan.Zero)
				return ExecutionResult.FromError("Mute duration must not be negative.");

			await TargetUser.SetTimeOutAsync(Duration, new RequestOptions() { AuditLogReason = MuteReason });

			string Days = Format.Bold(Duration.ToString("%d"));
			string Hours = Format.Bold(Duration.ToString("%h"));
			string Minutes = Format.Bold(Duration.ToString("%m"));
			string Seconds = Format.Bold(Duration.ToString("%s"));

			long UntilDate = (DateTimeOffset.Now + Duration).ToUnixTimeSeconds();

			EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context)
					.WithTitle($"⏱️ Timed out user {TargetUser.Username}.").WithColor(Color.LightGrey);

			ReplyEmbed.Description = $"**Reason**: {Reason}\n";
			ReplyEmbed.Description += $"**Duration**: {Days} day(s), {Hours} hour(s), {Minutes} minute(s) and {Seconds} second(s).\n";
			ReplyEmbed.Description += $"**Expires in**: <t:{UntilDate}:F>";

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync(null, embed: ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("purge")]
		[Alias("clean", "clear")]
		[Summary("Deletes an amount of messages.")]
		[FullDescription("Deletes the provided amount of messages.")]
		[RateLimit(2, 2)]
		[RequireBotPermission(GuildPermission.ManageMessages)]
		[RequireUserPermission(GuildPermission.ManageMessages)]
		public async Task<RuntimeResult> PurgeMessagesAsync(int Count)
		{
			IEnumerable<IMessage> RetrievedMessages = await Context.Message.Channel.GetMessagesAsync(Count + 1).FlattenAsync();

			await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(RetrievedMessages);

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			IUserMessage SuccessMessage = await ReplyAsync($":white_check_mark: Cleared `{Count}` message/s.", allowedMentions: AllowedMentions, messageReference: Reference);

			await Task.Delay(3000);
			await SuccessMessage.DeleteAsync();

			return ExecutionResult.Succesful();
		}
	}
}
