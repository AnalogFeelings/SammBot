using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharpCat.Types.Cat;
using SharpCat.Types.Dog;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
	[Name("Random")]
	[Group("random")]
	[Summary("Random crazyness!")]
	[ModuleEmoji("🎰")]
	public class RandomModule : ModuleBase<SocketCommandContext>
	{
		public RandomService RandomService { get; set; }

		[Command("cat")]
		[Alias("kit", "kitto", "cogga")]
		[Summary("Returns a random cat!")]
		public async Task<RuntimeResult> GetCatAsync()
		{
			CatImageSearchParams searchParams = new CatImageSearchParams()
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
			DogImageSearchParams searchParams = new DogImageSearchParams()
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
			using (PeoneImagesDB PeoneDatabase = new PeoneImagesDB())
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

		[Command("fox")]
		[Summary("Returns a random fox!")]
		public async Task<RuntimeResult> GetFoxAsync()
		{
			string jsonReply = string.Empty;

			using (HttpResponseMessage response = await RandomService.RandomClient.GetAsync("https://randomfox.ca/floof/"))
			{
				jsonReply = await response.Content.ReadAsStringAsync();
			}

			FoxImage foxReply = JsonConvert.DeserializeObject<FoxImage>(jsonReply);

			EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle("Random Fox");

			embed.ImageUrl = foxReply.ImageUrl;

			await Context.Channel.SendMessageAsync("", false, embed.Build());

			return ExecutionResult.Succesful();
		}

		[Command("duck")]
		[Summary("Returns a random duck!")]
		public async Task<RuntimeResult> GetDuckAsync()
		{
			string jsonReply = string.Empty;

			using (HttpResponseMessage response = await RandomService.RandomClient.GetAsync("https://random-d.uk/api/v2/random"))
			{
				jsonReply = await response.Content.ReadAsStringAsync();
			}

			DuckImage duckReply = JsonConvert.DeserializeObject<DuckImage>(jsonReply);

			EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle("Random Duck");
			embed.ImageUrl = duckReply.ImageUrl;

			await Context.Channel.SendMessageAsync("", false, embed.Build());

			return ExecutionResult.Succesful();
		}

		[Command("scp")]
		[Summary("Returns a random SCP!")]
		public async Task<RuntimeResult> GetSCPAsync()
		{
			int maxSCP = 6999;
			int result = Settings.Instance.GlobalRng.Next(maxSCP + 1);

			EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context, description: "http://www.scp-wiki.net/scp-" + result.ToString("D3"))
													.ChangeTitle("Random SCP");

			await Context.Channel.SendMessageAsync("", false, embed.Build());

			return ExecutionResult.Succesful();
		}
	}
}
