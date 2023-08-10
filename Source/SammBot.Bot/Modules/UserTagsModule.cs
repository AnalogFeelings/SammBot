#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2023 Analog Feelings
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
#endregion

using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions;
using SammBot.Bot.Core;
using SammBot.Bot.Database;
using SammBot.Library;
using SammBot.Library.Attributes;
using SammBot.Library.Database.Models;
using SammBot.Library.Extensions;
using SammBot.Library.Preconditions;

namespace SammBot.Bot.Modules;

[PrettyName("User Tags")]
[Group("tags", "Tags that reply with a message when searched.")]
[ModuleEmoji("\U0001f3f7\uFE0F")]
public class UserTagsModule : InteractionModuleBase<ShardedInteractionContext>
{
    [SlashCommand("delete", "Deletes a user tag.")]
    [DetailedDescription("Delets a user tag that you own. If you have permission to manage messages in the server, you can delete any tag without owning it.")]
    [RateLimit(3, 2)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> DeleteTagAsync([Summary(description: "Self-explanatory.")] string Name)
    {
        await DeferAsync(true);
            
        using (BotDatabase botDatabase = new BotDatabase())
        {
            UserTag? retrievedTag;

            if ((Context.User as SocketGuildUser)!.GuildPermissions.Has(GuildPermission.ManageMessages))
                retrievedTag = await botDatabase.UserTags.SingleOrDefaultAsync(x => x.Name == Name && x.GuildId == Context.Guild.Id);
            else
                retrievedTag = await botDatabase.UserTags.SingleOrDefaultAsync(x => x.Name == Name &&
                                                                                    x.AuthorId == Context.User.Id &&
                                                                                    x.GuildId == Context.Guild.Id);

            if (retrievedTag == default)
                return ExecutionResult.FromError($"The tag **\"{Name}\"** does not exist, or you don't have permission to delete it.");

            botDatabase.UserTags.Remove(retrievedTag);
            
            await botDatabase.SaveChangesAsync();
        }

        await FollowupAsync("Success!", allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    [SlashCommand("get", "Gets a tag by its name, and replies.")]
    [DetailedDescription("Retrieve a tag by its name, and sends its content on the chat.")]
    [RateLimit(2, 1)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> GetTagAsync([Summary(description: "Self-explanatory.")] string Name)
    {
        await DeferAsync();
            
        using (BotDatabase botDatabase = new BotDatabase())
        {
            UserTag? retrievedTag = await botDatabase.UserTags.SingleOrDefaultAsync(x => x.GuildId == Context.Guild.Id && x.Name == Name);

            if (retrievedTag == default)
                return ExecutionResult.FromError($"The tag **\"{Name}\"** does not exist!");

            await FollowupAsync(retrievedTag.Reply, allowedMentions: BotGlobals.Instance.AllowOnlyUsers);
        }

        return ExecutionResult.Succesful();
    }

    [SlashCommand("search", "Searches for similar tags.")]
    [DetailedDescription("Searches for tags with a similar name.")]
    [RateLimit(2, 1)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> SearchTagsAsync([Summary(description: "The search term.")] string Name)
    {
        await DeferAsync();
            
        using (BotDatabase botDatabase = new BotDatabase())
        {
            List<UserTag> filteredTags = botDatabase.UserTags.Where(x => x.GuildId == Context.Guild.Id && 
                                                                         Name.DamerauDistance(x.Name, SettingsManager.Instance.LoadedConfig.TagDistance) < int.MaxValue).Take(25).ToList();

            if (!filteredTags.Any())
                return ExecutionResult.FromError($"No tags found with a name similar to \"{Name}\".");

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

            replyEmbed.Title = "\U0001f50d Search Results";
            replyEmbed.Description = $"All of the tags similar to \"{Name}\".";
            replyEmbed.Color = new Color(187, 221, 245);

            foreach (UserTag tag in filteredTags)
            {
                RestUser globalAuthor = await Context.Client.Rest.GetUserAsync(tag.AuthorId);
                string userName = globalAuthor != null ? globalAuthor.GetFullUsername() : "Unknown";

                replyEmbed.AddField($"`{tag.Name}`", $"By: **{userName}**");
            }

            await FollowupAsync(embed: replyEmbed.Build(), allowedMentions: BotGlobals.Instance.AllowOnlyUsers);
        }

        return ExecutionResult.Succesful();
    }

    [SlashCommand("create", "Creates a new tag.")]
    [DetailedDescription("Creates a new tag with the specified reply.")]
    [RateLimit(3, 1)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> CreateTagAsync([Summary(description: "Self-explanatory.")] string Name, 
        [Summary(description: "The text the bot will reply with when retrieving the tag.")] string Reply)
    {
        if (Name.Length >= 15)
            return ExecutionResult.FromError("Please make the tag name shorter than 15 characters!");
        if (Reply.Length >= 128)
            return ExecutionResult.FromError("Please make the tag reply shorter than 128 characters!");
        if (Name.Contains(' '))
            return ExecutionResult.FromError("Tag names cannot contain spaces!");

        await DeferAsync();

        using (BotDatabase botDatabase = new BotDatabase())
        {
            List<UserTag> tagList = botDatabase.UserTags.Where(x => x.GuildId == Context.Guild.Id).ToList();

            if (tagList.Any(x => x.Name == Name))
                return ExecutionResult.FromError($"There's already a tag called **\"{Name}\"**!");

            UserTag newTag = new UserTag
            {
                Name = Name,
                AuthorId = Context.Interaction.User.Id,
                GuildId = Context.Guild.Id,
                Reply = Reply,
                CreatedAt = Context.Interaction.CreatedAt.ToUnixTimeSeconds()
            };

            await botDatabase.UserTags.AddAsync(newTag);
                
            await botDatabase.SaveChangesAsync();
        }

        await FollowupAsync($"Tag created succesfully! Use `/tags get {Name}` to use it!", allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }
}