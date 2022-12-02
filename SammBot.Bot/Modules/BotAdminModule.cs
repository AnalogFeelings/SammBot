using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using SammBot.Bot.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Interactions;
using SammBot.Bot.Attributes;
using SammBot.Bot.Core;
using SammBot.Bot.Extensions;
using SammBot.Bot.Preconditions;
using SammBot.Bot.Services;

namespace SammBot.Bot.Modules
{
    [PrettyName("Bot Administration")]
    [Group("badmin", "Bot management commands. Bot owner only.")]
    [ModuleEmoji("\U0001f4be")]
    [RequireOwner]
    public class BotAdminModule : InteractionModuleBase<ShardedInteractionContext>
    {
        public AdminService AdminService { get; set; }
        public Logger Logger { get; set; }

        [SlashCommand("say", "Make the bot say something.")]
        [DetailedDescription("Makes the bot say something. Use the **setsay** command to set the channel and guild beforehand.")]
        [RateLimit(2, 1)]
        public async Task<RuntimeResult> SayMessageAsync([Summary(description: "The message text.")] string Message)
        {
            if (AdminService.ChannelId == 0 || AdminService.GuildId == 0)
                return ExecutionResult.FromError("Please set a guild and channel ID beforehand!");

            SocketTextChannel targetChannel = Context.Client.GetGuild(AdminService.GuildId).GetTextChannel(AdminService.ChannelId);

            await targetChannel.SendMessageAsync(Message);

            return ExecutionResult.Succesful();
        }

