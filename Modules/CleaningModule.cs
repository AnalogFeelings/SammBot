using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
	[Name("Janitoring")]
	[Group("clean")]
	[Summary("Clearing and Purging.")]
	[ModuleEmoji("🧹")]
	public class CleaningModule : ModuleBase<SocketCommandContext>
	{
		[Command("tags")]
		[Alias("usertags")]
		[Summary("Deletes all tags.")]
		[BotOwnerOnly]
		public async Task<RuntimeResult> FlushTagsAsync()
		{
			using (TagDB CommandDatabase = new TagDB())
			{
				await ReplyAsync("Flushing database...");
				int AffectedRows = await CommandDatabase.Database.ExecuteSqlRawAsync("delete from UserTag");
				await ReplyAsync($"Success! `{AffectedRows}` rows affected.");
			}

			return ExecutionResult.Succesful();
		}

		[Command("quotes")]
		[Alias("phrases")]
		[Summary("Deletes all quotes.")]
		[BotOwnerOnly]
		public async Task<RuntimeResult> FlushQuotesAsync()
		{
			using (PhrasesDB PhrasesDatabase = new PhrasesDB())
			{
				await ReplyAsync("Flushing database...");
				int AffectedRows = await PhrasesDatabase.Database.ExecuteSqlRawAsync("delete from Phrase");
				await ReplyAsync($"Success! `{AffectedRows}` rows affected.");
			}

			return ExecutionResult.Succesful();
		}

		[Command("messages")]
		[Summary("Deletes an amount of messages.")]
		[RequireBotPermission(GuildPermission.ManageMessages)]
		[RequireUserPermission(GuildPermission.ManageMessages)]
		public async Task<RuntimeResult> FlushMessagesAsync(int Count)
		{
			IEnumerable<IMessage> RetrievedMessages = await Context.Message.Channel.GetMessagesAsync(Count + 1).FlattenAsync();

			await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(RetrievedMessages);

			IUserMessage SuccessMessage = await ReplyAsync($"Success! Cleared `{Count}` message/s.");

			await Task.Delay(3000);
			await SuccessMessage.DeleteAsync();

			return ExecutionResult.Succesful();
		}
	}
}
