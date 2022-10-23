using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using SammBot.Bot.Classes;
using SammBot.Bot.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SammBot.Bot.Modules
{
    [Name("Information")]
    [Group("info")]
    [Summary("Bot information and statistics.")]
    [ModuleEmoji("\u2139")]
    public class InformationModule : ModuleBase<SocketCommandContext>
    {
        public DiscordShardedClient ShardedClient { get; set; }
        
        [Command("full")]
        [Summary("Shows the FULL information of the bot.")]
        [FullDescription("Shows version, uptime, ping, etc...")]
        [RateLimit(3, 1)]
        public async Task<RuntimeResult> InformationFullAsync()
        {
            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, "Information");

            string elapsedUptime = string.Format("{0:00} days,\n{1:00} hours,\n{2:00} minutes",
                Settings.Instance.RuntimeStopwatch.Elapsed.Days,
                Settings.Instance.RuntimeStopwatch.Elapsed.Hours,
                Settings.Instance.RuntimeStopwatch.Elapsed.Minutes);
            long memoryUsage = Process.GetCurrentProcess().PrivateMemorySize64 / 1000000;

            string releaseConfig = "Unknown";
#if DEBUG
            releaseConfig = "Debug";
#elif RELEASE
            releaseConfig = "Release";
#endif
            
            string formattedWebsite = Format.Url("website", "https://aestheticalz.github.io/SammBot/");
            string formattedGithub = Format.Url("repository", "https://github.com/AestheticalZ/SammBot");

            replyEmbed.Title = "\u2139\uFE0F Bot Information";
            replyEmbed.WithColor(59, 136, 195);

            replyEmbed.Description += $"Here's some public information about {Settings.BOT_NAME}!\n\n";
            replyEmbed.Description += $":globe_with_meridians: Check out the bot's {formattedWebsite}!\n";
            replyEmbed.Description += $":open_file_folder: Also check out the GitHub {formattedGithub}!";

            replyEmbed.AddField("\U0001faaa Bot Version", $"Version {Settings.GetBotVersion()}", true);
            replyEmbed.AddField("\u2699\uFE0F Target Config", $"{releaseConfig} Configuration", true);
            replyEmbed.AddField("\U0001f4e6 .NET Version", $"{RuntimeInformation.FrameworkDescription}", true);

            replyEmbed.AddField("\U0001f4e1 Ping", $"{Context.Client.Latency} milliseconds", true);
            replyEmbed.AddField("\U0001f5c2\uFE0F Server Count", $"{Context.Client.Guilds.Count} server(s)", true);
            replyEmbed.AddField("\U0001f9f1 Shard Count", $"{ShardedClient.Shards.Count} shard(s)", true);
            
            replyEmbed.AddField("\u23F1\uFE0F Bot Uptime", elapsedUptime, true);
            replyEmbed.AddField("\U0001f5a5\uFE0F Host System", FriendlyOSName(), true);
            replyEmbed.AddField("\U0001f4ca Working Set", $"{memoryUsage} megabytes", true);

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync(null, false, replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("serverinfo")]
        [Alias("server", "guildinfo")]
        [Summary("Get information about a server!")]
        [FullDescription("Gets all the information about the server you execute the command in.")]
        [RateLimit(3, 1)]
        [RequireContext(ContextType.Guild)]
        public async Task<RuntimeResult> ServerInfoAsync()
        {
            RestUser serverOwner = await Context.Client.Rest.GetUserAsync(Context.Guild.OwnerId);

            string serverBanner = Context.Guild.BannerUrl != null ? $"[Banner URL]({Context.Guild.BannerUrl})" : "None";
            string discoverySplash = Context.Guild.DiscoverySplashUrl != null ? $"[Splash URL]({Context.Guild.DiscoverySplashId})" : "None";
            int channelCount = Context.Guild.Channels.Count;
            int emoteCount = Context.Guild.Emotes.Count;
            int memberCount = Context.Guild.MemberCount;
            int boostTier = (int)Context.Guild.PremiumTier;
            int roleCount = Context.Guild.Roles.Count;
            string boostCount = Context.Guild.PremiumSubscriptionCount != 0 ? Context.Guild.PremiumSubscriptionCount.ToString() : "No Boosts";
            string creationDate = $"<t:{Context.Guild.CreatedAt.ToUnixTimeSeconds()}>";
            string serverName = Context.Guild.Name;
            string serverOwnerName = $"{serverOwner.Username}#{serverOwner.Discriminator}";

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle("GUILD INFORMATION");

            if (Context.Guild.IconUrl != null) replyEmbed.WithThumbnailUrl(Context.Guild.IconUrl);
            replyEmbed.AddField("Name", serverName, true);
            replyEmbed.AddField("Owner", serverOwnerName, true);
            replyEmbed.AddField("Banner", serverBanner, true);
            replyEmbed.AddField("Discovery Splash", discoverySplash, true);
            replyEmbed.AddField("Nitro Boosts", boostCount, true);
            replyEmbed.AddField("Nitro Tier", boostTier, true);
            replyEmbed.AddField("Created At", creationDate, true);
            replyEmbed.AddField("Channel Count", channelCount, true);
            replyEmbed.AddField("Emote Count", emoteCount, true);
            replyEmbed.AddField("Member Count", memberCount, true);
            replyEmbed.AddField("Role Count", roleCount, true);

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync(null, false, replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("userinfo")]
        [Alias("user")]
        [Summary("Get information about a user!")]
        [FullDescription("Gets all the information about the provided user.")]
        [RateLimit(3, 1)]
        [RequireContext(ContextType.Guild)]
        public async Task<RuntimeResult> UserInfoAsync([Summary("The user you want to get the information from.")] SocketGuildUser User = null)
        {
            SocketGuildUser targetUser = User ?? Context.Message.Author as SocketGuildUser;

            string userAvatar = targetUser.GetAvatarOrDefault(2048);
            string userName = $"{targetUser.Username}";
            string userDiscriminator = $"#{targetUser.Discriminator}";
            string userNickname = targetUser.Nickname ?? "None";
            string isBot = targetUser.IsBot.ToYesNo();
            string isWebhook = targetUser.IsWebhook.ToYesNo();
            string joinDate = $"<t:{targetUser.JoinedAt.Value.ToUnixTimeSeconds()}>";
            string signUpDate = $"<t:{targetUser.CreatedAt.ToUnixTimeSeconds()}>";
            string boostingSince = targetUser.PremiumSince != null ? $"<t:{targetUser.PremiumSince.Value.ToUnixTimeSeconds()}:R>" : "Never";
            string roles = targetUser.Roles.Count > 1 ?
                string.Join(", ", targetUser.Roles.Skip(1).Select(x => $"<@&{x.Id}>")).Truncate(512)
                : "None";
            string onlineStatus = targetUser.GetStatusString();

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle("USER INFORMATION");

            replyEmbed.WithThumbnailUrl(userAvatar);
            replyEmbed.AddField("Username", userName, true);
            replyEmbed.AddField("Nickname", userNickname, true);
            replyEmbed.AddField("Discriminator", userDiscriminator, true);
            replyEmbed.AddField("Status", onlineStatus, true);
            replyEmbed.AddField("Is Bot", isBot, true);
            replyEmbed.AddField("Is Webhook", isWebhook, true);
            replyEmbed.AddField("Join Date", joinDate, true);
            replyEmbed.AddField("Create Date", signUpDate, true);
            replyEmbed.AddField("Booster Since", boostingSince, true);
            replyEmbed.AddField("Roles", roles, false);

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync(null, false, replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        public string FriendlyOSName()
        {
            string osName = string.Empty;
            Version version = Environment.OSVersion.Version;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
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
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                switch (version.Major)
                {
                    case 10:
                        switch (version.Minor)
                        {
                            case 15:
                                osName = "macOS Catalina";
                                break;
                            default:
                                osName = "Unknown macOS";
                                break;
                        }
                        break;
                    case 11:
                        osName = "macOS Big Sur";
                        break;
                    case 12:
                        osName = "macOS Monterey";
                        break;
                    case 13:
                        osName = "macOS Ventura";
                        break;
                    default:
                        osName = "Unknown macOS";
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
            else
            {
                osName = "Unknown OS";
            }

            return osName;
        }
    }
}