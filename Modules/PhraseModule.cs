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
        public async Task<RuntimeResult> RandomAsync()
        {
            if (PhrasesService.IsDisabled)
                return ExecutionResult.FromError($"The module \"{nameof(PhraseModule)}\" is disabled.");

            List<Phrase> phrases = await PhrasesDatabase.Phrase.ToListAsync();
            Phrase finalPhrase = phrases.Where(x => x.serverID == Context.Guild.Id).ToList().PickRandom();

            EmbedBuilder embed = new()
            {
                Color = Color.DarkPurple
            };

            embed.AddField($"**\"{finalPhrase.content}\"**", $"By <@{finalPhrase.authorID}>");
            await Context.Channel.SendMessageAsync("", false, embed.Build());

            return ExecutionResult.Succesful();
        }

        [Command("by", RunMode = RunMode.Async)]
        [Summary("Sends a quote from a user in the server!")]
        public async Task<RuntimeResult> PhraseAsync(IUser user)
        {
            if (PhrasesService.IsDisabled)
                return ExecutionResult.FromError($"The module \"{nameof(PhraseModule)}\" is disabled.");

            List<Phrase> phrases = await PhrasesDatabase.Phrase.ToListAsync();
            if (!phrases.Any(x => x.authorID == user.Id))
                return ExecutionResult.FromError($"This user doesn't have any phrases!");

            Phrase finalPhrase = phrases.Where(x => x.authorID == user.Id)
                .Where(x => x.serverID == Context.Guild.Id).ToList().PickRandom();

            EmbedBuilder embed = new()
            {
                Color = Color.DarkPurple
            };

            embed.AddField($"**\"{finalPhrase.content}\"**", $"By <@{finalPhrase.authorID}>");
            await Context.Channel.SendMessageAsync("", false, embed.Build());

            return ExecutionResult.Succesful();
        }
    }
}
