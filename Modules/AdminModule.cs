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

			await ReplyAsync($"Success. Set guild to `{TargetGuild.Name}` and channel to `{TargetGuild.GetTextChannel(Channel).Name}`.");

			return ExecutionResult.Succesful();
		}

		[Command("shutdown")]
		[Alias("kill")]
		[Summary("Shuts the bot down.")]
		[BotOwnerOnly]
		public async Task<RuntimeResult> ShutdownAsync()
		{
			await ReplyAsync($"{Settings.Instance.LoadedConfig.BotName} will shut down.");
			Logger.Log($"{Settings.Instance.LoadedConfig.BotName} will shut down.\n\n", LogLevel.Warning);

			Environment.Exit(0);

			return ExecutionResult.Succesful();
		}

		[Command("restart")]
		[Alias("reboot", "reset")]
		[Summary("Restarts the bot.")]
		[BotOwnerOnly]
		public async Task<RuntimeResult> RestartAsync()
		{
			await ReplyAsync($"{Settings.Instance.LoadedConfig.BotName} will restart.");
			Logger.Log($"{Settings.Instance.LoadedConfig.BotName} will restart.\n\n", LogLevel.Warning);

			Settings.Instance.RestartBot();

			return ExecutionResult.Succesful();
		}

		[Command("leaveserver")]
		[Alias("leave")]
		[Summary("Leaves the specified server.")]
		[BotOwnerOnly]
		public async Task<RuntimeResult> LeaveAsync(ulong ServerId)
		{
			SocketGuild targetGuild = Context.Client.GetGuild(ServerId);
			if (targetGuild == null)
				return ExecutionResult.FromError("I am not currently in this guild!");

			string guildName = targetGuild.Name;
			await targetGuild.LeaveAsync();

			await ReplyAsync($"Left the server \"{guildName}\".");

			return ExecutionResult.Succesful();
		}

		[Command("mime")]
		[Alias("mimic, spy")]
		[Summary("Mimics a user, used for testing.")]
		[BotOwnerOnly]
		public async Task<RuntimeResult> MimicUserAsync(SocketGuildUser User, [Remainder] string Command)
		{
			//LORD HAVE MERCY
			SocketMessage message = Context.Message as SocketMessage;
			FieldInfo authorField = typeof(SocketMessage).GetField("<Author>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
			FieldInfo contentField = typeof(SocketMessage).GetField("<Content>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
			authorField.SetValue(message, User);
			contentField.SetValue(message, Command);

			await CommandHandler.HandleCommandAsync(message);

			return ExecutionResult.Succesful();
		}

		[Command("listcfg")]
		[Alias("lc")]
		[Summary("Lists all of the bot settings available.")]
		[BotOwnerOnly]
		public async Task<RuntimeResult> ListConfigAsync(bool Override = false)
		{
			EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context, "Configuration File");

			List<PropertyInfo> properties = typeof(JsonConfig).GetProperties()
				.Where(x => !x.PropertyType.IsGenericType &&
						x.Name != "UrlRegex" &&
						x.Name != "BotToken").ToList();

			if (!Override)
				properties = properties.Where(x => x.GetCustomAttribute<NotModifiable>() == null).ToList();

			foreach (PropertyInfo property in properties)
			{
				embed.AddField(property.Name, property.GetValue(Settings.Instance.LoadedConfig, null));
			}

			await Context.Channel.SendMessageAsync("", false, embed.Build());

			return ExecutionResult.Succesful();
		}

		[Command("setcfg")]
		[Alias("config")]
		[Summary("Sets a bot setting to the specified value.")]
		[BotOwnerOnly]
		public async Task<RuntimeResult> SetConfigAsync(string VarName, string VarValue, bool RestartBot = false)
		{
			PropertyInfo retrievedVariable = typeof(JsonConfig).GetProperty(VarName);

			if (retrievedVariable == null)
				return ExecutionResult.FromError($"{VarName} does not exist!");

			if (retrievedVariable.PropertyType is IList)
				return ExecutionResult.FromError($"{VarName} is a list variable!");

			if (retrievedVariable.GetCustomAttribute<NotModifiable>() != null && !RestartBot)
				return ExecutionResult.FromError($"{VarName} cannot be modified at runtime! " +
					$"Please pass `true` to the `restartBot` parameter.");

			AdminService.ChangingConfig = true;

			retrievedVariable.SetValue(Settings.Instance.LoadedConfig, Convert.ChangeType(VarValue, retrievedVariable.PropertyType));

			object newValue = retrievedVariable.GetValue(Settings.Instance.LoadedConfig);

			await ReplyAsync($"Set variable \"{VarName}\" to `{newValue.ToString().Truncate(128)}` succesfully.");
			await File.WriteAllTextAsync(Settings.Instance.ConfigFile,
				JsonConvert.SerializeObject(Settings.Instance.LoadedConfig, Formatting.Indented));

			if (RestartBot)
			{
				await ReplyAsync($"{Settings.Instance.LoadedConfig.BotName} will restart.");
				Settings.Instance.RestartBot();
			}

			AdminService.ChangingConfig = false;

			return ExecutionResult.Succesful();
		}
	}
}