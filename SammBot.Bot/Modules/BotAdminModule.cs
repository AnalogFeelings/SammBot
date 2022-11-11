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

namespace SammBot.Bot.Modules
{
    [FullName("Bot Administration")]
    [Group("badmin", "Bot management commands. Bot owner only.")]
    [ModuleEmoji("\U0001f4be")]
    [RequireOwner]
    public class BotAdminModule : InteractionModuleBase<SocketInteractionContext>
    {
        public AdminService AdminService { get; set; }
        public Logger Logger { get; set; }

        [SlashCommand("say", "Make the bot say something.")]
        [FullDescription("Makes the bot say something. Use the **setsay** command to set the channel and guild beforehand.")]
        [RateLimit(2, 1)]
        public async Task<RuntimeResult> SayMessageAsync([Summary("The message text.")] string Message)
        {
            if (AdminService.ChannelId == 0 || AdminService.GuildId == 0)
                return ExecutionResult.FromError("Please set a guild and channel ID beforehand!");

            SocketTextChannel targetChannel = Context.Client.GetGuild(AdminService.GuildId).GetTextChannel(AdminService.ChannelId);

            using (targetChannel.EnterTypingState()) await targetChannel.SendMessageAsync(Message);

            return ExecutionResult.Succesful();
        }

        [SlashCommand("setsay", "Set the channel in which the say command will broadcast.")]
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

            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await RespondAsync($"Success. Set guild to `{targetGuild.Name}` and channel to `{targetGuild.GetTextChannel(Channel).Name}`.", allowedMentions: allowedMentions);

            return ExecutionResult.Succesful();
        }

        [SlashCommand("listservers", "Shows a list of all the servers the bot is in.")]
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

            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await RespondAsync(builtMessage, allowedMentions: allowedMentions);

