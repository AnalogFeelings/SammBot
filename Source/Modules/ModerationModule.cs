using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
	[Name("Moderation")]
	[Group("mod")]
	[Summary("Moderation commands like kick, ban, mute...")]
	[ModuleEmoji("🧑‍⚖️")]
	public class ModerationModule : ModuleBase<SocketCommandContext>
	{
		[Command("ban")]
		[Alias("toss", "bonk")]
		[Summary("Bans a user with a reason.")]
		[FullDescription("Bans a user from the server with the set reason.")]
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

		[Command("mute")]
		[Alias("timeout", "shush", "shutup")]
		[Summary("Mutes a user for an amount of time with a reason.")]
		[FullDescription("Mutes the specified user for an amount of time with the specified reason. The reason is optional.")]
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
