using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using SammBotNET.Core;
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
        public CommandHandler CommandHandler { get; set; }

        [Command("say", RunMode = RunMode.Async)]
        [HideInHelp]
        public async Task<RuntimeResult> SayMessageAsync([Remainder] string Message)
        {
            if (Context.Message.Author.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            if (AdminService.ChannelId == 0 || AdminService.GuildId == 0)
                return ExecutionResult.FromError("Please set a guild and channel ID beforehand!");

            SocketTextChannel channel = Context.Client.GetGuild(AdminService.GuildId).GetTextChannel(AdminService.ChannelId);

            using (channel.EnterTypingState()) await channel.SendMessageAsync(Message);

            return ExecutionResult.Succesful();
        }

        [Command("setsay", RunMode = RunMode.Async)]
        [HideInHelp]
        public async Task<RuntimeResult> SetSayAsync(ulong Channel, ulong Guild)
        {
            if (Context.Message.Author.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            if (Context.Client.GetGuild(Guild) == null) return ExecutionResult.FromError("I am not invited in that guild!");
            if (Context.Client.GetGuild(Guild).GetTextChannel(Channel) == null)
                return ExecutionResult.FromError($"Channel with ID {Channel} does not exist in guild with ID {Guild}.");

            AdminService.ChannelId = Channel;
            AdminService.GuildId = Guild;

            SocketGuild socketGuild = Context.Client.GetGuild(Guild);

            await ReplyAsync($"Success. Set guild to `{socketGuild.Name}` and channel to `{socketGuild.GetTextChannel(Channel).Name}`.");

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
        public async Task<RuntimeResult> LeaveAsync(ulong ServerId)
        {
            if (Context.User.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            SocketGuild guild = Context.Client.GetGuild(ServerId);
            if (guild == null)
                return ExecutionResult.FromError("I am not currently in this guild!");

            string guildName = guild.Name;
            await guild.LeaveAsync();

            await ReplyAsync($"Left the server \"{guildName}\".");

            return ExecutionResult.Succesful();
        }

        [Command("mime")]
        [Alias("mimic, spy")]
        [HideInHelp]
        public async Task<RuntimeResult> MimicUserAsync(SocketGuildUser User, string Command)
        {
            if (Context.User.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

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
        [HideInHelp]
        public async Task<RuntimeResult> ListConfigAsync(bool Override = false)
        {
            if (Context.User.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context, "Configuration File");

            List<PropertyInfo> properties = typeof(JsonConfig).GetProperties()
                .Where(x => !x.PropertyType.IsGenericType &&
                        x.Name != "UrlRegex" &&
                        x.Name != "BotToken").ToList();

            if (!Override)
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
        public async Task<RuntimeResult> SetConfigAsync(string VarName, string VarValue, bool RestartBot = false)
        {
            if (Context.User.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            PropertyInfo retrievedVariable = typeof(JsonConfig).GetProperty(VarName);

            if (retrievedVariable == null)
                return ExecutionResult.FromError($"{VarName} does not exist!");

            if (retrievedVariable.PropertyType is IList)
                return ExecutionResult.FromError($"{VarName} is a list variable!");

            if (retrievedVariable.GetCustomAttribute<NotModifiable>() != null && !RestartBot)
                return ExecutionResult.FromError($"{VarName} cannot be modified at runtime! " +
                    $"Please pass `true` to the `restartBot` parameter.");

            AdminService.ChangingConfig = true;

            retrievedVariable.SetValue(GlobalConfig.Instance.LoadedConfig, Convert.ChangeType(VarValue, retrievedVariable.PropertyType));

            object newValue = retrievedVariable.GetValue(GlobalConfig.Instance.LoadedConfig);

            await ReplyAsync($"Set variable \"{VarName}\" to `{newValue.ToString().Truncate(128)}` succesfully.");
            await File.WriteAllTextAsync(GlobalConfig.Instance.ConfigFile,
                JsonConvert.SerializeObject(GlobalConfig.Instance.LoadedConfig, Formatting.Indented));

            if (RestartBot)
            {
                await ReplyAsync($"{GlobalConfig.Instance.LoadedConfig.BotName} will restart.");
                GlobalConfig.Instance.RestartBot();
            }

            AdminService.ChangingConfig = false;

            return ExecutionResult.Succesful();
        }
    }
}