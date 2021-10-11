using Discord.Commands;
using SammBotNET.Extensions;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Administration")]
    [Group("admin")]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [Command("say", RunMode = RunMode.Async)]
        public async Task<RuntimeResult> SayMessageAsync([Remainder] string message)
        {
            if (Context.Message.Author.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            await Context.Client.GetGuild(850875298290597898).GetTextChannel(850875298290597902).SendMessageAsync(message);

            return ExecutionResult.Succesful();
        }
    }
}