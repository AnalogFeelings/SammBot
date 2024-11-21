#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021-2024 Analog Feelings
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using SammBot.Library;
using SammBot.Library.Attributes;
using SammBot.Library.Extensions;
using SammBot.Library.Models;
using SammBot.Library.Models.Database;
using SammBot.Library.Preconditions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SammBot.Services;

namespace SammBot.Modules;

[PrettyName("User Tags")]
[Group("tags", "Tags that reply with a message when searched.")]
[ModuleEmoji("\U0001f3f7\uFE0F")]
public class UserTagsModule : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly SettingsService _settingsService;

    public UserTagsModule(IServiceProvider provider)
    {
        _settingsService = provider.GetRequiredService<SettingsService>();
    }

    [SlashCommand("delete", "Deletes a user tag.")]
    [DetailedDescription("Delets a user tag that you own. If you have permission to manage messages in the server, you can delete any tag without owning it.")]
    [RateLimit(3, 2)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> DeleteTagAsync
    (
        [Summary("Name", "Self-explanatory.")] string tagName
    )
    {
        await DeferAsync(true);

        using (DatabaseService databaseService = new DatabaseService())
        {
            UserTag? retrievedTag;

            if ((Context.User as SocketGuildUser)!.GuildPermissions.Has(GuildPermission.ManageMessages))
            {
                retrievedTag = await databaseService.UserTags.SingleOrDefaultAsync(x => x.Name == tagName && x.GuildId == Context.Guild.Id);
            }
            else
            {
                retrievedTag = await databaseService.UserTags.SingleOrDefaultAsync(x => x.Name == tagName &&
                                                                                    x.AuthorId == Context.User.Id &&
                                                                                    x.GuildId == Context.Guild.Id);
            }

            if (retrievedTag == default)
                return ExecutionResult.FromError($"The tag **\"{tagName}\"** does not exist, or you don't have permission to delete it.");

            databaseService.UserTags.Remove(retrievedTag);

            await databaseService.SaveChangesAsync();
        }

        await FollowupAsync("Success!", allowedMentions: Constants.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    [SlashCommand("get", "Gets a tag by its name, and replies.")]
    [DetailedDescription("Retrieve a tag by its name, and sends its content on the chat.")]
    [RateLimit(2, 1)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> GetTagAsync
    (
        [Summary("Name", "Self-explanatory.")] string tagName
    )
    {
        await DeferAsync();

        using (DatabaseService databaseService = new DatabaseService())
        {
            UserTag? retrievedTag = await databaseService.UserTags.SingleOrDefaultAsync(x => x.GuildId == Context.Guild.Id && x.Name == tagName);

            if (retrievedTag == default)
                return ExecutionResult.FromError($"The tag **\"{tagName}\"** does not exist!");

            string builtMessage = $"\u2611\uFE0F Here is the tag named `{tagName}`:\n" +
                                  retrievedTag.Reply;

            await FollowupAsync(builtMessage, allowedMentions: Constants.AllowOnlyUsers);
        }

        return ExecutionResult.Succesful();
    }

    [SlashCommand("search", "Searches for similar tags.")]
    [DetailedDescription("Searches for tags with a similar name.")]
    [RateLimit(2, 1)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> SearchTagsAsync
    (
        [Summary("Term", "The search term.")] string searchTerm
    )
    {
        await DeferAsync();

        using (DatabaseService databaseService = new DatabaseService())
        {
            List<UserTag> allTags = await databaseService.UserTags.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
            List<UserTag> filteredTags = allTags.Where(x => searchTerm.DamerauDistance(x.Name, _settingsService.Settings.TagDistance) < int.MaxValue).Take(25).ToList();

            if (!filteredTags.Any())
                return ExecutionResult.FromError($"No tags found with a name similar to \"{searchTerm}\".");

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

            replyEmbed.Title = "\U0001f50d Search Results";
            replyEmbed.Description = $"All of the tags similar to \"{searchTerm}\".";
            replyEmbed.Color = new Color(187, 221, 245);

            foreach (UserTag tag in filteredTags)
            {
                RestUser globalAuthor = await Context.Client.Rest.GetUserAsync(tag.AuthorId);
                string userName = globalAuthor != null ? globalAuthor.GetFullUsername() : "Unknown";

                replyEmbed.AddField($"`{tag.Name}`", $"By: **{userName}**");
            }

            await FollowupAsync(embed: replyEmbed.Build(), allowedMentions: Constants.AllowOnlyUsers);
        }

        return ExecutionResult.Succesful();
    }

    [SlashCommand("create", "Creates a new tag.")]
    [DetailedDescription("Creates a new tag with the specified reply.")]
    [RateLimit(3, 1)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> CreateTagAsync
    (
        [Summary("Name", "Self-explanatory.")] string tagName,
        [Summary("Reply", "The text the bot will reply with when retrieving the tag.")]
        string tagReply
    )
    {
        if (tagName.Length >= 15)
            return ExecutionResult.FromError("Please make the tag name shorter than 15 characters!");
        if (tagReply.Length >= 128)
            return ExecutionResult.FromError("Please make the tag reply shorter than 128 characters!");
        if (tagName.Contains(' '))
            return ExecutionResult.FromError("Tag names cannot contain spaces!");

        await DeferAsync();

        using (DatabaseService databaseService = new DatabaseService())
        {
            List<UserTag> tagList = databaseService.UserTags.Where(x => x.GuildId == Context.Guild.Id).ToList();

            if (tagList.Any(x => x.Name == tagName))
                return ExecutionResult.FromError($"There's already a tag called **\"{tagName}\"**!");

            UserTag newTag = new UserTag
            {
                Name = tagName,
                AuthorId = Context.Interaction.User.Id,
                GuildId = Context.Guild.Id,
                Reply = tagReply,
                CreatedAt = Context.Interaction.CreatedAt.ToUnixTimeSeconds()
            };

            await databaseService.UserTags.AddAsync(newTag);

            await databaseService.SaveChangesAsync();
        }

        await FollowupAsync($"Tag created succesfully! Use `/tags get {tagName}` to use it!", allowedMentions: Constants.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }
}