#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2023 AestheticalZ
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
#endregion

using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord.Interactions;
using SammBot.Bot.Common;
using SammBot.Bot.Common.Attributes;
using SammBot.Bot.Common.Preconditions;
using SammBot.Bot.Core;
using SammBot.Bot.Extensions;

namespace SammBot.Bot.Modules;

[PrettyName("Information")]
[Group("info", "Bot information and statistics.")]
[ModuleEmoji("\u2139")]
public class InformationModule : InteractionModuleBase<ShardedInteractionContext>
{
    public DiscordShardedClient ShardedClient { get; set; }
        
    [SlashCommand("bot", "Shows information about the bot.")]
    [DetailedDescription("Shows information about the bot such as version, uptime, ping, etc...")]
    [RateLimit(3, 1)]
    public async Task<RuntimeResult> InformationFullAsync()
    {
        EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, "Information");

        string elapsedUptime = string.Format("{0:00} days,\n{1:00} hours,\n{2:00} minutes",
            BotGlobals.Instance.RuntimeStopwatch.Elapsed.Days,
            BotGlobals.Instance.RuntimeStopwatch.Elapsed.Hours,
            BotGlobals.Instance.RuntimeStopwatch.Elapsed.Minutes);
        long memoryUsage = Process.GetCurrentProcess().PrivateMemorySize64 / 1000000;

        string releaseConfig;
#if DEBUG
        releaseConfig = "Debug";
#elif RELEASE
            releaseConfig = "Release";
#endif
            
        string formattedWebsite = Format.Url("website", "https://aestheticalz.github.io/SammBot/");
        string formattedGithub = Format.Url("repository", "https://github.com/AestheticalZ/SammBot");

        replyEmbed.Title = "\u2139\uFE0F Bot Information";
        replyEmbed.WithColor(59, 136, 195);

        replyEmbed.Description += $"Here's some public information about {SettingsManager.BOT_NAME}!\n\n";
        replyEmbed.Description += $":globe_with_meridians: Check out the bot's {formattedWebsite}!\n";
        replyEmbed.Description += $":open_file_folder: Also check out the GitHub {formattedGithub}!";

        replyEmbed.AddField("\U0001faaa Bot Version", $"Version {SettingsManager.GetBotVersion()}", true);
        replyEmbed.AddField("\u2699\uFE0F Target Config", $"{releaseConfig} Configuration", true);
        replyEmbed.AddField("\U0001f4e6 .NET Version", $"{RuntimeInformation.FrameworkDescription}", true);

        replyEmbed.AddField("\U0001f4e1 Ping", $"{Context.Client.Latency} milliseconds", true);
        replyEmbed.AddField("\U0001f5c2\uFE0F Server Count", $"{Context.Client.Guilds.Count} server(s)", true);
        replyEmbed.AddField("\U0001f9f1 Shard Count", $"{ShardedClient.Shards.Count} shard(s)", true);
            
        replyEmbed.AddField("\u23F1\uFE0F Bot Uptime", elapsedUptime, true);
        replyEmbed.AddField("\U0001f5a5\uFE0F Host System", FriendlySystemName(), true);
        replyEmbed.AddField("\U0001f4ca Working Set", $"{memoryUsage} megabytes", true);

