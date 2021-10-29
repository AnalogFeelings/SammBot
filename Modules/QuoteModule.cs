using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using SammBotNET.Database;
using SammBotNET.Extensions;
using SammBotNET.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Quotes")]
    [Summary("Random quotes by users.")]
    [Group("quotes")]
    public class QuoteModule : ModuleBase<SocketCommandContext>
    {
        public QuoteService PhrasesService { get; set; }

        [Command("random", RunMode = RunMode.Async)]
        [Summary("Sends a random quote from a user in the server!")]
        public async Task<RuntimeResult> RandomAsync()
        {
            if (PhrasesService.IsDisabled)
                return ExecutionResult.FromError($"The module \"{nameof(QuoteModule)}\" is disabled.");

            using (PhrasesDB PhrasesDatabase = new())
            {
                List<Phrase> phrases = await PhrasesDatabase.Phrase.ToListAsync();
                Phrase finalPhrase = phrases.Where(x => x.ServerId == Context.Guild.Id).ToList().PickRandom();

                EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle(string.Empty);

                embed.AddField($"*\"{finalPhrase.Content}\"*", $"By <@{finalPhrase.AuthorId}>");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }

            return ExecutionResult.Succesful();
        }

        [Command("by", RunMode = RunMode.Async)]
        [Alias("from")]
        [Summary("Sends a quote from a user in the server!")]
        public async Task<RuntimeResult> PhraseAsync(IUser user)
        {
            if (PhrasesService.IsDisabled)
                return ExecutionResult.FromError($"The module \"{nameof(QuoteModule)}\" is disabled.");

            using (PhrasesDB PhrasesDatabase = new())
            {
                List<Phrase> phrases = await PhrasesDatabase.Phrase.ToListAsync();
                if (!phrases.Any(x => x.AuthorId == user.Id))
                    return ExecutionResult.FromError($"This user doesn't have any phrases!");

                Phrase finalPhrase = phrases.Where(x => x.AuthorId == user.Id && x.ServerId == Context.Guild.Id).ToList().PickRandom();

                EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle(string.Empty);

                embed.AddField($"*\"{finalPhrase.Content}\"*", $"By <@{finalPhrase.AuthorId}>");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }

            return ExecutionResult.Succesful();
        }
    }
}
