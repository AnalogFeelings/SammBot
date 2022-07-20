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

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			IUserMessage ReplyMessage = await ReplyAsync(":8ball: Asking the magic 8-ball...", allowedMentions: AllowedMentions, messageReference: Reference);

			using (Context.Channel.EnterTypingState()) await Task.Delay(2000);

			await ReplyMessage.ModifyAsync(x => x.Content = $"The magic 8-ball has answered!\n`{ChosenAnswer}`");

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

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			IUserMessage ReplyMessage = await ReplyAsync(":game_die: Rolling the dice...", allowedMentions: AllowedMentions, messageReference: Reference);

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
			string ChosenKaomoji = Settings.Instance.LoadedConfig.HugKaomojis.PickRandom();

			SocketGuildUser AuthorGuildUser = Context.Message.Author as SocketGuildUser;

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync($"Warm hugs from **{AuthorGuildUser.GetUsernameOrNick()}**!\n{ChosenKaomoji} <@{User.Id}>", allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("pat")]
		[MustRunInGuild]
		[Summary("Pats a user!")]
		public async Task<RuntimeResult> PatUserAsync(IUser User)
		{
			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync($"(c・_・)ノ”<@{User.Id}>", allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("kill")]
		[Alias("murder")]
		[MustRunInGuild]
		[Summary("Commit first degree murder, fuck it.")]
		public async Task<RuntimeResult> FirstDegreeMurderAsync(IUser User)
		{
			SocketGuildUser AuthorGuildUser = Context.Message.Author as SocketGuildUser;
			SocketGuildUser TargetGuildUser = User as SocketGuildUser;

			Pronoun AuthorPronouns = await AuthorGuildUser.GetUserPronouns();
			Pronoun TargetPronouns = await TargetGuildUser.GetUserPronouns();

			string ChosenMessage = Settings.Instance.LoadedConfig.KillMessages.PickRandom();
			ChosenMessage = ChosenMessage.Replace("{Murderer}", $"**{AuthorGuildUser.GetUsernameOrNick()}**");
			ChosenMessage = ChosenMessage.Replace("{mPrnSub}", AuthorPronouns.Subject);
			ChosenMessage = ChosenMessage.Replace("{mPrnObj}", AuthorPronouns.Object);
			ChosenMessage = ChosenMessage.Replace("{mPrnDepPos}", AuthorPronouns.DependentPossessive);
			ChosenMessage = ChosenMessage.Replace("{mPrnIndepPos}", AuthorPronouns.IndependentPossessive);
			ChosenMessage = ChosenMessage.Replace("{mPrnRefSing}", AuthorPronouns.ReflexiveSingular);
			ChosenMessage = ChosenMessage.Replace("{mPrnRefPlur}", AuthorPronouns.ReflexivePlural);

			ChosenMessage = ChosenMessage.Replace("{Victim}", $"**{TargetGuildUser.GetUsernameOrNick()}**");
			ChosenMessage = ChosenMessage.Replace("{vPrnSub}", TargetPronouns.Subject);
			ChosenMessage = ChosenMessage.Replace("{vPrnObj}", TargetPronouns.Object);
			ChosenMessage = ChosenMessage.Replace("{vPrnDepPos}", TargetPronouns.DependentPossessive);
			ChosenMessage = ChosenMessage.Replace("{vPrnIndepPos}", TargetPronouns.IndependentPossessive);
			ChosenMessage = ChosenMessage.Replace("{vPrnRefSing}", TargetPronouns.ReflexiveSingular);
			ChosenMessage = ChosenMessage.Replace("{vPrnRefPlur}", TargetPronouns.ReflexivePlural);

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync(ChosenMessage, allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("urban")]
		[Summary("Gets a definition from the urban dictionary!")]
		public async Task<RuntimeResult> UrbanAsync([Remainder] string Term)
		{
			UrbanSearchParams SearchParameters = new()
			{
				term = Term
			};

			UrbanDefinitionList UrbanDefinitions = null;
			using (Context.Channel.EnterTypingState()) UrbanDefinitions = await GetUrbanDefinitionAsync(SearchParameters);

			if (UrbanDefinitions == null || UrbanDefinitions.List.Count == 0)
				return ExecutionResult.FromError($"Urban Dictionary returned no definitions for \"{Term}\"!");

			UrbanDefinition ChosenDefinition = UrbanDefinitions.List.First();

			string EmbedDescription = $"**Definition** : *{ChosenDefinition.Definition.Truncate(1024)}*\n\n";
			EmbedDescription += $"**Example** : {(string.IsNullOrEmpty(ChosenDefinition.Example) ? "No Example" : ChosenDefinition.Example)}\n\n";
			EmbedDescription += $"**Author** : {ChosenDefinition.Author}\n";
			EmbedDescription += $"**Thumbs Up** : {ChosenDefinition.ThumbsUp}\n";
			EmbedDescription += $"**Thumbs Down** : {ChosenDefinition.ThumbsDown}\n";

			EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, Description: EmbedDescription);
			ReplyEmbed.ChangeTitle($"URBAN DEFINITION OF \"{ChosenDefinition.Word}\"");
			ReplyEmbed.WithUrl(ChosenDefinition.Permalink);

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync(null, false, ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		public async Task<UrbanDefinitionList> GetUrbanDefinitionAsync(UrbanSearchParams searchParams)
		{
			string QueryString = searchParams.ToQueryString();
			string JsonReply = string.Empty;

			using (HttpResponseMessage Response = await FunService.UrbanClient.GetAsync($"/v0/define?{QueryString}"))
			{
				JsonReply = await Response.Content.ReadAsStringAsync();
			}

			UrbanDefinitionList DefinitionReply = JsonConvert.DeserializeObject<UrbanDefinitionList>(JsonReply);

			return DefinitionReply;
		}
	}
}
