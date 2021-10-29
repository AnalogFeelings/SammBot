using Discord;
using Discord.Commands;
using SammBotNET.Extensions;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Fun")]
    [Group("fun")]
    [Summary("Games and fun!")]
    public class GameModule : ModuleBase<SocketCommandContext>
    {
        public readonly Logger Logger;

        public GameModule(Logger logger) => Logger = logger;

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

        [Name("Greentext")]
        [Group("greentext")]
        [Summary("Greentext, 4chan style.")]
        public class GreentextSubmodule : ModuleBase<SocketCommandContext>
        {
            [Command("test")]
            public async Task<RuntimeResult> TestAsync()
            {
                return ExecutionResult.Succesful();
            }
        }
    }
}
