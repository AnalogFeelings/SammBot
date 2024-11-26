#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021 Analog Feelings
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
using Microsoft.Extensions.DependencyInjection;
using SammBot.Library;
using SammBot.Library.Attributes;
using SammBot.Library.Extensions;
using SammBot.Library.Models;
using SammBot.Library.Models.Animal;
using SammBot.Library.Preconditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SammBot.Services;

namespace SammBot.Modules;

[PrettyName("Random")]
[Group("random", "Random crazyness!")]
[ModuleEmoji("\U0001f3b0")]
public class RandomModule : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly HttpService _httpService;

    public RandomModule(IServiceProvider provider)
    {
        _httpService = provider.GetRequiredService<HttpService>();
    }

    [SlashCommand("cat", "Returns a random cat!")]
    [DetailedDescription("Gets a random cat image from The Cat API!")]
    [RateLimit(3, 2)]
    public async Task<RuntimeResult> GetCatAsync()
    {
        await DeferAsync();

        List<CatImage>? retrievedImages = await _httpService.GetObjectFromJsonAsync<List<CatImage>>("https://api.thecatapi.com/v1/images/search");
        
        if (retrievedImages == null || retrievedImages.Count == 0)
            return ExecutionResult.FromError("Could not retrieve a cat image! The service may be unavailable.");
        
        CatImage retrievedImage = retrievedImages.First();
        EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

        replyEmbed.Title = "\U0001f431 Random Cat";
        replyEmbed.Color = new Color(255, 204, 77);
        replyEmbed.ImageUrl = retrievedImage.Url;

        await FollowupAsync(embed: replyEmbed.Build(), allowedMentions: Constants.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    [SlashCommand("dog", "Returns a random dog!")]
    [DetailedDescription("Gets a random dog image from The Dog API!")]
    [RateLimit(3, 2)]
    public async Task<RuntimeResult> GetDogAsync()
    {
        await DeferAsync();

        List<DogImage>? retrievedImages = await _httpService.GetObjectFromJsonAsync<List<DogImage>>("https://api.thedogapi.com/v1/images/search");
        
        if (retrievedImages == null || retrievedImages.Count == 0)
            return ExecutionResult.FromError("Could not retrieve a dog image! The service may be unavailable.");

        DogImage retrievedImage = retrievedImages.First();
        EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

        replyEmbed.Title = "\U0001f436 Random Dog";
        replyEmbed.Color = new Color(217, 158, 130);
        replyEmbed.ImageUrl = retrievedImage.Url;

        await FollowupAsync(embed: replyEmbed.Build(), allowedMentions: Constants.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    [SlashCommand("fox", "Returns a random fox!")]
    [DetailedDescription("Gets a random fox image from the RandomFox API!")]
    [RateLimit(3, 2)]
    public async Task<RuntimeResult> GetFoxAsync()
    {
        await DeferAsync();

        FoxImage? repliedImage = await _httpService.GetObjectFromJsonAsync<FoxImage>("https://randomfox.ca/floof/");

        if (repliedImage == null)
            return ExecutionResult.FromError("Could not retrieve a fox image! The service may be unavailable.");

        EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

        replyEmbed.Title = "\U0001f98a Random Fox";
        replyEmbed.Color = new Color(241, 143, 38);
        replyEmbed.ImageUrl = repliedImage.ImageUrl;

        await FollowupAsync(embed: replyEmbed.Build(), allowedMentions: Constants.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    [SlashCommand("duck", "Returns a random duck!")]
    [DetailedDescription("Gets a random duck image from the RandomDuk API!")]
    [RateLimit(3, 2)]
    public async Task<RuntimeResult> GetDuckAsync()
    {
        await DeferAsync();

        DuckImage? repliedImage = await _httpService.GetObjectFromJsonAsync<DuckImage>("https://random-d.uk/api/v2/random");

        if (repliedImage == null)
            return ExecutionResult.FromError("Could not retrieve a duck image! The service may be unavailable.");

        EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

        replyEmbed.Title = "\U0001f986 Random Duck";
        replyEmbed.Color = new Color(62, 114, 29);
        replyEmbed.ImageUrl = repliedImage.ImageUrl;

        await FollowupAsync(embed: replyEmbed.Build(), allowedMentions: Constants.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }
}