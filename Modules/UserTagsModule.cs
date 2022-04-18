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
	[Name("User Tags")]
	[Group("tags")]
	[Summary("Tags that reply with a message when searched.")]
	[ModuleEmoji("🏷")]
	public class UserTagsModule : ModuleBase<SocketCommandContext>
	{
		public StartupService StartupService { get; set; }
		public DiscordSocketClient Client { get; set; }
		public CommandService CommandService { get; set; }

		[Command("delete")]
		[Alias("remove", "destroy")]
		[Summary("Deletes a user tag.")]
		[MustRunInGuild]
		public async Task<RuntimeResult> DeleteTagAsync(string Name)
		{
			using (TagDB TagDatabase = new TagDB())
			{
				List<UserTag> TagList = await TagDatabase.UserTag.ToListAsync();
				UserTag RetrievedTag = null;

				if (Context.Message.Author.Id == Settings.Instance.LoadedConfig.OwnerUserId)
					RetrievedTag = TagList.SingleOrDefault(x => x.Name == Name);
				else
					RetrievedTag = TagList.SingleOrDefault(x => x.Name == Name && x.AuthorId == Context.User.Id);

				if (RetrievedTag == null)
					return ExecutionResult.FromError($"The tag **\"{Name}\"** does not exist, or you don't have permission to delete it.");

				TagDatabase.Remove(RetrievedTag);
				await TagDatabase.SaveChangesAsync();
			}

			await ReplyAsync("Success!");

			return ExecutionResult.Succesful();
		}

		[Command("get")]
		[Alias("what")]
		[Summary("Gets a tag by its name, and replies.")]
		[MustRunInGuild]
		public async Task<RuntimeResult> GetTagAsync(string Name)
		{
			using (TagDB TagDatabase = new TagDB())
			{
				List<UserTag> TagList = await TagDatabase.UserTag.ToListAsync();
				UserTag RetrievedTag = TagList.SingleOrDefault(x => x.ServerId == Context.Guild.Id && x.Name == Name);

				if (RetrievedTag == null)
					return ExecutionResult.FromError($"The tag **\"{Name}\"** does not exist!");

				await ReplyAsync(RetrievedTag.Reply);
			}

			return ExecutionResult.Succesful();
		}

		[Command("search")]
		[Alias("find", "similar")]
		[Summary("Searches for similar tags.")]
		[MustRunInGuild]
		public async Task<RuntimeResult> SearchTagsAsync(string Name)
		{
			using (TagDB TagDatabase = new TagDB())
			{
				List<UserTag> TagList = await TagDatabase.UserTag.ToListAsync();
				List<UserTag> FilteredTags = TagList.Where(x => x.ServerId == Context.Guild.Id &&
							Name.DamerauLevenshteinDistance(x.Name, Settings.Instance.LoadedConfig.TagDistance) < int.MaxValue).Take(25).ToList();

				EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, Description: $"All of the tags similar to \"{Name}\".")
					.ChangeTitle("TAG RESULTS");

				foreach (UserTag Tag in FilteredTags)
				{
					RestUser GlobalAuthor = await Context.Client.Rest.GetUserAsync(Tag.AuthorId);
					string userName = GlobalAuthor != null ? GlobalAuthor.GetFullUsername() : "Unknown";

					ReplyEmbed.AddField($"`{Tag.Name}`", $"By: **{userName}**");
				}

				await Context.Channel.SendMessageAsync("", false, ReplyEmbed.Build(), messageReference: new MessageReference(Context.Message.Id));
			}

			return ExecutionResult.Succesful();
		}

		[Command("create")]
		[Alias("new")]
		[Summary("Creates a new tag.")]
		[MustRunInGuild]
		public async Task<RuntimeResult> CreateTagAsync(string Name, string Reply)
		{
			if (Name.Length > 15)
				return ExecutionResult.FromError("Please make the tag name shorter than 15 characters!");
			else if (Name.StartsWith(Settings.Instance.LoadedConfig.BotPrefix))
				return ExecutionResult.FromError("Tag names can't begin with my prefix!");
			else if (Context.Message.MentionedUsers.Count > 0)
				return ExecutionResult.FromError("Tag names or replies cannot contain mentions!");
			else if (Name.Contains(' '))
				return ExecutionResult.FromError("Tag names cannot contain spaces!");

			using (TagDB TagDatabase = new TagDB())
			{
				List<UserTag> TagList = await TagDatabase.UserTag.ToListAsync();
				TagList = TagList.Where(x => x.ServerId == Context.Guild.Id).ToList();

				foreach (UserTag Tag in TagList)
				{
					if (Name == Tag.Name) return ExecutionResult.FromError($"There's already a tag called **\"{Name}\"**!");
				}

				int NextId = 0;
				if (TagList.Count > 0) NextId = TagList.Max(x => x.Id) + 1;

				await TagDatabase.AddAsync(new UserTag
				{
					Id = NextId,
					Name = Name,
					AuthorId = Context.Message.Author.Id,
					ServerId = Context.Guild.Id,
					Reply = Reply,
					CreatedAt = Context.Message.Timestamp.ToUnixTimeSeconds()
				});
				await TagDatabase.SaveChangesAsync();
			}

			await ReplyAsync($"Tag created succesfully! Use `{Settings.Instance.LoadedConfig.BotPrefix}tags get {Name}` to use it!");

			return ExecutionResult.Succesful();
		}
	}
}
