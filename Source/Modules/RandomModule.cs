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
		[FullDescription("Gets a random cat image from The Cat API!")]
		public async Task<RuntimeResult> GetCatAsync()
		{
			CatImageSearchParams SearchParameters = new CatImageSearchParams()
			{
				has_breeds = true,
				mime_types = "jpg,png,gif",
				size = "small",
				limit = 1
			};

			List<CatImage> RetrievedImages = await RandomService.CatRequester.GetImageAsync(SearchParameters);

			EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, Description: ":warning: **The breed information has been " +
				"temporarily removed due to a bug in the Cat API.**").ChangeTitle("Random Cat");

			ReplyEmbed.ImageUrl = RetrievedImages[0].Url;

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync(null, false, ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("dog")]
		[Alias("doggo", "dogger")]
		[Summary("Returns a random dog!")]
		[FullDescription("Gets a random dog image from The Dog API!")]
		public async Task<RuntimeResult> GetDogAsync()
		{
			DogImageSearchParams SearchParameters = new DogImageSearchParams()
			{
				has_breeds = true,
				mime_types = "jpg,png,gif",
				size = "small",
				sub_id = Context.Message.Author.Username,
				limit = 1
			};

			List<DogImage> RetrievedImages = await RandomService.DogRequester.GetImageAsync(SearchParameters);

			EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, Description: $"**__Breed__**: {RetrievedImages[0].Breeds[0].Name}" +
					$"\n**__Temperament__**: {RetrievedImages[0].Breeds[0].Temperament}").ChangeTitle("Random Dog");

			ReplyEmbed.ImageUrl = RetrievedImages[0].Url;

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync(null, false, ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("peone")]
		[Summary("Returns a random image of Peone.")]
		[FullDescription("Returns a random image of Noah's favourite snake woman.")]
		public async Task<RuntimeResult> GetPeoneAsync()
		{
			using (BotDatabase BotDatabase = new BotDatabase())
			{
				List<PeoneImage> ImageList = await BotDatabase.PeoneImages.ToListAsync();
			RechooseImage:
				PeoneImage ChosenImage = ImageList.PickRandom();

				if (RandomService.RecentPeoneImages.Contains(ChosenImage.TwitterUrl)) goto RechooseImage;
				RandomService.RecentPeoneImages.Push(ChosenImage.TwitterUrl);

				EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle("Random Peone");
				ReplyEmbed.Color = new Color(105, 219, 221);

				ReplyEmbed.ImageUrl = ChosenImage.TwitterUrl;

				MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
				AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
				await ReplyAsync(null, false, ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);
			}

			return ExecutionResult.Succesful();
		}

		[Command("fox")]
		[Summary("Returns a random fox!")]
		[FullDescription("Gets a random fox image from the RandomFox API!")]
		public async Task<RuntimeResult> GetFoxAsync()
		{
			string JsonReply = string.Empty;

			using (HttpResponseMessage HttpResponse = await RandomService.RandomClient.GetAsync("https://randomfox.ca/floof/"))
			{
				JsonReply = await HttpResponse.Content.ReadAsStringAsync();
			}

			FoxImage RepliedImage = JsonConvert.DeserializeObject<FoxImage>(JsonReply);

			EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle("Random Fox");
			ReplyEmbed.ImageUrl = RepliedImage.ImageUrl;

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync(null, false, ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("duck")]
		[Summary("Returns a random duck!")]
		[FullDescription("Gets a random duck image from the RandomDuk API!")]
		public async Task<RuntimeResult> GetDuckAsync()
		{
			string JsonReply = string.Empty;

			using (HttpResponseMessage HttpResponse = await RandomService.RandomClient.GetAsync("https://random-d.uk/api/v2/random"))
			{
				JsonReply = await HttpResponse.Content.ReadAsStringAsync();
			}

			DuckImage RepliedImage = JsonConvert.DeserializeObject<DuckImage>(JsonReply);

			EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).ChangeTitle("Random Duck");
			ReplyEmbed.ImageUrl = RepliedImage.ImageUrl;

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync(null, false, ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("scp")]
		[Summary("Returns a random SCP!")]
		[FullDescription("Returns a random SCP! The article may not exist.")]
		public async Task<RuntimeResult> GetSCPAsync()
		{
			int MaxNumber = 6999;
			int ChosenNumber = Settings.Instance.GlobalRng.Next(MaxNumber + 1);

			EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, Description: "http://www.scp-wiki.net/scp-" + ChosenNumber.ToString("D3"))
														.ChangeTitle("Random SCP");

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync(null, false, ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}
	}
}
