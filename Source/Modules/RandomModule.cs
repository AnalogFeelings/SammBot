using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharpCat.Types.Cat;
using SharpCat.Types.Dog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SammBotNET.Modules
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

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, Description: ":warning: **The breed information has been " +
                "temporarily removed due to a bug in the Cat API.**").ChangeTitle("Random Cat");

            replyEmbed.ImageUrl = retrievedImages[0].Url;

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

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, Description: $"**__Breed__**: {retrievedImages[0].Breeds[0].Name}" +
                    $"\n**__Temperament__**: {retrievedImages[0].Breeds[0].Temperament}").ChangeTitle("Random Dog");

            replyEmbed.ImageUrl = retrievedImages[0].Url;

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

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle("Random Fox");
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

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle("Random Duck");
            replyEmbed.ImageUrl = repliedImage.ImageUrl;

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync(null, false, replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("scp")]
        [Summary("Returns a random SCP!")]
        [FullDescription("Returns a random SCP! The article may not exist.")]
        [RateLimit(3, 2)]
        public async Task<RuntimeResult> GetSCPAsync()
        {
            int maxNumber = 6999;
            int chosenNumber = Random.Shared.Next(maxNumber + 1);

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, Description: "https://scp-wiki.wikidot.com/scp-" + chosenNumber.ToString("D3"))
                                                        .ChangeTitle("Random SCP");

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync(null, false, replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }
    }
}
