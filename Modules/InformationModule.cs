using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using SammBotNET.Core;
using SammBotNET.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Information")]
    [Summary("Bot information and statistics.")]
    [Group("info")]
    public class InformationModule : ModuleBase<SocketCommandContext>
    {
        public DiscordSocketClient Client { get; set; }

        [Command("full")]
        [Summary("Shows the FULL information of the bot.")]
        public async Task<RuntimeResult> InformationFullAsync()
        {
            EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context, "Information", "All public information about the bot.");

            string elapsedTime = string.Format("{0:00}d{1:00}h{2:00}m",
                GlobalConfig.Instance.RuntimeStopwatch.Elapsed.Days,
                GlobalConfig.Instance.RuntimeStopwatch.Elapsed.Hours,
                GlobalConfig.Instance.RuntimeStopwatch.Elapsed.Minutes);

            embed.AddField("Bot Version", $"`{GlobalConfig.Instance.LoadedConfig.BotVersion}`", true);
            embed.AddField(".NET Version", $"`{RuntimeInformation.FrameworkDescription}`", true);
            embed.AddField("Ping", $"`{Context.Client.Latency}ms.`", true);
            embed.AddField("Im In", $"`{Context.Client.Guilds.Count} server/s.`", true);
            embed.AddField("Uptime", $"`{elapsedTime}`", true);
            embed.AddField("Host", $"`{FriendlyOSName()}`", true);

            await Context.Channel.SendMessageAsync("", false, embed.Build());

            return ExecutionResult.Succesful();
        }

        [Command("servers")]
        [Alias("guilds")]
        [Summary("Shows a list of all the servers the bot is in.")]
        public async Task<RuntimeResult> ServersAsync()
        {
            if (Context.User.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            string builtMsg = "I am invited in the following servers:\n```\n";
            string inside = string.Empty;

            int i = 1;
            foreach (SocketGuild guild in Context.Client.Guilds)
            {
                inside += $"{i}. {guild.Name} (ID {guild.Id})\n";
                i++;
            }
            inside += "```";
            builtMsg += inside;
            await ReplyAsync(builtMsg);

            return ExecutionResult.Succesful();
        }

        [Command("serverinfo")]
        [Alias("server", "guildinfo")]
        [MustRunInGuild]
        [Summary("Get information about a server!")]
        public async Task<RuntimeResult> ServerInfoAsync()
        {
            RestUser ownerRestUser = await Client.Rest.GetUserAsync(Context.Guild.OwnerId);

            string bannerUrl = Context.Guild.BannerUrl != null ? $"[Banner URL]({Context.Guild.BannerUrl})" : "None";
            string discoverySplashUrl = Context.Guild.DiscoverySplashUrl != null ? $"[Splash URL]({Context.Guild.DiscoverySplashId})" : "None";
            int channelCount = Context.Guild.Channels.Count;
            int emoteCount = Context.Guild.Emotes.Count;
            int memberCount = Context.Guild.MemberCount;
            int nitroTier = (int)Context.Guild.PremiumTier;
            int roleCount = Context.Guild.Roles.Count;
            string nitroBoosts = Context.Guild.PremiumSubscriptionCount != 0 ? Context.Guild.PremiumSubscriptionCount.ToString() : "No Boosts";
            string createDate = $"<t:{Context.Guild.CreatedAt.ToUnixTimeSeconds()}>";
            string guildName = Context.Guild.Name;
            string guildOwner = $"{ownerRestUser.Username}#{ownerRestUser.Discriminator}";

            EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle("GUILD INFORMATION");

            if (Context.Guild.IconUrl != null) embed.WithThumbnailUrl(Context.Guild.IconUrl);
            embed.AddField("Name", guildName, true);
            embed.AddField("Owner", guildOwner, true);
            embed.AddField("Banner", bannerUrl, true);
            embed.AddField("Discovery Splash", discoverySplashUrl, true);
            embed.AddField("Nitro Boosts", nitroBoosts, true);
            embed.AddField("Nitro Tier", nitroTier, true);
            embed.AddField("Created At", createDate, true);
            embed.AddField("Channel Count", channelCount, true);
            embed.AddField("Emote Count", emoteCount, true);
            embed.AddField("Member Count", memberCount, true);
            embed.AddField("Role Count", roleCount, true);

            await Context.Channel.SendMessageAsync(null, false, embed.Build());

            return ExecutionResult.Succesful();
        }

        [Command("userinfo")]
        [Alias("user")]
        [MustRunInGuild]
        [Summary("Get information about a user!")]
        public async Task<RuntimeResult> UserInfoAsync(SocketGuildUser User = null)
        {
            SocketGuildUser userHolder = User ?? Context.Message.Author as SocketGuildUser;

            string userAvatarUrl = userHolder.GetAvatarOrDefault();
            string userName = $"{userHolder.Username}";
            string userDiscriminator = $"#{userHolder.Discriminator}";
            string nickName = userHolder.Nickname ?? "None";
            string isABot = userHolder.IsBot.ToYesNo();
            string isAWebhook = userHolder.IsWebhook.ToYesNo();
            string joinDate = $"<t:{userHolder.JoinedAt.Value.ToUnixTimeSeconds()}>";
            string createDate = $"<t:{userHolder.CreatedAt.ToUnixTimeSeconds()}>";
            string boostingSince = userHolder.PremiumSince != null ? $"<t:{userHolder.PremiumSince.Value.ToUnixTimeSeconds()}:R>" : "Never";
            string userRoles = userHolder.Roles.Count > 1 ?
                string.Join(", ", userHolder.Roles.Skip(1).Select(x => $"<@&{x.Id}>")).Truncate(512)
                : "None";
            string userStatus = userHolder.GetStatusString();

            EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle("USER INFORMATION");

            embed.WithThumbnailUrl(userAvatarUrl);
            embed.AddField("Username", userName, true);
            embed.AddField("Nickname", nickName, true);
            embed.AddField("Discriminator", userDiscriminator, true);
            embed.AddField("Status", userStatus, true);
            embed.AddField("Is Bot", isABot, true);
            embed.AddField("Is Webhook", isAWebhook, true);
            embed.AddField("Join Date", joinDate, true);
            embed.AddField("Create Date", createDate, true);
            embed.AddField("Booster Since", boostingSince, true);
            embed.AddField("Roles", userRoles, false);

            await Context.Channel.SendMessageAsync(null, false, embed.Build());

            return ExecutionResult.Succesful();
        }

        public string FriendlyOSName()
        {
            string osName = string.Empty;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Version version = Environment.OSVersion.Version;

                switch (version.Major)
                {
                    case 6:
                        osName = version.Minor switch
                        {
                            1 => "Windows 7",
                            2 => "Windows 8",
                            3 => "Windows 8.1",
                            _ => "Unknown Windows",
                        };
                        break;
                    case 10:
                        switch (version.Minor)
                        {
                            case 0:
                                if (version.Build >= 22000) osName = "Windows 11";
                                else osName = "Windows 10";

                                break;
                            default: osName = "Unknown Windows"; break;
                        }
                        break;
                    default:
                        osName = "Unknown Windows";
                        break;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (File.Exists("/etc/issue.net"))
                    osName = File.ReadAllText("/etc/issue.net");
                else
                    osName = "Linux";
            }

            return osName;
        }
    }
}