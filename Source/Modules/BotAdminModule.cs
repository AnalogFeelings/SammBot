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
    [Name("Bot Administration")]
    [Group("badmin")]
    [Summary("Bot management commands. Bot owner only.")]
    [ModuleEmoji("\u2699")]
    [RequireOwner]
    public class BotAdminModule : ModuleBase<SocketCommandContext>
    {
        public AdminService AdminService { get; set; }
        public Logger Logger { get; set; }
        public CommandHandler CommandHandler { get; set; }

        [Command("say")]
        [Summary("Make the bot say something.")]
        [FullDescription("Makes the bot say something. Use the **setsay** command to set the channel and guild beforehand.")]
        [RateLimit(2, 1)]
        public async Task<RuntimeResult> SayMessageAsync([Summary("The message text.")] [Remainder] string Message)
        {
            if (AdminService.ChannelId == 0 || AdminService.GuildId == 0)
                return ExecutionResult.FromError("Please set a guild and channel ID beforehand!");

            SocketTextChannel targetChannel = Context.Client.GetGuild(AdminService.GuildId).GetTextChannel(AdminService.ChannelId);

            using (targetChannel.EnterTypingState()) await targetChannel.SendMessageAsync(Message);

            return ExecutionResult.Succesful();
        }

        [Command("setsay")]
        [Summary("Set the channel in which the say command will broadcast.")]
        [FullDescription("Sets the channel and guild where the say command will send messages to.")]
        [RateLimit(2, 1)]
        public async Task<RuntimeResult> SetSayAsync([Summary("The target channel's ID.")] ulong Channel, 
                                                     [Summary("The target guild's ID.")] ulong Guild)
        {
            if (Context.Client.GetGuild(Guild) == null) return ExecutionResult.FromError("I am not invited in that guild!");
            if (Context.Client.GetGuild(Guild).GetTextChannel(Channel) == null)
                return ExecutionResult.FromError($"Channel with ID {Channel} does not exist in guild with ID {Guild}.");

            AdminService.ChannelId = Channel;
            AdminService.GuildId = Guild;

            SocketGuild targetGuild = Context.Client.GetGuild(Guild);

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync($"Success. Set guild to `{targetGuild.Name}` and channel to `{targetGuild.GetTextChannel(Channel).Name}`.", allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("listservers")]
        [Alias("guilds")]
        [Summary("Shows a list of all the servers the bot is in.")]
        [FullDescription("Shows a list of the servers the bot is in, and their corresponding IDs.")]
        [RateLimit(3, 1)]
        public async Task<RuntimeResult> ServersAsync()
        {
            string builtMessage = "I am invited in the following servers:\n";
            string codeBlock = string.Empty;

            int i = 1;
            foreach (SocketGuild targetGuild in Context.Client.Guilds)
            {
                codeBlock += $"{i}. {targetGuild.Name} (ID {targetGuild.Id})\n";
                i++;
            }

            builtMessage += Format.Code(codeBlock);

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync(builtMessage, allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("shutdown")]
        [Alias("kill")]
        [Summary("Shuts the bot down.")]
        [FullDescription("Shuts the bot down. That's it.")]
        [RateLimit(1, 1)]
        public async Task<RuntimeResult> ShutdownAsync()
        {
            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync($"{Settings.BOT_NAME} will shut down.", allowedMentions: allowedMentions, messageReference: messageReference);

            Logger.Log($"{Settings.BOT_NAME} will shut down.\n\n", LogSeverity.Warning);

            Environment.Exit(0);

            return ExecutionResult.Succesful();
        }

        [Command("restart")]
        [Alias("reboot", "reset")]
        [Summary("Restarts the bot.")]
        [FullDescription("Restarts the bot. That's it.")]
        [RateLimit(1, 1)]
        public async Task<RuntimeResult> RestartAsync()
        {
            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync($"{Settings.BOT_NAME} will restart.", allowedMentions: allowedMentions, messageReference: messageReference);

            Logger.Log($"{Settings.BOT_NAME} will restart.\n\n", LogSeverity.Warning);

            Settings.RestartBot();

            return ExecutionResult.Succesful();
        }

        [Command("leaveserver")]
        [Alias("leave")]
        [Summary("Leaves the specified server.")]
        [FullDescription("Forces the bot to leave the specified guild.")]
        [RateLimit(3, 1)]
        public async Task<RuntimeResult> LeaveAsync([Summary("The ID of the guild you want the bot to leave.")] ulong ServerId)
        {
            SocketGuild targetGuild = Context.Client.GetGuild(ServerId);
            if (targetGuild == null)
                return ExecutionResult.FromError("I am not currently in this guild!");

            string targetGuildName = targetGuild.Name;
            await targetGuild.LeaveAsync();

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync($"Left the server \"{targetGuildName}\".", allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("mime")]
        [Alias("mimic, spy")]
        [Summary("Mimics a user, used for testing.")]
        [FullDescription("Hacky command to execute a command as someone else.")]
        public async Task<RuntimeResult> MimicUserAsync([Summary("The user you want to execute the command as.")] SocketGuildUser User, 
                                                        [Summary("The command you want to execute.")] [Remainder] string Command)
        {
            //LORD HAVE MERCY
            SocketMessage sourceMessage = Context.Message as SocketMessage;
            FieldInfo authorField = typeof(SocketMessage).GetField("<Author>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo contentField = typeof(SocketMessage).GetField("<Content>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            authorField.SetValue(sourceMessage, User);
            contentField.SetValue(sourceMessage, Command);

            await CommandHandler.HandleCommandAsync(sourceMessage);

            return ExecutionResult.Succesful();
        }

        [Command("listcfg")]
        [Alias("lc")]
        [Summary("Lists all of the bot settings available.")]
        [FullDescription("Lists the bot settings. Does NOT list the bot's token or the URL detection regex. Some settings are not modifiable without a restart.")]
        [RateLimit(3, 1)]
        public async Task<RuntimeResult> ListConfigAsync([Summary("Set to **true** to list non-modifiable settings.")] bool Override = false)
        {
            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, "Configuration File");

            List<PropertyInfo> properties = typeof(JsonConfig).GetProperties()
                .Where(x => !x.PropertyType.IsGenericType &&
                        x.Name != "UrlRegex" &&
                        x.Name != "BotToken").ToList();

            if (!Override)
                properties = properties.Where(x => x.GetCustomAttribute<NotModifiable>() == null).ToList();

            foreach (PropertyInfo property in properties)
            {
                replyEmbed.AddField(property.Name, property.GetValue(Settings.Instance.LoadedConfig, null));
            }

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync(null, false, replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("setcfg")]
        [Alias("config")]
        [Summary("Sets a bot setting to the specified value.")]
        [FullDescription("Sets a bot setting to the value specified.")]
        [RateLimit(2, 1)]
        public async Task<RuntimeResult> SetConfigAsync([Summary("The name of the setting you want to modify.")] string VarName,
                                                        [Summary("The new value of the setting.")] string VarValue,
                                                        [Summary("Set to **true** to restart the bot afterwards. Needed for non-modifiable settings.")] bool RestartBot = false)
        {
            PropertyInfo retrievedVariable = typeof(JsonConfig).GetProperty(VarName);

            if (retrievedVariable == null)
                return ExecutionResult.FromError($"{VarName} does not exist!");

            if (retrievedVariable.PropertyType is IList)
                return ExecutionResult.FromError($"{VarName} is a list variable!");

            if (retrievedVariable.GetCustomAttribute<NotModifiable>() != null && !RestartBot)
                return ExecutionResult.FromError($"{VarName} cannot be modified at runtime! " +
                    $"Please pass `true` to the `RestartBot` parameter.");

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

            AdminService.ChangingConfig = true;

            retrievedVariable.SetValue(Settings.Instance.LoadedConfig, Convert.ChangeType(VarValue, retrievedVariable.PropertyType));

            object newValue = retrievedVariable.GetValue(Settings.Instance.LoadedConfig);

            await ReplyAsync($"Set variable \"{VarName}\" to `{newValue.ToString().Truncate(128)}` succesfully.", allowedMentions: allowedMentions, messageReference: messageReference);

            await File.WriteAllTextAsync(Settings.CONFIG_FILE,
                JsonConvert.SerializeObject(Settings.Instance.LoadedConfig, Formatting.Indented));

            if (RestartBot)
            {
                await ReplyAsync($"{Settings.BOT_NAME} will restart.", allowedMentions: allowedMentions, messageReference: messageReference);
                Settings.RestartBot();
            }

            AdminService.ChangingConfig = false;

            return ExecutionResult.Succesful();
        }
    }
}