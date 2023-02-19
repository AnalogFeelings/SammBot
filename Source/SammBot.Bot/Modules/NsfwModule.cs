#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021-2023 AestheticalZ
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using Newtonsoft.Json;
using SammBot.Bot.Common;
using SammBot.Bot.Common.Attributes;
using SammBot.Bot.Common.Preconditions;
using SammBot.Bot.Common.Rest.Rule34;
using SammBot.Bot.Core;
using SammBot.Bot.Extensions;
using SammBot.Bot.Services;

namespace SammBot.Bot.Modules;

[PrettyName("NSFW")]
[Group("nsfw", "NSFW commands such as rule34 search, and more. Requires NSFW channel.")]
[ModuleEmoji("\U0001f51e")]
public class NsfwModule : InteractionModuleBase<ShardedInteractionContext>
{
    public HttpService HttpService { get; set; }
    public InteractiveService InteractiveService { get; set; }
    
    [SlashCommand("r34", "Gets a list of images from rule34.")]
    [DetailedDescription("Gets a list of images from rule34. Maximum amount is 1000 images per command.")]
    [RateLimit(3, 2)]
    [RequireContext(ContextType.Guild)]
    [RequireNsfw]
    public async Task<RuntimeResult> GetRule34Async([Summary(description: "The tags you want to use for the search.")] string Tags)
    {
        if(string.IsNullOrWhiteSpace(Tags))
            return ExecutionResult.FromError("You must provide tags!");
        
        Rule34SearchParameters searchParameters = new Rule34SearchParameters()
        {
            Limit = 1000,
            Tags = Tags,
            UseJson = 1
        };

        await DeferAsync();

        List<Rule34Post>? nsfwPosts = await HttpService.GetObjectFromJsonAsync<List<Rule34Post>>("https://api.rule34.xxx/index.php?page=dapi&s=post&q=index", searchParameters);
        
        if(nsfwPosts == null || nsfwPosts.Count == 0)
            return ExecutionResult.FromError("Rule34 returned no posts! The API could be down for maintenance, or one of your tags is invalid.");

        List<Rule34Post> filteredPosts = nsfwPosts.Where(x => !x.FileUrl.EndsWith(".mp4") && !x.FileUrl.EndsWith(".webm")).ToList();

        if (filteredPosts.Count == 0)
            return ExecutionResult.FromError("All of the posts returned were videos! Please try another tag combination.");

        if (filteredPosts.Count == 1)
        {
            string embedDescription = $"\U0001f50d **Search Terms**: `{Tags.Truncate(1024)}`\n\n";
            embedDescription += $"\U0001f464 **Author**: `{filteredPosts[0].Owner}`\n";
            embedDescription += $"\U0001f44d **Score**: `{filteredPosts[0].Score}`\n";
            embedDescription += $"\U0001f51e **Rating**: `{filteredPosts[0].Rating.CapitalizeFirst()}`\n";
            embedDescription += $"\U0001f3f7\uFE0F **Tags**: `{filteredPosts[0].Tags.Truncate(512)}`\n";

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, Description: embedDescription);
            replyEmbed.Title = "\U0001f633 Rule34 Search Results";
            replyEmbed.WithColor(new Color(170, 229, 164));
            replyEmbed.WithUrl($"https://rule34.xxx/index.php?page=post&s=view&id={filteredPosts[0].Id}");
            replyEmbed.WithImageUrl(filteredPosts[0].FileUrl);

            await FollowupAsync(embed: replyEmbed.Build(), allowedMentions: BotGlobals.Instance.AllowOnlyUsers);
        }
        else
        {
            LazyPaginator lazyPaginator = new LazyPaginatorBuilder()
                .AddUser(Context.User)
                .WithPageFactory(GeneratePage)
                .WithMaxPageIndex(filteredPosts.Count - 1)
                .WithFooter(PaginatorFooter.None)
                .WithActionOnTimeout(ActionOnStop.DisableInput)
                .WithActionOnCancellation(ActionOnStop.DisableInput)
                .Build();

            await InteractiveService.SendPaginatorAsync(lazyPaginator, Context.Interaction, TimeSpan.FromMinutes(8), InteractionResponseType.DeferredChannelMessageWithSource);
        }

        return ExecutionResult.Succesful();

        PageBuilder GeneratePage(int Index)
        {
            string embedDescription = $"\U0001f50d **Search Terms**: `{Tags.Truncate(1024)}`\n\n";
            embedDescription += $"\U0001f464 **Author**: `{filteredPosts[Index].Owner}`\n";
            embedDescription += $"\U0001f44d **Score**: `{filteredPosts[Index].Score}`\n";
            embedDescription += $"\U0001f51e **Rating**: `{filteredPosts[Index].Rating.CapitalizeFirst()}`\n";
            embedDescription += $"\U0001f3f7\uFE0F **Tags**: `{filteredPosts[Index].Tags.Truncate(512)}`\n";

            return new PageBuilder()
                .WithTitle("\U0001f633 Rule34 Search Results")
                .WithDescription(embedDescription)
                .WithFooter($"Post {Index + 1}/{filteredPosts.Count}")
                .WithCurrentTimestamp()
                .WithColor(new Color(170, 229, 164))
                .WithUrl($"https://rule34.xxx/index.php?page=post&s=view&id={filteredPosts[Index].Id}")
                .WithImageUrl(filteredPosts[Index].FileUrl);
        }
    }
}