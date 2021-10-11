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
    [Name("Neveah <3")]
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
        public async Task RandomSupportAsync()
        {
            if (Context.Message.Author.Id != 850874605434175500 && Context.Message.Author.Id != 337950448130719754) return;

            List<EmotionalSupport> emotionalSupports = await SupportDatabase.EmotionalSupport.ToListAsync();
            EmotionalSupport finalEmotionalSupport = emotionalSupports.PickRandom();

            await ReplyAsync(finalEmotionalSupport.SupportMessage);
        }

        [Command("edit", RunMode = RunMode.Async)]
        [Summary("Edits a support message!")]
        public async Task EditSupportAsync(int supportId, [Remainder] string supportMessage)
        {
            if (Context.Message.Author.Id != 337950448130719754) return;

            List<EmotionalSupport> supportObjects = await SupportDatabase.EmotionalSupport.ToListAsync();
            EmotionalSupport supportObject = supportObjects.FirstOrDefault(x => x.SupportId == supportId);
            string previousValue = string.Empty;

            if (supportObject == null)
            {
                await ReplyAsync($"A support message with the ID {supportId} does not exist!");
                return;
            }
            previousValue = supportObject.SupportMessage;

            supportObject.SupportMessage = supportMessage;
            await SupportDatabase.SaveChangesAsync();

            await ReplyAsync($"Done!\n**Previous value**: `{previousValue}`\n**New value**: `{supportMessage}`");
        }

        [Command("list", RunMode = RunMode.Async)]
        [Summary("Lists all of the support messages!")]
        public async Task ListSupportAsync()
        {
            if (Context.Message.Author.Id != 337950448130719754) return;

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

        [Command("add", RunMode = RunMode.Async)]
        [Summary("Adds a cute little support message!")]
        public async Task AddSupportAsync(string supportMessage)
        {
            if (Context.Message.Author.Id != 337950448130719754) return;

            if (supportMessage == string.Empty)
            {
                await ReplyAsync("Hey, you didn't give me a support message to add!");
                return;
            }

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
        }
    }
}
