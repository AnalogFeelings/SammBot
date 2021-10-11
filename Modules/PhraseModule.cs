using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using SammBotNET.Database;
using SammBotNET.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SammBotNET.Extensions;

namespace SammBotNET.Modules
{
    [Name("User Phrases")]
    [Group("phrases")]
    public class PhraseModule : ModuleBase<SocketCommandContext>
    {
        public PhraseService PhrasesService { get; set; }
        private readonly PhrasesDB PhrasesDatabase;

        public PhraseModule(IServiceProvider services)
        {
            PhrasesDatabase = services.GetRequiredService<PhrasesDB>();
        }

        [Command("random", RunMode = RunMode.Async)]
        [Summary("Sends a random quote from a user in the server!")]
        public async Task RandomAsync()
        {
            if (PhrasesService.IsDisabled)
            {
                await ReplyAsync($"The module \"{this.GetType().Name}\" is disabled.");
                return;
            }
            List<Phrase> phrases = await PhrasesDatabase.Phrase.ToListAsync();
            Phrase finalPhrase = phrases.Where(x => x.serverID == Context.Guild.Id).ToList().PickRandom();

            EmbedBuilder embed = new EmbedBuilder
            {
                Color = Color.DarkPurple
            };

            embed.AddField($"**\"{finalPhrase.content}\"**", $"By <@{finalPhrase.authorID}>");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("by", RunMode = RunMode.Async)]
        [Summary("Sends a quote from a user in the server!")]
        public async Task PhraseAsync([Remainder] string userID)
        {
            if (PhrasesService.IsDisabled)
            {
                await ReplyAsync($"The module \"{this.GetType().Name}\" is disabled.");
                return;
            }
            if (!ulong.TryParse(userID, out ulong id))
            {
                await ReplyAsync("The user ID is invalid.");
                return;
            }

            List<Phrase> phrases = await PhrasesDatabase.Phrase.ToListAsync();
            if (!phrases.Any(x => x.authorID == id))
            {
                await ReplyAsync($"This user doesn't have any phrases!");
                return;
            }
            Phrase finalPhrase = phrases.Where(x => x.authorID == id)
                .Where(x => x.serverID == Context.Guild.Id).ToList().PickRandom();

            EmbedBuilder embed = new EmbedBuilder
            {
                Color = Color.DarkPurple
            };

            embed.AddField($"**\"{finalPhrase.content}\"**", $"By <@{finalPhrase.authorID}>");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}
