﻿using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Quotes")]
    [Group("quotes")]
    [Summary("Random quotes by users.")]
    [ModuleEmoji("🗣")]
    public class QuoteModule : ModuleBase<SocketCommandContext>
    {
        public QuoteService PhrasesService { get; set; }
        public DiscordSocketClient Client { get; set; }

        [Command("random")]
        [Summary("Sends a random quote from a user in the server!")]
        public async Task<RuntimeResult> RandomAsync()
        {
            if (PhrasesService.IsDisabled)
                return ExecutionResult.FromError($"The module \"{nameof(QuoteModule)}\" is disabled.");

            using (PhrasesDB PhrasesDatabase = new PhrasesDB())
            {
                List<Phrase> phrases = await PhrasesDatabase.Phrase.ToListAsync();
                if (phrases.Count == 0) return ExecutionResult.FromError("I have no quotes in my record!");

                Phrase finalPhrase = phrases.Where(x => x.ServerId == Context.Guild.Id).ToList().PickRandom();

                RestUser globalUser = await Client.Rest.GetUserAsync(finalPhrase.AuthorId);
                string assembledAuthor = $"{globalUser.Username}#{globalUser.Discriminator}";

                EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle(string.Empty);

                embed.AddField($"*\"{finalPhrase.Content}\"*", $"- {assembledAuthor}, <t:{finalPhrase.CreatedAt}:D>");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }

            return ExecutionResult.Succesful();
        }

        [Command("by")]
        [Alias("from")]
        [Summary("Sends a quote from a user in the server!")]
        public async Task<RuntimeResult> PhraseAsync(IUser User)
        {
            if (PhrasesService.IsDisabled)
                return ExecutionResult.FromError($"The module \"{nameof(QuoteModule)}\" is disabled.");

            using (PhrasesDB PhrasesDatabase = new PhrasesDB())
            {
                List<Phrase> phrases = await PhrasesDatabase.Phrase.ToListAsync();
                if (!phrases.Any(x => x.AuthorId == User.Id && x.ServerId == Context.Guild.Id))
                    return ExecutionResult.FromError($"This user doesn't have any phrases!");

                Phrase finalPhrase = phrases.Where(x => x.AuthorId == User.Id && x.ServerId == Context.Guild.Id).ToList().PickRandom();

                RestUser globalUser = await Client.Rest.GetUserAsync(finalPhrase.AuthorId);
                string assembledAuthor = $"{globalUser.Username}#{globalUser.Discriminator}";

                EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle(string.Empty);

                embed.AddField($"*\"{finalPhrase.Content}\"*", $"- {assembledAuthor}, <t:{finalPhrase.CreatedAt}:D>");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }

            return ExecutionResult.Succesful();
        }
    }
}
