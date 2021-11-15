using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SammBotNET.Extensions;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Fun")]
    [Group("fun")]
    [Summary("Games and fun!")]
    public class GameModule : ModuleBase<SocketCommandContext>
    {
        public Logger Logger { get; set; }

        [Command("8ball")]
        [Alias("ask", "8")]
        [Summary("Ask the magic 8 ball!")]
        public async Task<RuntimeResult> MagicBallAsync([Remainder] string Question)
        {
            string chosenAnswer = GlobalConfig.Instance.LoadedConfig.MagicBallAnswers.PickRandom();

            IUserMessage message = await ReplyAsync(":8ball: Asking the magic 8-ball...");

            using (Context.Channel.EnterTypingState()) await Task.Delay(2000);

            await message.ModifyAsync(x => x.Content = $"<@{Context.Message.Author.Id}> **The magic 8-ball answered**:\n`{chosenAnswer}`");

            return ExecutionResult.Succesful();
        }

        [Command("dice")]
        [Alias("roll")]
        [Summary("Random number between 0 and FaceCount (6 if no parameter passed)")]
        public async Task<RuntimeResult> RollDiceAsync(int FaceCount = 6)
        {
            if (FaceCount < 3)
                return ExecutionResult.FromError("The dice must have at least 3 faces!");

            int chosenNumber = GlobalConfig.Instance.GlobalRng.Next(0, FaceCount + 1);

            IUserMessage message = await ReplyAsync(":game_die: Rolling the dice...");

            using (Context.Channel.EnterTypingState()) await Task.Delay(1500);

            await message.ModifyAsync(x => x.Content = $"The dice landed on **{chosenNumber}**!");

            return ExecutionResult.Succesful();
        }

        [Command("hug")]
        [Alias("cuddle")]
        [MustRunInGuild]
        [Summary("Hug a user!")]
        public async Task<RuntimeResult> HugUserAsync(IUser User)
        {
            string chosenKaomoji = GlobalConfig.Instance.LoadedConfig.HugKaomojis.PickRandom();

            SocketGuildUser authorAsGuild = Context.Message.Author as SocketGuildUser;

            string authorName = authorAsGuild.Nickname ?? authorAsGuild.Username;

            await ReplyAsync($"Warm hugs from **{authorName}**!\n{chosenKaomoji} <@{User.Id}>");

            return ExecutionResult.Succesful();
        }

        [Command("pat")]
        [MustRunInGuild]
        [Summary("Pats a user!")]
        public async Task<RuntimeResult> PatUserAsync(IUser User)
        {
            await ReplyAsync($"(c・_・)ノ”<@{User.Id}>");

            return ExecutionResult.Succesful();
        }
    }
}