        [SlashCommand("setsay", "Set the channel in which the say command will broadcast.")]
        [DetailedDescription("Sets the channel and guild where the say command will send messages to.")]
        [RateLimit(2, 1)]
        public async Task<RuntimeResult> SetSayAsync([Summary(description: "The target channel's ID.")] ulong Channel, 
                                                     [Summary(description: "The target guild's ID.")] ulong Guild)
        {
            if (Context.Client.GetGuild(Guild) == null) return ExecutionResult.FromError("I am not invited in that guild!");
            if (Context.Client.GetGuild(Guild).GetTextChannel(Channel) == null)
                return ExecutionResult.FromError($"Channel with ID {Channel} does not exist in guild with ID {Guild}.");

            AdminService.ChannelId = Channel;
            AdminService.GuildId = Guild;

            SocketGuild targetGuild = Context.Client.GetGuild(Guild);

            await RespondAsync($"Success. Set guild to `{targetGuild.Name}` and channel to `{targetGuild.GetTextChannel(Channel).Name}`.",
                ephemeral: true, allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

            return ExecutionResult.Succesful();
        }

        [SlashCommand("listservers", "Shows a list of all the servers the bot is in.")]
        [DetailedDescription("Shows a list of the servers the bot is in, and their corresponding IDs.")]
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

            await RespondAsync(builtMessage, ephemeral: true, allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

            return ExecutionResult.Succesful();
        }

        [SlashCommand("shutdown", "Shuts the bot down.")]
        [DetailedDescription("Shuts the bot down. That's it.")]
        [RateLimit(1, 1)]
        public async Task<RuntimeResult> ShutdownAsync()
        {
            await RespondAsync($"{SettingsManager.BOT_NAME} will shut down.", ephemeral: true, allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

            Logger.Log($"{SettingsManager.BOT_NAME} will shut down.\n\n", LogSeverity.Warning);

            Environment.Exit(0);

            return ExecutionResult.Succesful();
        }

        [SlashCommand("restart", "Restarts the bot.")]
        [DetailedDescription("Restarts the bot. That's it.")]
        [RateLimit(1, 1)]
        public async Task<RuntimeResult> RestartAsync()
        {
            await RespondAsync($"{SettingsManager.BOT_NAME} will restart.", ephemeral: true, allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

            Logger.Log($"{SettingsManager.BOT_NAME} will restart.\n\n", LogSeverity.Warning);

            BotGlobals.RestartBot();

            return ExecutionResult.Succesful();
        }

        [SlashCommand("leaveserver", "Leaves the specified server.")]
        [DetailedDescription("Forces the bot to leave the specified guild.")]
        [RateLimit(3, 1)]
        public async Task<RuntimeResult> LeaveAsync([Summary(description: "The ID of the guild you want the bot to leave.")] ulong ServerId)
        {
            SocketGuild targetGuild = Context.Client.GetGuild(ServerId);
            if (targetGuild == null)
                return ExecutionResult.FromError("I am not currently in this guild!");

            string targetGuildName = targetGuild.Name;
            await targetGuild.LeaveAsync();

            await RespondAsync($"Left the server \"{targetGuildName}\".", ephemeral: true, allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

            return ExecutionResult.Succesful();
        }

        [SlashCommand("listcfg", "Lists all of the bot settings available.")]
        [DetailedDescription("Lists the bot settings. Does NOT list the bot's token or the URL detection regex. Some settings are not modifiable without a restart.")]
        [RateLimit(3, 1)]
        public async Task<RuntimeResult> ListConfigAsync([Summary(description: "Set to **true** to list non-modifiable settings.")] bool Override = false)
        {
            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

            replyEmbed.Title = "\u2699\uFE0F Configuration File";
            replyEmbed.Description = "This is a list of all the bot settings that are safe to display publicly.\n" +
                                     "Properties with an orange diamond next to their name are marked as not modifiable at runtime.";
            replyEmbed.Color = new Color(102, 117, 127);

            List<PropertyInfo> properties = typeof(BotConfig).GetProperties()
                .Where(x => !x.PropertyType.IsGenericType &&
                            x.Name != "CatKey" &&
                            x.Name != "DogKey" &&
                            x.Name != "OpenWeatherKey" &&
                            x.Name != "BotToken").ToList();

            if (!Override)
            {
                replyEmbed.Description = "This is a list of all the bot settings that are safe to display publicly.";
                properties = properties.Where(x => x.GetCustomAttribute<RequiresReboot>() == null).ToList();
            }

            foreach (PropertyInfo property in properties)
            {
                string propertyName = string.Empty;
                string propertyValue = string.Empty;

                if (property.GetCustomAttribute<RequiresReboot>() == null) propertyName = "\U0001f539 ";
                else propertyName = "\U0001f538 ";

                propertyName += property.Name;
                propertyValue = $"\n**• Current Value**: `{property.GetValue(SettingsManager.Instance.LoadedConfig, null)}`";
                
                replyEmbed.AddField(propertyName, propertyValue);
            }

            await RespondAsync(null, embed: replyEmbed.Build(), ephemeral: true, allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

            return ExecutionResult.Succesful();
        }

        [SlashCommand("setcfg", "Sets a bot setting to the specified value.")]
        [DetailedDescription("Sets a bot setting to the value specified.")]
        [RateLimit(2, 1)]
        public async Task<RuntimeResult> SetConfigAsync([Summary(description: "The name of the setting you want to modify.")] string VarName,
                                                        [Summary(description: "The new value of the setting.")] string VarValue,
                                                        [Summary(description: "Set to **true** to restart the bot afterwards. Needed for non-modifiable settings.")] bool RestartBot = false)
        {
            PropertyInfo retrievedVariable = typeof(BotConfig).GetProperty(VarName);

            if (retrievedVariable == null)
                return ExecutionResult.FromError($"{VarName} does not exist!");

            if (typeof(IEnumerable).IsAssignableFrom(retrievedVariable.PropertyType) && retrievedVariable.PropertyType != typeof(string))
                return ExecutionResult.FromError($"{VarName} is a collection!");

            if (retrievedVariable.GetCustomAttribute<RequiresReboot>() != null && !RestartBot)
                return ExecutionResult.FromError($"**{VarName}** cannot be modified at runtime!\n" +
                    $"Please pass `true` to the **RestartBot** parameter.");

            AdminService.ChangingConfig = true;

            retrievedVariable.SetValue(SettingsManager.Instance.LoadedConfig, Convert.ChangeType(VarValue, retrievedVariable.PropertyType));

            object newValue = retrievedVariable.GetValue(SettingsManager.Instance.LoadedConfig);

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

            replyEmbed.Title = "\u2705 Success";
            replyEmbed.Description = $"Successfully set setting **{VarName}** to value `{newValue.ToString().Truncate(128)}`.";
            replyEmbed.WithColor(119, 178, 85);

            await RespondAsync(null, embed: replyEmbed.Build(), ephemeral: true, allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

            string configFilePath = Path.Combine(SettingsManager.Instance.BotDataDirectory, SettingsManager.CONFIG_FILE);
            string jsonContent = JsonConvert.SerializeObject(SettingsManager.Instance.LoadedConfig, Formatting.Indented);

            await File.WriteAllTextAsync(configFilePath, jsonContent);

            if (RestartBot)
            {
                await RespondAsync($"{SettingsManager.BOT_NAME} will restart.", allowedMentions: BotGlobals.Instance.AllowOnlyUsers);
                BotGlobals.RestartBot();
            }

            AdminService.ChangingConfig = false;

            return ExecutionResult.Succesful();
        }
    }
}