using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using SammBotNET.Extensions;
using SammBotNET.Services;
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
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        public AdminService AdminService { get; set; }
        public Logger Logger { get; set; }

        [Command("say", RunMode = RunMode.Async)]
        [HideInHelp]
        public async Task<RuntimeResult> SayMessageAsync([Remainder] string message)
        {
            if (Context.Message.Author.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            SocketTextChannel channel = Context.Client.GetGuild(AdminService.GuildId).GetTextChannel(AdminService.ChannelId);

            using (channel.EnterTypingState()) await channel.SendMessageAsync(message);

            return ExecutionResult.Succesful();
        }

        [Command("setsay", RunMode = RunMode.Async)]
        [HideInHelp]
        public async Task<RuntimeResult> SetSayAsync(ulong channel, ulong guild)
        {
            if (Context.Message.Author.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            if (Context.Client.GetGuild(guild) == null) return ExecutionResult.FromError("I am not invited in that guild!");
            if (Context.Client.GetGuild(guild).GetTextChannel(channel) == null)
                return ExecutionResult.FromError($"Channel with ID {channel} does not exist in guild with ID {guild}.");

            AdminService.ChannelId = channel;
            AdminService.GuildId = guild;

            SocketGuild socketGuild = Context.Client.GetGuild(guild);

            await ReplyAsync($"Success. Set guild to `{socketGuild.Name}` and channel to `{socketGuild.GetTextChannel(channel).Name}`.");

            return ExecutionResult.Succesful();
        }

        [Command("shutdown")]
        [Alias("kill")]
        [HideInHelp]
        public async Task<RuntimeResult> ShutdownAsync()
        {
            if (Context.User.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            await ReplyAsync($"{GlobalConfig.Instance.LoadedConfig.BotName} will shut down.");
            Logger.Log(LogLevel.Warning, $"{GlobalConfig.Instance.LoadedConfig.BotName} will shut down.\n\n");

            Environment.Exit(0);

            return ExecutionResult.Succesful();
        }

        [Command("restart")]
        [Alias("reboot", "reset")]
        [HideInHelp]
        public async Task<RuntimeResult> RestartAsync()
        {
            if (Context.User.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            await ReplyAsync($"{GlobalConfig.Instance.LoadedConfig.BotName} will restart.");
            Logger.Log(LogLevel.Warning, $"{GlobalConfig.Instance.LoadedConfig.BotName} will restart.\n\n");

            GlobalConfig.Instance.RestartBot();

            return ExecutionResult.Succesful();
        }

        [Command("leaveserver")]
        [Alias("leave")]
        [HideInHelp]
        public async Task<RuntimeResult> LeaveAsync(ulong serverId)
        {
            if (Context.User.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            SocketGuild guild = Context.Client.GetGuild(serverId);
            if (guild == null)
                return ExecutionResult.FromError("I am not currently in this guild!");

            string guildName = guild.Name;
            await guild.LeaveAsync();

            await ReplyAsync($"Left the server \"{guildName}\".");

            return ExecutionResult.Succesful();
        }

        [Command("listcfg")]
        [Alias("lc")]
        [HideInHelp]
        public async Task<RuntimeResult> ListConfigAsync(bool @override = false)
        {
            if (Context.User.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context, "Configuration File");

            List<PropertyInfo> properties = typeof(JsonConfig).GetProperties()
                .Where(x => !x.PropertyType.IsGenericType &&
                        x.Name != "UrlRegex" &&
                        x.Name != "BotToken").ToList();

            if (!@override)
                properties = properties.Where(x => x.GetCustomAttribute<NotModifiable>() == null).ToList();

            foreach (PropertyInfo property in properties)
            {
                embed.AddField(property.Name, property.GetValue(GlobalConfig.Instance.LoadedConfig, null));
            }

            await Context.Channel.SendMessageAsync("", false, embed.Build());

            return ExecutionResult.Succesful();
        }

        [Command("setcfg")]
        [Alias("config")]
        [HideInHelp]
        public async Task<RuntimeResult> SetConfigAsync(string varName, string varValue, bool restartBot = false)
        {
            if (Context.User.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            AdminService.ChangingConfig = true;

            PropertyInfo retrievedVariable = typeof(JsonConfig).GetProperty(varName);

            if (retrievedVariable == null)
                return ExecutionResult.FromError($"{varName} does not exist!");
            if (retrievedVariable.PropertyType is IList)
                return ExecutionResult.FromError($"{varName} is a list variable!");
            if(retrievedVariable.GetCustomAttribute<NotModifiable>() != null && !restartBot)
                return ExecutionResult.FromError($"{varName} cannot be modified at runtime! " +
                    $"Please pass `true` to the `restartBot` parameter.");

            retrievedVariable.SetValue(GlobalConfig.Instance.LoadedConfig, Convert.ChangeType(varValue, retrievedVariable.PropertyType));

            object newValue = retrievedVariable.GetValue(GlobalConfig.Instance.LoadedConfig);

            await ReplyAsync($"Set variable \"{varName}\" to `{newValue.ToString().Truncate(128)}` succesfully.");
            await File.WriteAllTextAsync(GlobalConfig.Instance.ConfigFile,
                JsonConvert.SerializeObject(GlobalConfig.Instance.LoadedConfig, Formatting.Indented));

            if (restartBot)
            {
                await ReplyAsync($"{GlobalConfig.Instance.LoadedConfig.BotName} will restart.");
                GlobalConfig.Instance.RestartBot();
            }

            AdminService.ChangingConfig = false;

            return ExecutionResult.Succesful();
        }
    }
}