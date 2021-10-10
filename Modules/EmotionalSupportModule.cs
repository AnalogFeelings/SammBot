using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using SammBotNET.Database;
using SammBotNET.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("For Neveah <3")]
    [Group("support")]
    public class EmotionalSupportModule : ModuleBase<SocketCommandContext>
    {
        public EmotionalSupportService SupportService { get; set; }
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

        [Command("add")]
        [Summary("Adds a cute little support message!")]
        public async Task AddSupportAsync([Remainder] string supportMessage)
        {
            if (Context.Message.Author.Id != 337950448130719754) return;

            if (supportMessage == string.Empty)
            {
                await ReplyAsync("Hey, you didn't give me a support message to add!");
                return;
            }

            await SupportDatabase.AddAsync(new EmotionalSupport
            {
                SupportMessage = supportMessage
            });
            await SupportDatabase.SaveChangesAsync();

            await ReplyAsync("Done!");
        }
    }
}
