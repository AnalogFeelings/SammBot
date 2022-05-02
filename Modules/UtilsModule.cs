using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
	[Name("Utils")]
	[Group("utils")]
	[Summary("Moderation commands & misc.")]
	[ModuleEmoji("🔧")]
	public class UtilsModule : ModuleBase<SocketCommandContext>
	{
		[Command("ban")]
		[Alias("toss", "bonk")]
		[Summary("Bans a user with a reason.")]
		[RequireBotPermission(GuildPermission.BanMembers)]
		[RequireUserPermission(GuildPermission.BanMembers)]
		public async Task<RuntimeResult> BanUserAsync(IUser User, int PruneDays, string Reason = null)
		{
			string BanReason = Reason ?? "No reason specified.";

			using (Context.Channel.EnterTypingState())
			{
				await Context.Guild.AddBanAsync(User, PruneDays, BanReason);

				await ReplyAsync($":hammer: **Banned user \"{User.Username}\" from this server.**\n" +
					$"Reason: *{BanReason}*");
			}

			return ExecutionResult.Succesful();
		}

		[Command("kick")]
		[Alias("boot", "exile")]
		[Summary("Kicks a user with a reason.")]
		[RequireBotPermission(GuildPermission.KickMembers)]
		[RequireUserPermission(GuildPermission.KickMembers)]
		public async Task<RuntimeResult> KickUserAsync(IUser User, string Reason = null)
		{
			string KickReason = Reason ?? "No reason specified.";
			IGuildUser TargetUser = Context.Guild.GetUser(User.Id);

			using (Context.Channel.EnterTypingState())
			{
				await TargetUser.KickAsync(KickReason);

				await ReplyAsync($":boot: **Kicked user \"{User.Username}\" from this server.**\n" +
					$"Reason: *{KickReason}*");
			}

			return ExecutionResult.Succesful();
		}

		[Command("avatar")]
		[Alias("pfp", "pic", "userpic")]
		[Summary("Gets the avatar of a user.")]
		public async Task<RuntimeResult> GetProfilePicAsync(IUser User)
		{
			EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle($"{User.Username}'s Profile Picture");

			string UserAvatar = User.GetAvatarUrl(size: 2048);
			if (UserAvatar == null)
				return ExecutionResult.FromError("This user does not have an avatar!");

			ReplyEmbed.ImageUrl = UserAvatar;

			await Context.Channel.SendMessageAsync("", false, ReplyEmbed.Build());

			return ExecutionResult.Succesful();
		}
	}
}
