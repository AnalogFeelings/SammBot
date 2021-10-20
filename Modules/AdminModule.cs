using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using SammBotNET.Extensions;
using SammBotNET.Services;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Administration")]
    [Group("admin")]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        public AdminService AdminService { get; set; }

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

        [Command("setcfg")]
        [Alias("config")]
        [HideInHelp]
        public async Task<RuntimeResult> SetConfigAsync(string varName, string varValue)
        {
            if (Context.User.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            PropertyInfo retrievedVariable = typeof(JsonConfig).GetProperty(varName);

            if (retrievedVariable == null)
                return ExecutionResult.FromError($"{varName} does not exist!");
            if (retrievedVariable.PropertyType is IList)
                return ExecutionResult.FromError($"{varName} is a list variable!");

            retrievedVariable.SetValue(GlobalConfig.Instance.LoadedConfig, Convert.ChangeType(varValue, retrievedVariable.PropertyType));

            object newValue = retrievedVariable.GetValue(GlobalConfig.Instance.LoadedConfig);

            await ReplyAsync($"Set variable \"{varName}\" to `{newValue.ToString().Truncate(128)}` succesfully.");
            await ReplyAsync($"{GlobalConfig.Instance.LoadedConfig.BotName} will restart.");

            await File.WriteAllTextAsync(GlobalConfig.Instance.ConfigFile,
                JsonConvert.SerializeObject(GlobalConfig.Instance.LoadedConfig, Formatting.Indented));

            string restartTimeoutCmd = $"/C timeout 3 && {Process.GetCurrentProcess().MainModule.FileName}";
            string restartFileCmd = "cmd.exe";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                restartTimeoutCmd = $"-c 'sleep 3s && ./{Process.GetCurrentProcess().MainModule.FileName}'";
                restartFileCmd = "bash";
            }

            ProcessStartInfo startInfo = new()
            {
                Arguments = restartTimeoutCmd,
                CreateNoWindow = true,
                FileName = restartFileCmd
            };
            Process.Start(startInfo);
            Environment.Exit(0);

            return ExecutionResult.Succesful();
        }
    }
}