using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
	[Name("Fun")]
	[Group("fun")]
	[Summary("Games and fun!")]
	[ModuleEmoji("🎲")]
	public class FunModule : ModuleBase<SocketCommandContext>
	{
		public Logger Logger { get; set; }
		public FunService FunService { get; set; }

		[Command("8ball")]
		[Alias("ask", "8")]
		[Summary("Ask the magic 8 ball!")]
		public async Task<RuntimeResult> MagicBallAsync([Remainder] string Question)
		{
			string ChosenAnswer = Settings.Instance.LoadedConfig.MagicBallAnswers.PickRandom();

			IUserMessage ReplyMessage = await ReplyAsync(":8ball: Asking the magic 8-ball...");

			using (Context.Channel.EnterTypingState()) await Task.Delay(2000);

			await ReplyMessage.ModifyAsync(x => x.Content = $"<@{Context.Message.Author.Id}> **The magic 8-ball answered**:\n`{ChosenAnswer}`");

			return ExecutionResult.Succesful();
		}

		[Command("dice")]
		[Alias("roll")]
		[Summary("Roll the dice, and get a random number!")]
		public async Task<RuntimeResult> RollDiceAsync(int FaceCount = 6)
		{
			if (FaceCount < 3)
				return ExecutionResult.FromError("The dice must have at least 3 faces!");

			int ChosenNumber = Settings.Instance.GlobalRng.Next(0, FaceCount + 1);

			IUserMessage ReplyMessage = await ReplyAsync(":game_die: Rolling the dice...");

			using (Context.Channel.EnterTypingState()) await Task.Delay(1500);

			await ReplyMessage.ModifyAsync(x => x.Content = $"The dice landed on **{ChosenNumber}**!");

			return ExecutionResult.Succesful();
		}

		[Command("hug")]
		[Alias("cuddle")]
		[Summary("Hug a user!")]
		[MustRunInGuild]
		public async Task<RuntimeResult> HugUserAsync(IUser User)
		{
			string chosenKaomoji = Settings.Instance.LoadedConfig.HugKaomojis.PickRandom();

			SocketGuildUser authorAsGuild = Context.Message.Author as SocketGuildUser;

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, Context.Guild.Id, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync($"Warm hugs from **{authorAsGuild.GetUsernameOrNick()}**!\n{chosenKaomoji} <@{User.Id}>", allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("pat")]
		[MustRunInGuild]
		[Summary("Pats a user!")]
		public async Task<RuntimeResult> PatUserAsync(IUser User)
		{
			await ReplyAsync($"(c・_・)ノ”<@{User.Id}>");

			return ExecutionResult.Succesful();
		}

		[Command("kill")]
		[Alias("murder")]
		[MustRunInGuild]
		[Summary("Commit first degree murder, fuck it.")]
		public async Task<RuntimeResult> FirstDegreeMurderAsync(IUser User)
		{
			SocketGuildUser authorAsGuild = Context.Message.Author as SocketGuildUser;
			SocketGuildUser targetAsGuild = User as SocketGuildUser;

			Pronoun authorPronouns = await authorAsGuild.GetUserPronouns();
			Pronoun targetPronouns = await targetAsGuild.GetUserPronouns();

			string chosenMessage = Settings.Instance.LoadedConfig.KillMessages.PickRandom();
			chosenMessage = chosenMessage.Replace("{Murderer}", $"**{authorAsGuild.GetUsernameOrNick()}**");
			chosenMessage = chosenMessage.Replace("{mPrnSub}", authorPronouns.Subject);
			chosenMessage = chosenMessage.Replace("{mPrnObj}", authorPronouns.Object);
			chosenMessage = chosenMessage.Replace("{mPrnDepPos}", authorPronouns.DependentPossessive);
			chosenMessage = chosenMessage.Replace("{mPrnIndepPos}", authorPronouns.IndependentPossessive);
			chosenMessage = chosenMessage.Replace("{mPrnRefSing}", authorPronouns.ReflexiveSingular);
			chosenMessage = chosenMessage.Replace("{mPrnRefPlur}", authorPronouns.ReflexivePlural);

			chosenMessage = chosenMessage.Replace("{Victim}", $"**{targetAsGuild.GetUsernameOrNick()}**");
			chosenMessage = chosenMessage.Replace("{vPrnSub}", targetPronouns.Subject);
			chosenMessage = chosenMessage.Replace("{vPrnObj}", targetPronouns.Object);
			chosenMessage = chosenMessage.Replace("{vPrnDepPos}", targetPronouns.DependentPossessive);
			chosenMessage = chosenMessage.Replace("{vPrnIndepPos}", targetPronouns.IndependentPossessive);
			chosenMessage = chosenMessage.Replace("{vPrnRefSing}", targetPronouns.ReflexiveSingular);
			chosenMessage = chosenMessage.Replace("{vPrnRefPlur}", targetPronouns.ReflexivePlural);

			await ReplyAsync(chosenMessage);

			return ExecutionResult.Succesful();
		}

		[Command("urban")]
		[Summary("Gets a definition from the urban dictionary!")]
		public async Task<RuntimeResult> UrbanAsync([Remainder] string Term)
		{
			UrbanSearchParams searchParams = new()
			{
				term = Term
			};

			UrbanDefinitionList urbanDefinitions = null;
			using (Context.Channel.EnterTypingState()) urbanDefinitions = await GetUrbanDefinitionAsync(searchParams);

			if (urbanDefinitions == null || urbanDefinitions.List.Count == 0)
				return ExecutionResult.FromError($"Urban Dictionary returned no definitions for \"{Term}\"!");

			UrbanDefinition selectedDefinition = urbanDefinitions.List.First();

			string embedDescription = $"**Definition** : *{selectedDefinition.Definition.Truncate(1024)}*\n\n";
			embedDescription += $"**Example** : {(string.IsNullOrEmpty(selectedDefinition.Example) ? "No Example" : selectedDefinition.Example)}\n\n";
			embedDescription += $"**Author** : {selectedDefinition.Author}\n";
			embedDescription += $"**Thumbs Up** : {selectedDefinition.ThumbsUp}\n";
			embedDescription += $"**Thumbs Down** : {selectedDefinition.ThumbsDown}\n";

			EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context, Description: embedDescription);
			embed.ChangeTitle($"URBAN DEFINITION OF \"{selectedDefinition.Word}\"");
			embed.WithUrl(selectedDefinition.Permalink);

			await Context.Channel.SendMessageAsync(null, false, embed.Build());

			return ExecutionResult.Succesful();
		}

		public async Task<UrbanDefinitionList> GetUrbanDefinitionAsync(UrbanSearchParams searchParams)
		{
			string queryString = searchParams.ToQueryString();
			string jsonReply = string.Empty;

			using (HttpResponseMessage response = await FunService.UrbanClient.GetAsync($"/v0/define?{queryString}"))
			{
				jsonReply = await response.Content.ReadAsStringAsync();
			}

			UrbanDefinitionList definitionReply = JsonConvert.DeserializeObject<UrbanDefinitionList>(jsonReply);

			return definitionReply;
		}
	}
}
