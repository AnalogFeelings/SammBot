﻿using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
	[Name("Information")]
	[Group("info")]
	[Summary("Bot information and statistics.")]
	[ModuleEmoji("ℹ️")]
	public class InformationModule : ModuleBase<SocketCommandContext>
	{
		public DiscordSocketClient Client { get; set; }

		[Command("full")]
		[Summary("Shows the FULL information of the bot.")]
		[FullDescription("Shows version, uptime, ping, etc...")]
		public async Task<RuntimeResult> InformationFullAsync()
		{
			EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, "Information", "All public information about the bot.");

			string ElapsedUptime = string.Format("{0:00}d{1:00}h{2:00}m",
				Settings.Instance.RuntimeStopwatch.Elapsed.Days,
				Settings.Instance.RuntimeStopwatch.Elapsed.Hours,
				Settings.Instance.RuntimeStopwatch.Elapsed.Minutes);

			ReplyEmbed.AddField("Bot Version", $"`{Settings.Instance.LoadedConfig.BotVersion}`", true);
			ReplyEmbed.AddField(".NET Version", $"`{RuntimeInformation.FrameworkDescription}`", true);
			ReplyEmbed.AddField("Ping", $"`{Context.Client.Latency}ms.`", true);
			ReplyEmbed.AddField("Im In", $"`{Context.Client.Guilds.Count} server/s.`", true);
			ReplyEmbed.AddField("Uptime", $"`{ElapsedUptime}`", true);
			ReplyEmbed.AddField("Host", $"`{FriendlyOSName()}`", true);

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync(null, false, ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("serverinfo")]
		[Alias("server", "guildinfo")]
		[Summary("Get information about a server!")]
		[FullDescription("Gets all the information about the server you execute the command in.")]
		[RequireContext(ContextType.Guild)]
		public async Task<RuntimeResult> ServerInfoAsync()
		{
			RestUser ServerOwner = await Client.Rest.GetUserAsync(Context.Guild.OwnerId);

			string ServerBanner = Context.Guild.BannerUrl != null ? $"[Banner URL]({Context.Guild.BannerUrl})" : "None";
			string DiscoverySplash = Context.Guild.DiscoverySplashUrl != null ? $"[Splash URL]({Context.Guild.DiscoverySplashId})" : "None";
			int ChannelCount = Context.Guild.Channels.Count;
			int EmoteCount = Context.Guild.Emotes.Count;
			int MemberCount = Context.Guild.MemberCount;
			int BoostTier = (int)Context.Guild.PremiumTier;
			int RoleCount = Context.Guild.Roles.Count;
			string BoostCount = Context.Guild.PremiumSubscriptionCount != 0 ? Context.Guild.PremiumSubscriptionCount.ToString() : "No Boosts";
			string CreationDate = $"<t:{Context.Guild.CreatedAt.ToUnixTimeSeconds()}>";
			string ServerName = Context.Guild.Name;
			string ServerOwnerName = $"{ServerOwner.Username}#{ServerOwner.Discriminator}";

			EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle("GUILD INFORMATION");

			if (Context.Guild.IconUrl != null) embed.WithThumbnailUrl(Context.Guild.IconUrl);
			embed.AddField("Name", ServerName, true);
			embed.AddField("Owner", ServerOwnerName, true);
			embed.AddField("Banner", ServerBanner, true);
			embed.AddField("Discovery Splash", DiscoverySplash, true);
			embed.AddField("Nitro Boosts", BoostCount, true);
			embed.AddField("Nitro Tier", BoostTier, true);
			embed.AddField("Created At", CreationDate, true);
			embed.AddField("Channel Count", ChannelCount, true);
			embed.AddField("Emote Count", EmoteCount, true);
			embed.AddField("Member Count", MemberCount, true);
			embed.AddField("Role Count", RoleCount, true);

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync(null, false, embed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("userinfo")]
		[Alias("user")]
		[Summary("Get information about a user!")]
		[FullDescription("Gets all the information about the provided user.")]
		[RequireContext(ContextType.Guild)]
		public async Task<RuntimeResult> UserInfoAsync(SocketGuildUser User = null)
		{
			SocketGuildUser TargetUser = User ?? Context.Message.Author as SocketGuildUser;

			string UserAvatar = TargetUser.GetAvatarOrDefault(2048);
			string UserName = $"{TargetUser.Username}";
			string UserDiscriminator = $"#{TargetUser.Discriminator}";
			string UserNickname = TargetUser.Nickname ?? "None";
			string IsBot = TargetUser.IsBot.ToYesNo();
			string IsWebhook = TargetUser.IsWebhook.ToYesNo();
			string JoinDate = $"<t:{TargetUser.JoinedAt.Value.ToUnixTimeSeconds()}>";
			string SignUpDate = $"<t:{TargetUser.CreatedAt.ToUnixTimeSeconds()}>";
			string BoostingSince = TargetUser.PremiumSince != null ? $"<t:{TargetUser.PremiumSince.Value.ToUnixTimeSeconds()}:R>" : "Never";
			string Roles = TargetUser.Roles.Count > 1 ?
				string.Join(", ", TargetUser.Roles.Skip(1).Select(x => $"<@&{x.Id}>")).Truncate(512)
				: "None";
			string OnlineStatus = TargetUser.GetStatusString();

			EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle("USER INFORMATION");

			ReplyEmbed.WithThumbnailUrl(UserAvatar);
			ReplyEmbed.AddField("Username", UserName, true);
			ReplyEmbed.AddField("Nickname", UserNickname, true);
			ReplyEmbed.AddField("Discriminator", UserDiscriminator, true);
			ReplyEmbed.AddField("Status", OnlineStatus, true);
			ReplyEmbed.AddField("Is Bot", IsBot, true);
			ReplyEmbed.AddField("Is Webhook", IsWebhook, true);
			ReplyEmbed.AddField("Join Date", JoinDate, true);
			ReplyEmbed.AddField("Create Date", SignUpDate, true);
			ReplyEmbed.AddField("Booster Since", BoostingSince, true);
			ReplyEmbed.AddField("Roles", Roles, false);

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync(null, false, ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		public string FriendlyOSName()
		{
			string OsName = string.Empty;

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				Version Version = Environment.OSVersion.Version;

				switch (Version.Major)
				{
					case 6:
						OsName = Version.Minor switch
						{
							1 => "Windows 7",
							2 => "Windows 8",
							3 => "Windows 8.1",
							_ => "Unknown Windows",
						};
						break;
					case 10:
						switch (Version.Minor)
						{
							case 0:
								if (Version.Build >= 22000) OsName = "Windows 11";
								else OsName = "Windows 10";

								break;
							default: OsName = "Unknown Windows"; break;
						}
						break;
					default:
						OsName = "Unknown Windows";
						break;
				}
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				if (File.Exists("/etc/issue.net"))
					OsName = File.ReadAllText("/etc/issue.net");
				else
					OsName = "Linux";
			}

			return OsName;
		}
	}
}