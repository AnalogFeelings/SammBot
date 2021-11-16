using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using SammBotNET.Core;
using SammBotNET.Database;
using SammBotNET.Extensions;
using SammBotNET.Services;
using SharpCat.Types.Cat;
using SharpCat.Types.Dog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Random")]
    [Summary("Random crazyness!")]
    [Group("random")]
    public class RandomModule : ModuleBase<SocketCommandContext>
    {
        public RandomService RandomService { get; set; }

        [Command("cat")]
        [Alias("kit", "kitto", "cogga")]
        [Summary("Returns a random cat!")]
        public async Task<RuntimeResult> GetCatAsync()
        {
            CatSearchParams searchParams = new()
            {
                has_breeds = true,
                mime_types = "jpg,png",
                size = "small",
                sub_id = Context.Message.Author.Username,
                limit = 1
            };

            List<CatImage> images = await RandomService.CatRequester.GetImageAsync(searchParams);

            EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context, description: $"__Breed__: **{images[0].Breeds[0].Name}**" +
                $"\n__Temperament__: *{images[0].Breeds[0].Temperament}*").ChangeTitle("Random Cat");

            embed.ImageUrl = images[0].Url;

            await Context.Channel.SendMessageAsync("", false, embed.Build());

            return ExecutionResult.Succesful();
        }

        [Command("dog")]
        [Alias("doggo", "dogger")]
        [Summary("Returns a random cat!")]
        public async Task<RuntimeResult> GetDogAsync()
        {
            DogSearchParams searchParams = new()
            {
                has_breeds = true,
                mime_types = "jpg,png",
                size = "small",
                sub_id = Context.Message.Author.Username,
                limit = 1
            };

            List<DogImage> images = await RandomService.DogRequester.GetImageAsync(searchParams);

            EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context, description: $"__Breed__: **{images[0].Breeds[0].Name}**" +
                $"\n__Temperament__: *{images[0].Breeds[0].Temperament}*").ChangeTitle("Random Dog");

            embed.ImageUrl = images[0].Url;

            await Context.Channel.SendMessageAsync("", false, embed.Build());

            return ExecutionResult.Succesful();
        }

        [Command("peone")]
        [Summary("Returns a random image of Peone.")]
        public async Task<RuntimeResult> GetPeoneAsync()
        {
            using (PeoneImagesDB PeoneDatabase = new())
            {
                List<PeoneImage> peoneImages = await PeoneDatabase.PeoneImage.ToListAsync();
            chooseImage:
                PeoneImage selectedPeoneImage = peoneImages.PickRandom();

                if (RandomService.RecentPeoneImages.Contains(selectedPeoneImage.TwitterUrl)) goto chooseImage;
                RandomService.RecentPeoneImages.Push(selectedPeoneImage.TwitterUrl);

                EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle("Random Peone");
                embed.Color = new Color(105, 219, 221);

                embed.ImageUrl = selectedPeoneImage.TwitterUrl;

                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }

            return ExecutionResult.Succesful();
        }

        [Command("scp")]
        [Summary("Returns a random SCP!")]
        public async Task<RuntimeResult> GetSCPAsync()
        {
            int maxSCP = 5999;
            int result = GlobalConfig.Instance.GlobalRng.Next(maxSCP + 1);

            EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context, description: "http://www.scp-wiki.net/scp-" + result.ToString("D3"))
                                                    .ChangeTitle("Random SCP");

            await Context.Channel.SendMessageAsync("", false, embed.Build());

            return ExecutionResult.Succesful();
        }
    }
}
