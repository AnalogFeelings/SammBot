using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Skyler")]
	[Group("support")]
    [Summary("For my cute angel <3")]
    [ModuleEmoji("💞")]
    public class EmotionalSupportModule : ModuleBase<SocketCommandContext>
    {
        [Command("random", RunMode = RunMode.Async)]
        [Summary("I love you, Skyler <3")]
        public async Task<RuntimeResult> RandomSupportAsync()
        {
            if (Context.Message.Author.Id != BotCore.Instance.LoadedConfig.SkylerUid &&
                Context.Message.Author.Id != BotCore.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            using (EmotionalSupportDB SupportDatabase = new())
            {
                List<EmotionalSupport> emotionalSupports = await SupportDatabase.EmotionalSupport.ToListAsync();
                EmotionalSupport finalEmotionalSupport = emotionalSupports.PickRandom();

                await ReplyAsync(finalEmotionalSupport.SupportMessage);
            }

            return ExecutionResult.Succesful();
        }

        [Command("edit", RunMode = RunMode.Async)]
        [Summary("Edits a support message!")]
        public async Task<RuntimeResult> EditSupportAsync(int SupportId, [Remainder] string SupportMessage)
        {
            if (Context.Message.Author.Id != BotCore.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            using (EmotionalSupportDB SupportDatabase = new())
            {
                List<EmotionalSupport> supportObjects = await SupportDatabase.EmotionalSupport.ToListAsync();
                EmotionalSupport supportObject = supportObjects.FirstOrDefault(x => x.SupportId == SupportId);

                if (supportObject == null)
                    return ExecutionResult.FromError($"A support message with the ID {SupportId} does not exist!");

                string previousValue = supportObject.SupportMessage;

                supportObject.SupportMessage = SupportMessage;
                await SupportDatabase.SaveChangesAsync();

                await ReplyAsync($"Done!\n**Previous value**: `{previousValue}`\n**New value**: `{SupportMessage}`");
            }

            return ExecutionResult.Succesful();
        }

        [Command("list", RunMode = RunMode.Async)]
        [Alias("all")]
        [Summary("Lists all of the support messages!")]
        public async Task<RuntimeResult> ListSupportAsync()
        {
            if (Context.Message.Author.Id != BotCore.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            using (EmotionalSupportDB SupportDatabase = new())
            {
                List<EmotionalSupport> supportObjects = await SupportDatabase.EmotionalSupport.ToListAsync();

                string builtMsg = "```\n";
                string inside = string.Empty;

                foreach (EmotionalSupport supportObject in supportObjects)
                {
                    inside += $"ID {supportObject.SupportId} :: \"{supportObject.SupportMessage}\"\n";
                }
                inside += "```";
                builtMsg += inside;
                await ReplyAsync(builtMsg);
            }

            return ExecutionResult.Succesful();
        }

        [Command("add", RunMode = RunMode.Async)]
        [Summary("Adds a cute little support message!")]
        public async Task<RuntimeResult> AddSupportAsync(string SupportMessage)
        {
            if (Context.Message.Author.Id != BotCore.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            using (EmotionalSupportDB SupportDatabase = new())
            {
                List<EmotionalSupport> supportList = await SupportDatabase.EmotionalSupport.ToListAsync();
                int nextId = 0;
                if (supportList.Count > 0)
                {
                    nextId = supportList.Max(x => x.SupportId) + 1;
                }

                await SupportDatabase.AddAsync(new EmotionalSupport
                {
                    SupportId = nextId,
                    SupportMessage = SupportMessage
                });
                await SupportDatabase.SaveChangesAsync();

                await ReplyAsync($"Done! Added new support object with ID {nextId}.");
            }

            return ExecutionResult.Succesful();
        }
    }
}
