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
		[FullDescription("Dumps the entire user tag database.")]
		[RequireOwner]
		public async Task<RuntimeResult> FlushTagsAsync()
		{
			using (TagDB CommandDatabase = new TagDB())
			{
				MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
				AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
				IUserMessage Message = await ReplyAsync("Flushing database...", allowedMentions: AllowedMentions, messageReference: Reference);

				int AffectedRows = await CommandDatabase.Database.ExecuteSqlRawAsync("delete from UserTag");

				await Message.ModifyAsync(x => x.Content = $"Success! `{AffectedRows}` rows affected.");
			}

			return ExecutionResult.Succesful();
		}

		[Command("quotes")]
		[Alias("phrases")]
		[Summary("Deletes all quotes.")]
		[FullDescription("Dumps the entire quote database.")]
		[RequireOwner]
		public async Task<RuntimeResult> FlushQuotesAsync()
		{
			using (PhrasesDB PhrasesDatabase = new PhrasesDB())
			{
				MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
				AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
				IUserMessage Message = await ReplyAsync("Flushing database...", allowedMentions: AllowedMentions, messageReference: Reference);

				int AffectedRows = await PhrasesDatabase.Database.ExecuteSqlRawAsync("delete from Phrase");

				await Message.ModifyAsync(x => x.Content = $"Success! `{AffectedRows}` rows affected.");
			}

			return ExecutionResult.Succesful();
		}

		[Command("messages")]
		[Summary("Deletes an amount of messages.")]
		[FullDescription("Deletes the provided amount of messages.")]
		[RequireBotPermission(GuildPermission.ManageMessages)]
		[RequireUserPermission(GuildPermission.ManageMessages)]
		public async Task<RuntimeResult> FlushMessagesAsync(int Count)
		{
			IEnumerable<IMessage> RetrievedMessages = await Context.Message.Channel.GetMessagesAsync(Count + 1).FlattenAsync();

			await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(RetrievedMessages);

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			IUserMessage SuccessMessage = await ReplyAsync($"Success! Cleared `{Count}` message/s.", allowedMentions: AllowedMentions, messageReference: Reference);

			await Task.Delay(3000);
			await SuccessMessage.DeleteAsync();

			return ExecutionResult.Succesful();
		}
	}
}
