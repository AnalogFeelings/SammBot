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

using Discord;
using Discord.Interactions;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using Microsoft.Extensions.DependencyInjection;
using SammBot.Bot.Services;
using SammBot.Library;
using SammBot.Library.Attributes;
using SammBot.Library.Extensions;
using SammBot.Library.Models;
using SammBot.Library.Models.EsixFurry;
using SammBot.Library.Models.Rule34;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SammBot.Library.Preconditions;

namespace SammBot.Bot.Modules;

[PrettyName("Booru")]
[Group("booru", "Commands to retrieve images from Booru-style sites.")]
[ModuleEmoji("\U0001f5bc\uFE0F")]
public class BooruModule : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly HttpService _httpService;
    private readonly InteractiveService _interactiveService;

    public BooruModule(IServiceProvider provider)
    {
        _httpService = provider.GetRequiredService<HttpService>();
        _interactiveService = provider.GetRequiredService<InteractiveService>();
    }

    [SlashCommand("r34", "Gets a list of images from rule34.")]
    [DetailedDescription("Gets a list of images from rule34. Maximum amount is 1000 images per command.")]
    [RateLimit(3, 2)]
    [RequireNsfw]
    public async Task<RuntimeResult> GetRule34Async
    (
        [Summary("Tags", "The tags you want to use for the search.")]
        string postTags
    )
    {
        if (string.IsNullOrWhiteSpace(postTags))
            return ExecutionResult.FromError("You must provide tags!");

        Rule34SearchParameters searchParameters = new Rule34SearchParameters()
        {
            Limit = 1000,
            Tags = postTags,
            UseJson = 1
        };

        await DeferAsync();

        List<Rule34Post>? nsfwPosts = await _httpService.GetObjectFromJsonAsync<List<Rule34Post>>("https://api.rule34.xxx/index.php?page=dapi&s=post&q=index", searchParameters);

        if (nsfwPosts == null || nsfwPosts.Count == 0)
            return ExecutionResult.FromError("Rule34 returned no posts! The API could be down for maintenance, or one of your tags is invalid.");

        List<Rule34Post> filteredPosts = nsfwPosts.Where(x => !x.FileUrl.EndsWith(".mp4") && !x.FileUrl.EndsWith(".webm")).ToList();

        if (filteredPosts.Count == 0)
            return ExecutionResult.FromError("All of the posts returned were videos! Please try another tag combination.");

        if (filteredPosts.Count == 1)
        {
            string embedDescription = $"\U0001f50d **Search Terms**: `{postTags.Truncate(1024)}`\n\n";
            embedDescription += $"\U0001f464 **Author**: `{filteredPosts[0].Owner}`\n";
            embedDescription += $"\U0001f44d **Score**: `{filteredPosts[0].Score}`\n";
            embedDescription += $"\U0001f51e **Rating**: `{filteredPosts[0].Rating.CapitalizeFirst()}`\n";
            embedDescription += $"\U0001f3f7\uFE0F **postTags**: `{filteredPosts[0].Tags.Truncate(512)}`\n";

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

            replyEmbed.Title = "\U0001f633 Rule34 Search Results";
            replyEmbed.Description = embedDescription;

            replyEmbed.WithColor(new Color(170, 229, 164));
            replyEmbed.WithUrl($"https://rule34.xxx/index.php?page=post&s=view&id={filteredPosts[0].Id}");
            replyEmbed.WithImageUrl(filteredPosts[0].FileUrl);

            await FollowupAsync(embed: replyEmbed.Build(), allowedMentions: Constants.AllowOnlyUsers);
        }
        else
        {
            LazyPaginator lazyPaginator = new LazyPaginatorBuilder().AddUser(Context.User)
                                                                    .WithPageFactory(GeneratePage)
                                                                    .WithMaxPageIndex(filteredPosts.Count - 1)
                                                                    .WithFooter(PaginatorFooter.None)
                                                                    .WithActionOnTimeout(ActionOnStop.DisableInput)
                                                                    .WithActionOnCancellation(ActionOnStop.DisableInput)
                                                                    .Build();

            await _interactiveService.SendPaginatorAsync(lazyPaginator, Context.Interaction, TimeSpan.FromMinutes(8), InteractionResponseType.DeferredChannelMessageWithSource);
        }

        return ExecutionResult.Succesful();

        PageBuilder GeneratePage(int index)
        {
            string embedDescription = $"\U0001f50d **Search Terms**: `{postTags.Truncate(1024)}`\n\n";
            embedDescription += $"\U0001f464 **Author**: `{filteredPosts[index].Owner}`\n";
            embedDescription += $"\U0001f44d **Score**: `{filteredPosts[index].Score}`\n";
            embedDescription += $"\U0001f51e **Rating**: `{filteredPosts[index].Rating.CapitalizeFirst()}`\n";
            embedDescription += $"\U0001f3f7\uFE0F **postTags**: `{filteredPosts[index].Tags.Truncate(512)}`\n";

            return new PageBuilder().WithTitle("\U0001f633 Rule34 Search Results")
                                    .WithDescription(embedDescription)
                                    .WithFooter($"Post {index + 1}/{filteredPosts.Count}")
                                    .WithCurrentTimestamp()
                                    .WithColor(new Color(170, 229, 164))
                                    .WithUrl($"https://rule34.xxx/index.php?page=post&s=view&id={filteredPosts[index].Id}")
                                    .WithImageUrl(filteredPosts[index].FileUrl);
        }
    }

    [SlashCommand("e621", "Gets a list of images from e621.")]
    [DetailedDescription("Gets a list of images from e621. Maximum amount is 320 images per command.")]
    [RateLimit(2, 1)]
    [RequireNsfw]
    public async Task<RuntimeResult> GetE621Async
    (
        [Summary("Tags", "The tags you want to use for the search.")]
        string postTags
    )
    {
        if (string.IsNullOrWhiteSpace(postTags))
            return ExecutionResult.FromError("You must provide tags!");

        EsixFurrySearchParameters searchParameters = new EsixFurrySearchParameters()
        {
            Limit = 320,
            Tags = postTags
        };

        await DeferAsync();

        EsixFurryReply? nsfwPosts = await _httpService.GetObjectFromJsonAsync<EsixFurryReply>("https://e621.net/posts.json", searchParameters);

        if (nsfwPosts == null || nsfwPosts.Posts.Count == 0)
            return ExecutionResult.FromError("e621 returned no posts! The API could be down for maintenance, or one of your tags is invalid.");

        List<EsixFurryPost> filteredPosts = nsfwPosts.Posts.Where(x => x.File.Extension != "mp4" && x.File.Extension != "webm").ToList();

        if (filteredPosts.Count == 0)
            return ExecutionResult.FromError("All of the posts returned were videos! Please try another tag combination.");

        if (filteredPosts.Count == 1)
        {
            EsixFurryPost post = filteredPosts[0];

            string artist = post.Tags.Artist.IsNullOrEmpty() ? "Unknown" : string.Join(", ", post.Tags.Artist);
            string tags = post.Tags.General.IsNullOrEmpty() ? "Unknown" : string.Join(", ", post.Tags.General);
            string species = post.Tags.Species.IsNullOrEmpty() ? "Unknown" : string.Join(", ", post.Tags.Species);
            string character = post.Tags.Character.IsNullOrEmpty() ? "Unknown" : string.Join(", ", post.Tags.Character);

            string embedDescription = $"\U0001f9d1\u200D\U0001f3a8 **Artist**: {artist}\n";
            embedDescription += $"\U0001f4d6 **Characters**: `{character.Truncate(512)}`\n";
            embedDescription += $"\U0001f9ec **Species**: `{species.Truncate(512)}`\n";
            embedDescription += $"\U0001f3f7\uFE0F **Post Tags**: `{tags.Truncate(512)}`\n";
            embedDescription += $"\U0001f44d **Likes**: `{post.Score.Upvotes}`\n";
            embedDescription += $"\U0001f44e **Dislikes**: `{Math.Abs(post.Score.Downvotes)}`\n";

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

            replyEmbed.Title = "\U0001f98a e621 Search Results";
            replyEmbed.Description = embedDescription;

            replyEmbed.WithColor(new Color(0, 73, 150));
            replyEmbed.WithUrl($"https://e621.net/posts/{post.Id}");
            replyEmbed.WithImageUrl(post.File.FileUrl);

            await FollowupAsync(embed: replyEmbed.Build(), allowedMentions: Constants.AllowOnlyUsers);
        }
        else
        {
            LazyPaginator lazyPaginator = new LazyPaginatorBuilder().AddUser(Context.User)
                                                                    .WithPageFactory(GeneratePage)
                                                                    .WithMaxPageIndex(filteredPosts.Count - 1)
                                                                    .WithFooter(PaginatorFooter.None)
                                                                    .WithActionOnTimeout(ActionOnStop.DeleteInput)
                                                                    .WithActionOnCancellation(ActionOnStop.DeleteInput)
                                                                    .Build();

            await _interactiveService.SendPaginatorAsync(lazyPaginator, Context.Interaction, TimeSpan.FromMinutes(8), InteractionResponseType.DeferredChannelMessageWithSource);
        }

        return ExecutionResult.Succesful();

        PageBuilder GeneratePage(int index)
        {
            EsixFurryPost post = filteredPosts[index];

            string artist = post.Tags.Artist.IsNullOrEmpty() ? "Unknown" : string.Join(", ", post.Tags.Artist);
            string tags = post.Tags.General.IsNullOrEmpty() ? "Unknown" : string.Join(", ", post.Tags.General);
            string species = post.Tags.Species.IsNullOrEmpty() ? "Unknown" : string.Join(", ", post.Tags.Species);
            string character = post.Tags.Character.IsNullOrEmpty() ? "Unknown" : string.Join(", ", post.Tags.Character);

            string embedDescription = $"\U0001f9d1\u200D\U0001f3a8 **Artist**: {artist}\n";
            embedDescription += $"\U0001f4d6 **Characters**: `{character.Truncate(512)}`\n";
            embedDescription += $"\U0001f9ec **Species**: `{species.Truncate(512)}`\n";
            embedDescription += $"\U0001f3f7\uFE0F **Post Tags**: `{tags.Truncate(512)}`\n";
            embedDescription += $"\U0001f44d **Likes**: `{post.Score.Upvotes}`\n";
            embedDescription += $"\U0001f44e **Dislikes**: `{Math.Abs(post.Score.Downvotes)}`\n";

            return new PageBuilder().WithTitle("\U0001f98a e621 Search Results")
                                    .WithDescription(embedDescription)
                                    .WithFooter($"Post {index + 1}/{filteredPosts.Count}")
                                    .WithCurrentTimestamp()
                                    .WithColor(new Color(0, 73, 150))
                                    .WithUrl($"https://e621.net/posts/{post.Id}")
                                    .WithImageUrl(post.File.FileUrl);
        }
    }
}