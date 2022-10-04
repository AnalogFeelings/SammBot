using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("User Tags")]
    [Group("tags")]
    [Summary("Tags that reply with a message when searched.")]
    [ModuleEmoji("\U0001f3f7\uFE0F")]
    public class UserTagsModule : ModuleBase<SocketCommandContext>
    {
        [Command("delete")]
        [Alias("remove", "destroy")]
        [Summary("Deletes a user tag.")]
        [FullDescription("Delets a user tag that you own.")]
        [RateLimit(3, 2)]
        [RequireContext(ContextType.Guild)]
        public async Task<RuntimeResult> DeleteTagAsync([Summary("Self-explanatory.")] string Name)
        {
            using (BotDatabase botDatabase = new BotDatabase())
            {
                List<UserTag> tagList = await botDatabase.UserTags.ToListAsync();
                UserTag retrievedTag = null;

                if ((Context.User as SocketGuildUser).GuildPermissions.Has(GuildPermission.ManageMessages))
                    retrievedTag = tagList.SingleOrDefault(x => x.Name == Name && x.GuildId == Context.Guild.Id);
                else
                    retrievedTag = tagList.SingleOrDefault(x => x.Name == Name && x.AuthorId == Context.User.Id && x.GuildId == Context.Guild.Id);

                if (retrievedTag == null)
                    return ExecutionResult.FromError($"The tag **\"{Name}\"** does not exist, or you don't have permission to delete it.");

                botDatabase.UserTags.Remove(retrievedTag);
                await botDatabase.SaveChangesAsync();
            }

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync("Success!", allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("get")]
        [Alias("what")]
        [Summary("Gets a tag by its name, and replies.")]
        [FullDescription("Retrieve a tag by its name, and sends its content on the chat.")]
        [RateLimit(2, 1)]
        [RequireContext(ContextType.Guild)]
        public async Task<RuntimeResult> GetTagAsync([Summary("Self-explanatory.")] string Name)
        {
            using (BotDatabase botDatabase = new BotDatabase())
            {
                List<UserTag> tagList = await botDatabase.UserTags.ToListAsync();
                UserTag retrievedTag = tagList.SingleOrDefault(x => x.GuildId == Context.Guild.Id && x.Name == Name);

                if (retrievedTag == null)
                    return ExecutionResult.FromError($"The tag **\"{Name}\"** does not exist!");

                MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
                AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
                await ReplyAsync(retrievedTag.Reply, allowedMentions: allowedMentions, messageReference: messageReference);
            }

            return ExecutionResult.Succesful();
        }

        [Command("search")]
        [Alias("find", "similar")]
        [Summary("Searches for similar tags.")]
        [FullDescription("Searches for tags with a similar name.")]
        [RateLimit(2, 1)]
        [RequireContext(ContextType.Guild)]
        public async Task<RuntimeResult> SearchTagsAsync([Summary("The search term.")] string Name)
        {
            using (BotDatabase botDatabase = new BotDatabase())
            {
                List<UserTag> tagList = await botDatabase.UserTags.ToListAsync();
                List<UserTag> filteredTags = tagList.Where(x => x.GuildId == Context.Guild.Id &&
                            Name.DamerauLevenshteinDistance(x.Name, Settings.Instance.LoadedConfig.TagDistance) < int.MaxValue).Take(25).ToList();

                if (!filteredTags.Any())
                    return ExecutionResult.FromError($"No tags found with a name similar to \"{Name}\".");

                EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, Description: $"All of the tags similar to \"{Name}\".")
                    .ChangeTitle("TAG RESULTS");

                foreach (UserTag tag in filteredTags)
                {
                    RestUser globalAuthor = await Context.Client.Rest.GetUserAsync(tag.AuthorId);
                    string userName = globalAuthor != null ? globalAuthor.GetFullUsername() : "Unknown";

                    replyEmbed.AddField($"`{tag.Name}`", $"By: **{userName}**");
                }

                MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
                AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
                await ReplyAsync(null, false, replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);
            }

            return ExecutionResult.Succesful();
        }

        [Command("create")]
        [Alias("new")]
        [Summary("Creates a new tag.")]
        [FullDescription("Creates a new tag with the specified reply.")]
        [RateLimit(3, 1)]
        [RequireContext(ContextType.Guild)]
        public async Task<RuntimeResult> CreateTagAsync([Summary("Self-explanatory.")] string Name, 
                                                        [Summary("The text the bot will reply with when retrieving the tag.")] string Reply)
        {
            if (Name.Length > 15)
                return ExecutionResult.FromError("Please make the tag name shorter than 15 characters!");
            else if (Name.StartsWith(Settings.Instance.LoadedConfig.BotPrefix))
                return ExecutionResult.FromError("Tag names can't begin with my prefix!");
            else if (Context.Message.MentionedUsers.Count > 0)
                return ExecutionResult.FromError("Tag names or replies cannot contain mentions!");
            else if (Name.Contains(' '))
                return ExecutionResult.FromError("Tag names cannot contain spaces!");

            using (BotDatabase botDatabase = new BotDatabase())
            {
                List<UserTag> tagList = await botDatabase.UserTags.ToListAsync();
                tagList = tagList.Where(x => x.GuildId == Context.Guild.Id).ToList();

                if (tagList.Any(x => x.Name == Name))
                    return ExecutionResult.FromError($"There's already a tag called **\"{Name}\"**!");

                Guid newGuid = Guid.NewGuid();

                await botDatabase.UserTags.AddAsync(new UserTag
                {
                    Id = newGuid.ToString(),
                    Name = Name,
                    AuthorId = Context.Message.Author.Id,
                    GuildId = Context.Guild.Id,
                    Reply = Reply,
                    CreatedAt = Context.Message.Timestamp.ToUnixTimeSeconds()
                });
                await botDatabase.SaveChangesAsync();
            }

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync($"Tag created succesfully! Use `{Settings.Instance.LoadedConfig.BotPrefix}tags get {Name}` to use it!", allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }
    }
}