        await RespondAsync(embed: replyEmbed.Build(), allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    [SlashCommand("serverinfo", "Get information about a server!")]
    [DetailedDescription("Gets all the information about the server you execute the command in.")]
    [RateLimit(3, 1)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> ServerInfoAsync()
    {
        RestUser serverOwner = await Context.Client.Rest.GetUserAsync(Context.Guild.OwnerId);

        string serverBanner = Context.Guild.BannerUrl != null ? $"[Banner URL]({Context.Guild.BannerUrl})" : "None";
        string discoverySplash = Context.Guild.DiscoverySplashUrl != null ? $"[Splash URL]({Context.Guild.DiscoverySplashId})" : "None";
        int channelCount = Context.Guild.Channels.Count;
        int emoteCount = Context.Guild.Emotes.Count;
        int stickerCount = Context.Guild.Stickers.Count;
        int memberCount = Context.Guild.MemberCount;
        int boostTier = (int)Context.Guild.PremiumTier;
        int roleCount = Context.Guild.Roles.Count;
        string boostCount = Context.Guild.PremiumSubscriptionCount != 0 ? Context.Guild.PremiumSubscriptionCount.ToString() : "No Boosts";
        string creationDate = $"<t:{Context.Guild.CreatedAt.ToUnixTimeSeconds()}>";
        string serverName = Context.Guild.Name;
        string serverOwnerName = $"{serverOwner.Username}#{serverOwner.Discriminator}";

        EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle("GUILD INFORMATION");

        replyEmbed.Title = "\u2139\uFE0F Server Information";
        replyEmbed.Description = "Here's some information about the current server.";
        replyEmbed.WithColor(59, 136, 195);

        if (Context.Guild.IconUrl != null) replyEmbed.WithThumbnailUrl(Context.Guild.IconUrl);
        replyEmbed.AddField("\U0001faaa Name", serverName, true);
        replyEmbed.AddField("\U0001fac5 Owner", serverOwnerName, true);
        replyEmbed.AddField("\U0001f5bc\uFE0F Banner", serverBanner, true);
        replyEmbed.AddField("\U0001faa7 Discovery Splash", discoverySplash, true);
        replyEmbed.AddField("\U0001f680 Nitro Boosts", boostCount, true);
        replyEmbed.AddField("\U0001f3c6 Nitro Tier", boostTier, true);
        replyEmbed.AddField("\U0001f4c6 Created At", creationDate, true);
        replyEmbed.AddField("\U0001f4e2 Channel Count", channelCount, true);
        replyEmbed.AddField("\U0001f642 Emote Count", emoteCount, true);
        replyEmbed.AddField("\U0001f5fd Sticker Count", stickerCount, true);
        replyEmbed.AddField("\U0001f465 Member Count", memberCount, true);
        replyEmbed.AddField("\U0001f4e6 Role Count", roleCount, true);

        await RespondAsync(embed: replyEmbed.Build(), allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    [SlashCommand("userinfo", "Get information about a user!")]
    [DetailedDescription("Gets all the information about the provided user.")]
    [RateLimit(3, 1)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> UserInfoAsync([Summary(description: "The user you want to get the information from.")] SocketGuildUser? User = null)
    {
        SocketGuildUser targetUser = User ?? (Context.Interaction.User as SocketGuildUser)!;

        string userAvatar = targetUser.GetAvatarOrDefault(2048);
        string userName = targetUser.GetFullUsername();
        string userId = targetUser.Id.ToString();
        string userNickname = targetUser.Nickname ?? "None";
        string isBot = targetUser.IsBot.ToYesNo();
        string isOwner = (Context.Guild.Owner.Id == targetUser.Id).ToYesNo();
        string joinDate = $"<t:{targetUser.JoinedAt.Value.ToUnixTimeSeconds()}>"; // ???
        string signUpDate = $"<t:{targetUser.CreatedAt.ToUnixTimeSeconds()}>";
        string boostingSince = targetUser.PremiumSince != null ? $"<t:{targetUser.PremiumSince.Value.ToUnixTimeSeconds()}:R>" : "Never";
        string roles = targetUser.Roles.Count > 1 ?
            string.Join(", ", targetUser.Roles.Skip(1).Select(x => $"<@&{x.Id}>")).Truncate(512)
            : "None";
        string onlineStatus = targetUser.GetStatusString();

        EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle("USER INFORMATION");
            
        replyEmbed.Title = "\u2139\uFE0F User Information";
        replyEmbed.Description = $"Here's some information about {targetUser.Mention}.";
        replyEmbed.WithColor(59, 136, 195);

        replyEmbed.WithThumbnailUrl(userAvatar);
        replyEmbed.AddField("\U0001faaa Username", userName, true);
        replyEmbed.AddField("\U0001f3ad Nickname", userNickname, true);
        replyEmbed.AddField("\U0001f6c2 User ID", userId, true);
        replyEmbed.AddField("\U0001f518 Status", onlineStatus, true);
        replyEmbed.AddField("\U0001fac5 Is Owner", isOwner, true);
        replyEmbed.AddField("\U0001f916 Is Bot", isBot, true);
        replyEmbed.AddField("\U0001f44b Join Date", joinDate, true);
        replyEmbed.AddField("\U0001f382 Creation Date", signUpDate, true);
        replyEmbed.AddField("\U0001f680 Booster Since", boostingSince, true);
        replyEmbed.AddField("\U0001f4e6 Roles", roles);
            
        await RespondAsync(embed: replyEmbed.Build(), allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    private string FriendlySystemName()
    {
        string osName;
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