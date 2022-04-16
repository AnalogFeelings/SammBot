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
        [MustRunInGuild]
        [Summary("Deletes a user tag.")]
        public async Task<RuntimeResult> DeleteTagAsync(string Name)
        {
            using (TagDB TagDatabase = new TagDB())
            {
                List<UserTag> userTags = await TagDatabase.UserTag.ToListAsync();
                UserTag userTag = null;

                if (Context.Message.Author.Id == BotCore.Instance.LoadedConfig.AestheticalUid)
                    userTag = userTags.SingleOrDefault(x => x.Name == Name);
                else
                    userTag = userTags.SingleOrDefault(x => x.Name == Name && x.AuthorId == Context.User.Id);

                if (userTag == null)
                    return ExecutionResult.FromError($"The tag **\"{Name}\"** does not exist, or you don't have permission to delete it.");

                TagDatabase.Remove(userTag);
                await TagDatabase.SaveChangesAsync();
            }
            await ReplyAsync("Success!");

            return ExecutionResult.Succesful();
        }

        [Command("get")]
        [Alias("what")]
        [MustRunInGuild]
        [Summary("Gets a tag by its name, and replies.")]
        public async Task<RuntimeResult> GetTagAsync(string Name)
        {
            using (TagDB TagDatabase = new TagDB())
            {
                List<UserTag> userTags = await TagDatabase.UserTag.ToListAsync();
                UserTag userTag = userTags.SingleOrDefault(x => x.ServerId == Context.Guild.Id && x.Name == Name);

                if (userTag == null)
                    return ExecutionResult.FromError($"The tag **\"{Name}\"** does not exist!");

                await ReplyAsync(userTag.Reply);
            }
            return ExecutionResult.Succesful();
        }

        [Command("search")]
        [Alias("find", "similar")]
        [MustRunInGuild]
        [Summary("Searches for similar tags.")]
        public async Task<RuntimeResult> SearchTagsAsync(string Name)
        {
            using (TagDB TagDatabase = new TagDB())
            {
                List<UserTag> userTags = await TagDatabase.UserTag.ToListAsync();
                List<UserTag> validTags = userTags.Where(x => x.ServerId == Context.Guild.Id &&
                        Name.DamerauLevenshteinDistance(x.Name, BotCore.Instance.LoadedConfig.TagDistance) < int.MaxValue).Take(25).ToList();

                EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context, description: $"All of the tags similar to \"{Name}\".")
                    .ChangeTitle("TAG RESULTS");

                foreach (UserTag tag in validTags)
                {
                    RestUser user = await Context.Client.Rest.GetUserAsync(tag.AuthorId);
                    string userName = user != null ? $"{user.Username}#{user.Discriminator}" : "Unknown";

                    embed.AddField($"`{tag.Name}`", $"By: **{userName}**");
                }

                await Context.Channel.SendMessageAsync("", false, embed.Build(), messageReference: new MessageReference(Context.Message.Id));
            }

            return ExecutionResult.Succesful();
        }

        [Command("create")]
        [Alias("new")]
        [MustRunInGuild]
        [Summary("Creates a new tag.")]
        public async Task<RuntimeResult> CreateTagAsync(string Name, string Reply)
        {
            if (Name.Length > 15)
                return ExecutionResult.FromError("Please make the tag name shorter than 15 characters!");
            else if (Name.StartsWith(BotCore.Instance.LoadedConfig.BotPrefix))
                return ExecutionResult.FromError("Tag names can't begin with my prefix!");
            else if (Context.Message.MentionedUsers.Count > 0)
                return ExecutionResult.FromError("Tag names or replies cannot contain mentions!");
            else if (Name.Contains(' '))
                return ExecutionResult.FromError("Tag names cannot contain spaces!");

            using (TagDB TagDatabase = new TagDB())
            {
                List<UserTag> userTags = await TagDatabase.UserTag.ToListAsync();
                userTags = userTags.Where(x => x.ServerId == Context.Guild.Id).ToList();

                foreach (UserTag userTag in userTags)
                {
                    if (Name == userTag.Name) return ExecutionResult.FromError($"There's already a tag called **\"{Name}\"**!");
                }

                int nextId = 0;
                if (userTags.Count > 0) nextId = userTags.Max(x => x.Id) + 1;

                await TagDatabase.AddAsync(new UserTag
                {
                    Id = nextId,
                    Name = Name,
                    AuthorId = Context.Message.Author.Id,
                    ServerId = Context.Guild.Id,
                    Reply = Reply,
                    CreatedAt = Context.Message.Timestamp.ToUnixTimeSeconds()
                });
                await TagDatabase.SaveChangesAsync();
            }

            await ReplyAsync($"Tag created succesfully! Use `{BotCore.Instance.LoadedConfig.BotPrefix}tags get {Name}` to use it!");

            return ExecutionResult.Succesful();
        }
    }
}
