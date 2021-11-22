using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Utils")]
    [Group("utils")]
    [Summary("Moderation commands & misc.")]
    public class UtilsModule : ModuleBase<SocketCommandContext>
    {
        [Command("ban")]
        [Alias("toss", "bonk")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [Summary("Bans a user with a reason.")]
        public async Task<RuntimeResult> BanUserAsync(IUser User, int PruneDays, string Reason = null)
        {
            string banReason = Reason ?? "No reason specified.";
            using (Context.Channel.EnterTypingState())
            {
                await Context.Guild.AddBanAsync(User, PruneDays, banReason);

                await ReplyAsync($":hammer: **Banned user \"{User.Username}\" from this server.**\n" +
                    $"Reason: *{banReason}*");
            }

            return ExecutionResult.Succesful();
        }

        [Command("kick")]
        [Alias("boot", "exile")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [Summary("Kicks a user with a reason.")]
        public async Task<RuntimeResult> KickUserAsync(IUser User, string Reason = null)
        {
            string kickReason = Reason ?? "No reason specified.";
            IGuildUser guildUser = Context.Guild.GetUser(User.Id);

            using (Context.Channel.EnterTypingState())
            {
                await guildUser.KickAsync(kickReason);

                await ReplyAsync($":boot: **Kicked user \"{User.Username}\" from this server.**\n" +
                    $"Reason: *{kickReason}*");
            }

            return ExecutionResult.Succesful();
        }

        [Command("avatar")]
        [Alias("pfp", "pic", "userpic")]
        [Summary("Gets the avatar of a user.")]
        public async Task<RuntimeResult> GetProfilePicAsync(IUser User)
        {
            EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle($"{User.Username}'s Profile Picture");

            string avatarUrl = User.GetAvatarUrl(size: 2048);
            if (avatarUrl == null)
                return ExecutionResult.FromError("This user does not have an avatar!");

            embed.ImageUrl = avatarUrl;

            await Context.Channel.SendMessageAsync("", false, embed.Build());

            return ExecutionResult.Succesful();
        }
    }
}