            return ExecutionResult.Succesful();
        }

        [SlashCommand("shutdown", "Shuts the bot down.")]
        [FullDescription("Shuts the bot down. That's it.")]
        [RateLimit(1, 1)]
        public async Task<RuntimeResult> ShutdownAsync()
        {
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await RespondAsync($"{Settings.BOT_NAME} will shut down.", allowedMentions: allowedMentions);

            Logger.Log($"{Settings.BOT_NAME} will shut down.\n\n", LogSeverity.Warning);

            Environment.Exit(0);

            return ExecutionResult.Succesful();
        }

        [SlashCommand("restart", "Restarts the bot.")]
        [FullDescription("Restarts the bot. That's it.")]
        [RateLimit(1, 1)]
        public async Task<RuntimeResult> RestartAsync()
        {
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await RespondAsync($"{Settings.BOT_NAME} will restart.", allowedMentions: allowedMentions);

            Logger.Log($"{Settings.BOT_NAME} will restart.\n\n", LogSeverity.Warning);

            Settings.RestartBot();

            return ExecutionResult.Succesful();
        }

        [SlashCommand("leaveserver", "Leaves the specified server.")]
        [FullDescription("Forces the bot to leave the specified guild.")]
        [RateLimit(3, 1)]
        public async Task<RuntimeResult> LeaveAsync([Summary("The ID of the guild you want the bot to leave.")] ulong ServerId)
        {
            SocketGuild targetGuild = Context.Client.GetGuild(ServerId);
            if (targetGuild == null)
                return ExecutionResult.FromError("I am not currently in this guild!");

            string targetGuildName = targetGuild.Name;
            await targetGuild.LeaveAsync();

            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await RespondAsync($"Left the server \"{targetGuildName}\".", allowedMentions: allowedMentions);

            return ExecutionResult.Succesful();
        }

        [SlashCommand("listcfg", "Lists all of the bot settings available.")]
        [FullDescription("Lists the bot settings. Does NOT list the bot's token or the URL detection regex. Some settings are not modifiable without a restart.")]
        [RateLimit(3, 1)]
        public async Task<RuntimeResult> ListConfigAsync([Summary("Set to **true** to list non-modifiable settings.")] bool Override = false)
        {
            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

            replyEmbed.Title = "\u2699\uFE0F Configuration File";
            replyEmbed.Description = "This is a list of all the bot settings that are safe to display publicly.\n" +
                                     "Properties with an orange diamond next to their name are marked as not modifiable at runtime.";
            replyEmbed.Color = new Color(102, 117, 127);

            List<PropertyInfo> properties = typeof(JsonConfig).GetProperties()
                .Where(x => !x.PropertyType.IsGenericType &&
                            x.Name != "CatKey" &&
                            x.Name != "DogKey" &&
                            x.Name != "OpenWeatherKey" &&
                            x.Name != "BotToken").ToList();

            if (!Override)
            {
                replyEmbed.Description = "This is a list of all the bot settings that are safe to display publicly.";
                properties = properties.Where(x => x.GetCustomAttribute<NotModifiable>() == null).ToList();
            }

            foreach (PropertyInfo property in properties)
            {
                string propertyName = string.Empty;
                string propertyValue = string.Empty;

                if (property.GetCustomAttribute<NotModifiable>() == null) propertyName = "\U0001f539 ";
                else propertyName = "\U0001f538 ";

                propertyName += property.Name;
                propertyValue = $"\n**• Current Value**: `{property.GetValue(Settings.Instance.LoadedConfig, null)}`";
                
                replyEmbed.AddField(propertyName, propertyValue);
            }

            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await RespondAsync(null, embed: replyEmbed.Build(), ephemeral: true, allowedMentions: allowedMentions);

            return ExecutionResult.Succesful();
        }

        [SlashCommand("setcfg", "Sets a bot setting to the specified value.")]
        [FullDescription("Sets a bot setting to the value specified.")]
        [RateLimit(2, 1)]
        public async Task<RuntimeResult> SetConfigAsync([Summary("The name of the setting you want to modify.")] string VarName,
                                                        [Summary("The new value of the setting.")] string VarValue,
                                                        [Summary("Set to **true** to restart the bot afterwards. Needed for non-modifiable settings.")] bool RestartBot = false)
        {
            PropertyInfo retrievedVariable = typeof(JsonConfig).GetProperty(VarName);

            if (retrievedVariable == null)
                return ExecutionResult.FromError($"{VarName} does not exist!");

            if (typeof(IEnumerable).IsAssignableFrom(retrievedVariable.PropertyType) && retrievedVariable.PropertyType != typeof(string))
                return ExecutionResult.FromError($"{VarName} is a collection!");

            if (retrievedVariable.GetCustomAttribute<NotModifiable>() != null && !RestartBot)
                return ExecutionResult.FromError($"**{VarName}** cannot be modified at runtime!\n" +
                    $"Please pass `true` to the **RestartBot** parameter.");

            AdminService.ChangingConfig = true;

            retrievedVariable.SetValue(Settings.Instance.LoadedConfig, Convert.ChangeType(VarValue, retrievedVariable.PropertyType));

            object newValue = retrievedVariable.GetValue(Settings.Instance.LoadedConfig);
            
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            
            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

            replyEmbed.Title = "\u2705 Success";
            replyEmbed.Description = $"Successfully set setting **{VarName}** to value `{newValue.ToString().Truncate(128)}`.";
            replyEmbed.WithColor(119, 178, 85);

            await RespondAsync(null, embed: replyEmbed.Build(), ephemeral: true, allowedMentions: allowedMentions);

            await File.WriteAllTextAsync(Settings.CONFIG_FILE,
                JsonConvert.SerializeObject(Settings.Instance.LoadedConfig, Formatting.Indented));

            if (RestartBot)
            {
                await RespondAsync($"{Settings.BOT_NAME} will restart.", allowedMentions: allowedMentions);
                Settings.RestartBot();
            }

            AdminService.ChangingConfig = false;

            return ExecutionResult.Succesful();
        }
    }
}