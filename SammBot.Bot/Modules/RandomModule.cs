using Discord;
using Newtonsoft.Json;
using SammBot.Bot.Classes;
using SharpCat.Types.Cat;
using SharpCat.Types.Dog;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Interactions;

namespace SammBot.Bot.Modules
{
    [FullName("Random")]
    [Group("random", "Random crazyness!")]
    [ModuleEmoji("\U0001f3b0")]
    public class RandomModule : InteractionModuleBase<ShardedInteractionContext>
    {
        public RandomService RandomService { get; set; }

        [SlashCommand("cat", "Returns a random cat!")]
        [FullDescription("Gets a random cat image from The Cat API!")]
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
            CatBreed retrievedBreed = retrievedImage.Breeds?.FirstOrDefault();

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

            replyEmbed.Title = "\U0001f431 Random Cat";
            replyEmbed.Description = retrievedBreed != default(CatBreed) ?
                $"\U0001f43e **Breed**: {retrievedBreed.Name}\n" +
                $"\u2764\uFE0F **Temperament**: {retrievedBreed.Temperament}" 
                : string.Empty;
            
            replyEmbed.Color = new Color(255, 204, 77);
            replyEmbed.ImageUrl = retrievedImage.Url;

            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await FollowupAsync(null, embed: replyEmbed.Build(), allowedMentions: allowedMentions);

            return ExecutionResult.Succesful();
        }

        [SlashCommand("dog", "Returns a random dog!")]
        [FullDescription("Gets a random dog image from The Dog API!")]
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
            DogBreed retrievedBreed = retrievedImage.Breeds?.FirstOrDefault();

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

            replyEmbed.Title = "\U0001f436 Random Dog";
            replyEmbed.Description = retrievedBreed != default(DogBreed) ?
                $"\U0001f43e **Breed**: {retrievedBreed.Name}\n" +
                $"\u2764\uFE0F **Temperament**: {retrievedBreed.Temperament}" 
                : string.Empty;
            
            replyEmbed.Color = new Color(217, 158, 130);
            replyEmbed.ImageUrl = retrievedImage.Url;

            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await FollowupAsync(null, embed: replyEmbed.Build(), allowedMentions: allowedMentions);

            return ExecutionResult.Succesful();
        }

        [SlashCommand("fox", "Returns a random fox!")]
        [FullDescription("Gets a random fox image from the RandomFox API!")]
        [RateLimit(3, 2)]
        public async Task<RuntimeResult> GetFoxAsync()
        {
            await DeferAsync();
            
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

            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await FollowupAsync(null, embed: replyEmbed.Build(), allowedMentions: allowedMentions);

            return ExecutionResult.Succesful();
        }

        [SlashCommand("duck", "Returns a random duck!")]
        [FullDescription("Gets a random duck image from the RandomDuk API!")]
        [RateLimit(3, 2)]
        public async Task<RuntimeResult> GetDuckAsync()
        {
            await DeferAsync();
            
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

            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await FollowupAsync(null, embed: replyEmbed.Build(), allowedMentions: allowedMentions);

            return ExecutionResult.Succesful();
        }
    }
}
