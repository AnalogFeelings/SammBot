using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using SammBotNET.Database;
using SammBotNET.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Neveah")]
    [Summary("For my cute angel <3")]
    [Group("support")]
    public class EmotionalSupportModule : ModuleBase<SocketCommandContext>
    {
        private readonly EmotionalSupportDB SupportDatabase;

        public EmotionalSupportModule(IServiceProvider service)
        {
            SupportDatabase = service.GetRequiredService<EmotionalSupportDB>();
        }

        [Command("random", RunMode = RunMode.Async)]
        [Summary("I love you, Neveah <3")]
        public async Task<RuntimeResult> RandomSupportAsync()
        {
            if (Context.Message.Author.Id != GlobalConfig.Instance.LoadedConfig.NeveahUid &&
                Context.Message.Author.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            List<EmotionalSupport> emotionalSupports = await SupportDatabase.EmotionalSupport.ToListAsync();
            EmotionalSupport finalEmotionalSupport = emotionalSupports.PickRandom();

            await ReplyAsync(finalEmotionalSupport.SupportMessage);

            return ExecutionResult.Succesful();
        }

        [Command("edit", RunMode = RunMode.Async)]
        [Summary("Edits a support message!")]
        public async Task<RuntimeResult> EditSupportAsync(int supportId, [Remainder] string supportMessage)
        {
            if (Context.Message.Author.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            List<EmotionalSupport> supportObjects = await SupportDatabase.EmotionalSupport.ToListAsync();
            EmotionalSupport supportObject = supportObjects.FirstOrDefault(x => x.SupportId == supportId);

            if (supportObject == null)
                return ExecutionResult.FromError($"A support message with the ID {supportId} does not exist!");

            string previousValue = supportObject.SupportMessage;

            supportObject.SupportMessage = supportMessage;
            await SupportDatabase.SaveChangesAsync();

            await ReplyAsync($"Done!\n**Previous value**: `{previousValue}`\n**New value**: `{supportMessage}`");

            return ExecutionResult.Succesful();
        }

        [Command("list", RunMode = RunMode.Async)]
        [Summary("Lists all of the support messages!")]
        public async Task<RuntimeResult> ListSupportAsync()
        {
            if (Context.Message.Author.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

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

            return ExecutionResult.Succesful();
        }

        [Command("add", RunMode = RunMode.Async)]
        [Summary("Adds a cute little support message!")]
        public async Task<RuntimeResult> AddSupportAsync(string supportMessage)
        {
            if (Context.Message.Author.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            List<EmotionalSupport> supportList = await SupportDatabase.EmotionalSupport.ToListAsync();
            int nextId = 0;
            if (supportList.Count > 0)
            {
                nextId = supportList.Max(x => x.SupportId) + 1;
            }

            await SupportDatabase.AddAsync(new EmotionalSupport
            {
                SupportId = nextId,
                SupportMessage = supportMessage
            });
            await SupportDatabase.SaveChangesAsync();

            await ReplyAsync($"Done! Added new support object with ID {nextId}.");

            return ExecutionResult.Succesful();
        }
    }
}
