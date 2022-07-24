using Discord;
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
		public DiscordSocketClient Client { get; set; }

		[Command("random")]
		[Summary("Sends a random quote from a user in the server!")]
		[FullDescription("Remember those funny quote memes? Now you can kind of recreate them with this command!")]
		[MustRunInGuild]
		public async Task<RuntimeResult> RandomAsync()
		{
			using (PhrasesDB PhrasesDatabase = new PhrasesDB())
			{
				List<Phrase> QuoteList = await PhrasesDatabase.Phrase.ToListAsync();
				if (QuoteList.Count == 0) return ExecutionResult.FromError("I have no quotes in my record!");

				List<Phrase> ServerQuotes = QuoteList.Where(x => x.ServerId == Context.Guild.Id).ToList();
				if (ServerQuotes.Count == 0) return ExecutionResult.FromError("I have no quotes in my record!");

				Phrase ChosenQuote = QuoteList.Where(x => x.ServerId == Context.Guild.Id).ToList().PickRandom();
				RestUser GlobalAuthor = await Client.Rest.GetUserAsync(ChosenQuote.AuthorId);

				EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle(string.Empty);

				ReplyEmbed.AddField($"*\"{ChosenQuote.Content}\"*", $"- {GlobalAuthor.GetFullUsername()}, <t:{ChosenQuote.CreatedAt}:D>");

				MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
				AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
				await ReplyAsync(null, false, ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);
			}

			return ExecutionResult.Succesful();
		}

		[Command("by")]
		[Alias("from")]
		[Summary("Sends a quote from a user in the server!")]
		[FullDescription("Gets a random quote from the specified user!")]
		[MustRunInGuild]
		public async Task<RuntimeResult> PhraseAsync(IUser User)
		{
			using (PhrasesDB PhrasesDatabase = new PhrasesDB())
			{
				List<Phrase> QuoteList = await PhrasesDatabase.Phrase.ToListAsync();
				if (!QuoteList.Any(x => x.AuthorId == User.Id && x.ServerId == Context.Guild.Id))
					return ExecutionResult.FromError($"This user doesn't have any quotes!");

				Phrase ChosenQuote = QuoteList.Where(x => x.AuthorId == User.Id && x.ServerId == Context.Guild.Id).ToList().PickRandom();

				RestUser GlobalAuthor = await Client.Rest.GetUserAsync(ChosenQuote.AuthorId);

				EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle(string.Empty);

				ReplyEmbed.AddField($"*\"{ChosenQuote.Content}\"*", $"- {GlobalAuthor.GetFullUsername()}, <t:{ChosenQuote.CreatedAt}:D>");

				MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
				AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
				await ReplyAsync(null, false, ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);
			}

			return ExecutionResult.Succesful();
		}
	}
}
