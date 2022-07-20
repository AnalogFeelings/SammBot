using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
	[Name("Administration")]
	[Group("admin")]
	[Summary("Bot management commands. Bot owner only.")]
	[ModuleEmoji("🛠")]
	public class AdminModule : ModuleBase<SocketCommandContext>
	{
		public AdminService AdminService { get; set; }
		public Logger Logger { get; set; }
		public CommandHandler CommandHandler { get; set; }

		[Command("say")]
		[Summary("Make the bot say something.")]
		[BotOwnerOnly]
		public async Task<RuntimeResult> SayMessageAsync([Remainder] string Message)
		{
			if (AdminService.ChannelId == 0 || AdminService.GuildId == 0)
				return ExecutionResult.FromError("Please set a guild and channel ID beforehand!");

			SocketTextChannel TargetChannel = Context.Client.GetGuild(AdminService.GuildId).GetTextChannel(AdminService.ChannelId);

			using (TargetChannel.EnterTypingState()) await TargetChannel.SendMessageAsync(Message);

			return ExecutionResult.Succesful();
		}

		[Command("setsay")]
		[Summary("Set the channel in which the say command will broadcast.")]
		[BotOwnerOnly]
		public async Task<RuntimeResult> SetSayAsync(ulong Channel, ulong Guild)
		{
			if (Context.Client.GetGuild(Guild) == null) return ExecutionResult.FromError("I am not invited in that guild!");
			if (Context.Client.GetGuild(Guild).GetTextChannel(Channel) == null)
				return ExecutionResult.FromError($"Channel with ID {Channel} does not exist in guild with ID {Guild}.");

			AdminService.ChannelId = Channel;
			AdminService.GuildId = Guild;

			SocketGuild TargetGuild = Context.Client.GetGuild(Guild);

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, Context.Guild.Id, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync($"Success. Set guild to `{TargetGuild.Name}` and channel to `{TargetGuild.GetTextChannel(Channel).Name}`.", allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("shutdown")]
		[Alias("kill")]
		[Summary("Shuts the bot down.")]
		[BotOwnerOnly]
		public async Task<RuntimeResult> ShutdownAsync()
		{
			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, Context.Guild.Id, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync($"{Settings.Instance.LoadedConfig.BotName} will shut down.", allowedMentions: AllowedMentions, messageReference: Reference);

			Logger.Log($"{Settings.Instance.LoadedConfig.BotName} will shut down.\n\n", LogSeverity.Warning);

			Environment.Exit(0);

			return ExecutionResult.Succesful();
		}

		[Command("restart")]
		[Alias("reboot", "reset")]
		[Summary("Restarts the bot.")]
		[BotOwnerOnly]
		public async Task<RuntimeResult> RestartAsync()
		{
			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, Context.Guild.Id, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync($"{Settings.Instance.LoadedConfig.BotName} will restart.", allowedMentions: AllowedMentions, messageReference: Reference);

			Logger.Log($"{Settings.Instance.LoadedConfig.BotName} will restart.\n\n", LogSeverity.Warning);

			Settings.Instance.RestartBot();

			return ExecutionResult.Succesful();
		}

		[Command("leaveserver")]
		[Alias("leave")]
		[Summary("Leaves the specified server.")]
		[BotOwnerOnly]
		public async Task<RuntimeResult> LeaveAsync(ulong ServerId)
		{
			SocketGuild TargetGuild = Context.Client.GetGuild(ServerId);
			if (TargetGuild == null)
				return ExecutionResult.FromError("I am not currently in this guild!");

			string GuildName = TargetGuild.Name;
			await TargetGuild.LeaveAsync();

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, Context.Guild.Id, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync($"Left the server \"{GuildName}\".", allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("mime")]
		[Alias("mimic, spy")]
		[Summary("Mimics a user, used for testing.")]
		[BotOwnerOnly]
		public async Task<RuntimeResult> MimicUserAsync(SocketGuildUser User, [Remainder] string Command)
		{
			//LORD HAVE MERCY
			SocketMessage SourceMessage = Context.Message as SocketMessage;
			FieldInfo AuthorField = typeof(SocketMessage).GetField("<Author>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
			FieldInfo ContentField = typeof(SocketMessage).GetField("<Content>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
			AuthorField.SetValue(SourceMessage, User);
			ContentField.SetValue(SourceMessage, Command);

			await CommandHandler.HandleCommandAsync(SourceMessage);

			return ExecutionResult.Succesful();
		}

		[Command("listcfg")]
		[Alias("lc")]
		[Summary("Lists all of the bot settings available.")]
		[BotOwnerOnly]
		public async Task<RuntimeResult> ListConfigAsync(bool Override = false)
		{
			EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, "Configuration File");

			List<PropertyInfo> Properties = typeof(JsonConfig).GetProperties()
				.Where(x => !x.PropertyType.IsGenericType &&
						x.Name != "UrlRegex" &&
						x.Name != "BotToken").ToList();

			if (!Override)
				Properties = Properties.Where(x => x.GetCustomAttribute<NotModifiable>() == null).ToList();

			foreach (PropertyInfo Property in Properties)
			{
				ReplyEmbed.AddField(Property.Name, Property.GetValue(Settings.Instance.LoadedConfig, null));
			}

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, Context.Guild.Id, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync(null, false, ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("setcfg")]
		[Alias("config")]
		[Summary("Sets a bot setting to the specified value.")]
		[BotOwnerOnly]
		public async Task<RuntimeResult> SetConfigAsync(string VarName, string VarValue, bool RestartBot = false)
		{
			PropertyInfo RetrievedVariable = typeof(JsonConfig).GetProperty(VarName);

			if (RetrievedVariable == null)
				return ExecutionResult.FromError($"{VarName} does not exist!");

			if (RetrievedVariable.PropertyType is IList)
				return ExecutionResult.FromError($"{VarName} is a list variable!");

			if (RetrievedVariable.GetCustomAttribute<NotModifiable>() != null && !RestartBot)
				return ExecutionResult.FromError($"{VarName} cannot be modified at runtime! " +
					$"Please pass `true` to the `restartBot` parameter.");

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, Context.Guild.Id, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

			AdminService.ChangingConfig = true;

			RetrievedVariable.SetValue(Settings.Instance.LoadedConfig, Convert.ChangeType(VarValue, RetrievedVariable.PropertyType));

			object NewValue = RetrievedVariable.GetValue(Settings.Instance.LoadedConfig);
			
			await ReplyAsync($"Set variable \"{VarName}\" to `{NewValue.ToString().Truncate(128)}` succesfully.", allowedMentions: AllowedMentions, messageReference: Reference);

			await File.WriteAllTextAsync(Settings.Instance.ConfigFile,
				JsonConvert.SerializeObject(Settings.Instance.LoadedConfig, Formatting.Indented));

			if (RestartBot)
			{
				await ReplyAsync($"{Settings.Instance.LoadedConfig.BotName} will restart.", allowedMentions: AllowedMentions, messageReference: Reference);
				Settings.Instance.RestartBot();
			}

			AdminService.ChangingConfig = false;

			return ExecutionResult.Succesful();
		}
	}
}