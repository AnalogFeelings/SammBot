﻿using Discord.Commands;
using Discord.WebSocket;
using SammBotNET.Extensions;
using SammBotNET.Services;
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
    }
}