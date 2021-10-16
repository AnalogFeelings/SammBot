using Discord.Commands;
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
        public async Task<RuntimeResult> SayMessageAsync([Remainder] string message)
        {
            if (Context.Message.Author.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            SocketTextChannel channel = Context.Client.GetGuild(AdminService.GuildId).GetTextChannel(AdminService.ChannelId);

            using (channel.EnterTypingState()) await channel.SendMessageAsync(message);

            return ExecutionResult.Succesful();
        }

        [Command("setsay", RunMode = RunMode.Async)]
        public async Task<RuntimeResult> SetSayAsync(ulong channel, ulong guild)
        {
            if (Context.Message.Author.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            AdminService.ChannelId = channel;
            AdminService.GuildId = guild;

            return ExecutionResult.Succesful();
        }
    }
}