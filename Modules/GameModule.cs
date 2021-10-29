using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using SammBotNET.Extensions;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Games")]
    [Group("games")]
    [Summary("Games and fun!")]
    public class GameModule : ModuleBase<SocketCommandContext>
    {
        public readonly Logger Logger;
        public readonly DiscordSocketClient Client;

        public GameModule(Logger logger, DiscordSocketClient client)
        {
            Logger = logger;
            Client = client;
        }

        [Command("8ball")]
        [Alias("ask", "8")]
        [Summary("Ask the magic 8 ball!")]
        public async Task<RuntimeResult> MagicBallAsync([Remainder] string question)
        {
            string chosenAnswer = GlobalConfig.Instance.LoadedConfig.MagicBallAnswers.PickRandom();

            IUserMessage message = await ReplyAsync(":8ball: Asking the magic 8-ball...");

            using (Context.Channel.EnterTypingState()) await Task.Delay(2000);

            await message.ModifyAsync(x => x.Content = $"<@{Context.Message.Author.Id}> **The magic 8-ball answered**:\n`{chosenAnswer}`");

            return ExecutionResult.Succesful();
        }

        [Command("resttest")]
        public async Task<RuntimeResult> TestAsync(ulong id)
        {
            string assembledUsername = "No User Found";
            RestUser user = await Client.Rest.GetUserAsync(id);

            assembledUsername = $"{user.Username}#{user.DiscriminatorValue}";

            await ReplyAsync(assembledUsername);

            return ExecutionResult.Succesful();
        }
    }
}
