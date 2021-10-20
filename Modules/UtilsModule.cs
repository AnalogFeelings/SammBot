using Discord;
using Discord.Commands;
using SammBotNET.Extensions;
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
        public async Task<RuntimeResult> BanUserAsync(IUser user, int pruneDays, string reason = null)
        {
            string banReason = reason == null ? "No reason specified." : reason;
            using (Context.Channel.EnterTypingState())
            {
                await Context.Guild.AddBanAsync(user, pruneDays, banReason);

                await ReplyAsync($":hammer: **Banned user \"{user.Username}\" from this server.**\n" +
                    $"Reason: *{banReason}*");
            }

            return ExecutionResult.Succesful();
        }

        [Command("kick")]
        [Alias("boot", "exile")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [Summary("Kicks a user with a reason.")]
        public async Task<RuntimeResult> KickUserAsync(IUser user, string reason = null)
        {
            string kickReason = reason == null ? "No reason specified." : reason;
            IGuildUser guildUser = Context.Guild.GetUser(user.Id);

            using (Context.Channel.EnterTypingState())
            {
                await guildUser.KickAsync(kickReason);

                await ReplyAsync($":boot: **Kicked user \"{user.Username}\" from this server.**\n" +
                    $"Reason: *{kickReason}*");
            }

            return ExecutionResult.Succesful();
        }

        [Command("avatar")]
        [Alias("pfp", "pic", "userpic")]
        [Summary("Gets the avatar of a user.")]
        public async Task<RuntimeResult> GetProfilePicAsync(IUser user)
        {
            EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle($"{user.Username}'s Profile Picture");

            string avatarUrl = user.GetAvatarUrl(size: 2048);
            if (avatarUrl == null)
                return ExecutionResult.FromError("This user does not have an avatar!");

            embed.ImageUrl = avatarUrl;

            await Context.Channel.SendMessageAsync("", false, embed.Build());

            return ExecutionResult.Succesful();
        }
    }
}
