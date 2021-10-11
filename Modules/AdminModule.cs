using Discord.Commands;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Administration")]
    [Group("admin")]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [Command("say", RunMode = RunMode.Async)]
        public async Task SayMessageAsync([Remainder] string message)
        {
            if (Context.Message.Author.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
            {
                return;
            }

            await Context.Client.GetGuild(850875298290597898).GetTextChannel(850875298290597902).SendMessageAsync(message);
        }
    }
}