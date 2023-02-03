#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2023 AestheticalZ
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
using Newtonsoft.Json;
using SharpCat.Types.Cat;
using SharpCat.Types.Dog;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Interactions;
using SammBot.Bot.Attributes;
using SammBot.Bot.Core;
using SammBot.Bot.Extensions;
using SammBot.Bot.Preconditions;
using SammBot.Bot.Rest;
using SammBot.Bot.Services;

namespace SammBot.Bot.Modules;

[PrettyName("Random")]
[Group("random", "Random crazyness!")]
[ModuleEmoji("\U0001f3b0")]
public class RandomModule : InteractionModuleBase<ShardedInteractionContext>
{
    public RandomService RandomService { get; set; }

    [SlashCommand("cat", "Returns a random cat!")]
    [DetailedDescription("Gets a random cat image from The Cat API!")]
    [RateLimit(3, 2)]
    public async Task<RuntimeResult> GetCatAsync()
    {
        await DeferAsync();
            
        CatImageSearchParams searchParameters = new CatImageSearchParams()
        {
            has_breeds = true,
            mime_types = "jpg,png,gif",
            size = "small",
            limit = 1
        };

        List<CatImage> retrievedImages = await RandomService.CatRequester.GetImageAsync(searchParameters);
            
        CatImage retrievedImage = retrievedImages.First();
        CatBreed? retrievedBreed = retrievedImage.Breeds?.FirstOrDefault();

        EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

        replyEmbed.Title = "\U0001f431 Random Cat";
        replyEmbed.Description = retrievedBreed != default(CatBreed) ?
            $"\U0001f43e **Breed**: {retrievedBreed.Name}\n" +
            $"\u2764\uFE0F **Temperament**: {retrievedBreed.Temperament}" 
            : string.Empty;
            
        replyEmbed.Color = new Color(255, 204, 77);
        replyEmbed.ImageUrl = retrievedImage.Url;

        await FollowupAsync(embed: replyEmbed.Build(), allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    [SlashCommand("dog", "Returns a random dog!")]
    [DetailedDescription("Gets a random dog image from The Dog API!")]
    [RateLimit(3, 2)]
    public async Task<RuntimeResult> GetDogAsync()
    {
        await DeferAsync();
            
        DogImageSearchParams searchParameters = new DogImageSearchParams()
        {
            has_breeds = true,
            mime_types = "jpg,png,gif",
            size = "small",
            limit = 1
        };

        List<DogImage> retrievedImages = await RandomService.DogRequester.GetImageAsync(searchParameters);

        DogImage retrievedImage = retrievedImages.First();
        DogBreed? retrievedBreed = retrievedImage.Breeds?.FirstOrDefault();

        EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

        replyEmbed.Title = "\U0001f436 Random Dog";
        replyEmbed.Description = retrievedBreed != default(DogBreed) ?
            $"\U0001f43e **Breed**: {retrievedBreed.Name}\n" +
            $"\u2764\uFE0F **Temperament**: {retrievedBreed.Temperament}" 
            : string.Empty;
            
        replyEmbed.Color = new Color(217, 158, 130);
        replyEmbed.ImageUrl = retrievedImage.Url;

        await FollowupAsync(embed: replyEmbed.Build(), allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    [SlashCommand("fox", "Returns a random fox!")]
    [DetailedDescription("Gets a random fox image from the RandomFox API!")]
    [RateLimit(3, 2)]
    public async Task<RuntimeResult> GetFoxAsync()
    {
        await DeferAsync();
            
        string jsonReply;
        using (HttpResponseMessage responseMessage = await RandomService.RandomClient.GetAsync("https://randomfox.ca/floof/"))
        {
            jsonReply = await responseMessage.Content.ReadAsStringAsync();
        }

        FoxImage? repliedImage = JsonConvert.DeserializeObject<FoxImage>(jsonReply);

        if (repliedImage == null)
            return ExecutionResult.FromError("Could not retrieve a fox image! The service may be unavailable.");

        EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);
            
        replyEmbed.Title = "\U0001f98a Random Fox";
        replyEmbed.Color = new Color(241, 143, 38);
        replyEmbed.ImageUrl = repliedImage.ImageUrl;

        await FollowupAsync(embed: replyEmbed.Build(), allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    [SlashCommand("duck", "Returns a random duck!")]
    [DetailedDescription("Gets a random duck image from the RandomDuk API!")]
    [RateLimit(3, 2)]
    public async Task<RuntimeResult> GetDuckAsync()
    {
        await DeferAsync();
            
        string jsonReply;
        using (HttpResponseMessage responseMessage = await RandomService.RandomClient.GetAsync("https://random-d.uk/api/v2/random"))
        {
            jsonReply = await responseMessage.Content.ReadAsStringAsync();
        }

        DuckImage? repliedImage = JsonConvert.DeserializeObject<DuckImage>(jsonReply);
        
        if (repliedImage == null)
            return ExecutionResult.FromError("Could not retrieve a duck image! The service may be unavailable.");

        EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);
            
        replyEmbed.Title = "\U0001f986 Random Duck";
        replyEmbed.Color = new Color(62, 114, 29);
        replyEmbed.ImageUrl = repliedImage.ImageUrl;

        await FollowupAsync(embed: replyEmbed.Build(), allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }
}