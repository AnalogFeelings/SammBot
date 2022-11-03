using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SammBot.Bot.Classes;
using SammBot.Bot.RestDefinitions;
using SammBot.Bot.Services;
using SharpCat.Types.Cat;
using SharpCat.Types.Dog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SammBot.Bot.Modules
{
    [Name("Random")]
    [Group("random")]
    [Summary("Random crazyness!")]
    [ModuleEmoji("\U0001f3b0")]
    public class RandomModule : ModuleBase<SocketCommandContext>
    {
        public RandomService RandomService { get; set; }

        [Command("cat")]
        [Alias("kit", "kitto", "cogga")]
        [Summary("Returns a random cat!")]
        [FullDescription("Gets a random cat image from The Cat API!")]
        [RateLimit(3, 2)]
        public async Task<RuntimeResult> GetCatAsync()
        {
            CatImageSearchParams searchParameters = new CatImageSearchParams()
            {
                has_breeds = true,
                mime_types = "jpg,png,gif",
                size = "small",
                limit = 1
            };

            List<CatImage> retrievedImages = await RandomService.CatRequester.GetImageAsync(searchParameters);
            
            CatImage retrievedImage = retrievedImages.First();
            CatBreed retrievedBreed = retrievedImage.Breeds?.FirstOrDefault();

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

            replyEmbed.Title = "\U0001f431 Random Cat";
            replyEmbed.Description = retrievedBreed != default(CatBreed) ?
                $"\U0001f43e **Breed**: {retrievedBreed.Name}\n" +
                $"\u2764\uFE0F **Temperament**: {retrievedBreed.Temperament}" 
                : string.Empty;
            
            replyEmbed.Color = new Color(255, 204, 77);
            replyEmbed.ImageUrl = retrievedImage.Url;

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync(null, false, replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("dog")]
        [Alias("doggo", "dogger")]
        [Summary("Returns a random dog!")]
        [FullDescription("Gets a random dog image from The Dog API!")]
        [RateLimit(3, 2)]
        public async Task<RuntimeResult> GetDogAsync()
        {
            DogImageSearchParams searchParameters = new DogImageSearchParams()
            {
                has_breeds = true,
                mime_types = "jpg,png,gif",
                size = "small",
                sub_id = Context.Message.Author.Username,
                limit = 1
            };

            List<DogImage> retrievedImages = await RandomService.DogRequester.GetImageAsync(searchParameters);

            DogImage retrievedImage = retrievedImages.First();
            DogBreed retrievedBreed = retrievedImage.Breeds?.FirstOrDefault();

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

            replyEmbed.Title = "\U0001f436 Random Dog";
            replyEmbed.Description = retrievedBreed != default(DogBreed) ?
                $"\U0001f43e **Breed**: {retrievedBreed.Name}\n" +
                $"\u2764\uFE0F **Temperament**: {retrievedBreed.Temperament}" 
                : string.Empty;
            
            replyEmbed.Color = new Color(217, 158, 130);
            replyEmbed.ImageUrl = retrievedImage.Url;

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync(null, false, replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("fox")]
        [Summary("Returns a random fox!")]
        [FullDescription("Gets a random fox image from the RandomFox API!")]
        [RateLimit(3, 2)]
        public async Task<RuntimeResult> GetFoxAsync()
        {
            string jsonReply = string.Empty;

            using (HttpResponseMessage responseMessage = await RandomService.RandomClient.GetAsync("https://randomfox.ca/floof/"))
            {
                jsonReply = await responseMessage.Content.ReadAsStringAsync();
            }

            FoxImage repliedImage = JsonConvert.DeserializeObject<FoxImage>(jsonReply);

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);
            
            replyEmbed.Title = "\U0001f98a Random Fox";
            replyEmbed.Color = new Color(241, 143, 38);
            replyEmbed.ImageUrl = repliedImage.ImageUrl;

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync(null, false, replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("duck")]
        [Summary("Returns a random duck!")]
        [FullDescription("Gets a random duck image from the RandomDuk API!")]
        [RateLimit(3, 2)]
        public async Task<RuntimeResult> GetDuckAsync()
        {
            string jsonReply = string.Empty;

            using (HttpResponseMessage responseMessage = await RandomService.RandomClient.GetAsync("https://random-d.uk/api/v2/random"))
            {
                jsonReply = await responseMessage.Content.ReadAsStringAsync();
            }

            DuckImage repliedImage = JsonConvert.DeserializeObject<DuckImage>(jsonReply);

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);
            
            replyEmbed.Title = "\U0001f986 Random Duck";
            replyEmbed.Color = new Color(62, 114, 29);
            replyEmbed.ImageUrl = repliedImage.ImageUrl;

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync(null, false, replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }
    }
}
