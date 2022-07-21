using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
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

		private int[] ShipSegments = new int[10]
		{
			10, 20, 30, 40, 50, 60, 70, 80, 90, 100
		};

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
		[Summary("Pats a user!")]
		[MustRunInGuild]
		public async Task<RuntimeResult> PatUserAsync(IUser User)
		{
			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync($"(c・_・)ノ”<@{User.Id}>", allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("dox")]
		[Alias("doxx")]
		[Summary("Leak someone's (fake) IP address!")]
		[MustRunInGuild]
		public async Task<RuntimeResult> DoxUserAsync(IUser User)
		{
			int FirstSegment = Settings.Instance.GlobalRng.Next(0, 256);
			int SecondSegment = Settings.Instance.GlobalRng.Next(0, 256);
			int ThirdSegment = Settings.Instance.GlobalRng.Next(0, 256);
			int FourthSegment = Settings.Instance.GlobalRng.Next(0, 256);

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync($"<@{User.Id}>'s IPv4 address: `{FirstSegment}.{SecondSegment}.{ThirdSegment}.{FourthSegment}`", allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("ship")]
		[Alias("loverating, shiprating")]
		[Summary("Ship 2 users together! Awww!")]
		[MustRunInGuild]
		public async Task<RuntimeResult> ShipUsersAsync(SocketGuildUser FirstUser, SocketGuildUser SecondUser = null)
		{
			//If the second user is null, ship the author with the first user.
			SocketGuildUser LoverOne;
			SocketGuildUser LoverTwo;
			if (SecondUser == null)
			{
				LoverOne = Context.Message.Author as SocketGuildUser;
				LoverTwo = FirstUser;
			}
			else
			{
				LoverOne = FirstUser;
				LoverTwo = SecondUser;
			}

			//Get random ship percentage and text.
			int Percentage = Settings.Instance.GlobalRng.Next(0, 101);
			string PercentageText = string.Empty;

			switch(Percentage)
			{
				case 0:
					PercentageText = "Incompatible! ❌";
					break;
				case > 0 and <= 25:
					PercentageText = "Awful! 💔";
					break;
				case > 25 and <= 50:
					PercentageText = "Not bad! ❤️‍🩹";
					break;
				case > 50 and <= 75:
					PercentageText = "Decent! ❤️";
					break;
				case > 75 and <= 85:
					PercentageText = "True love! 💖";
					break;
				case > 85 and < 100:
					PercentageText = "AMAZING! 💛";
					break;
				case 100:
					PercentageText = "INSANE! 💗";
					break;
			}

			//Split usernames into halves, then sanitize them.
			string LoverOneUsername = LoverOne.GetUsernameOrNick();
			string LoverTwoUsername = LoverTwo.GetUsernameOrNick();

			if(LoverOneUsername.Length != 1)
				LoverOneUsername = LoverOneUsername.Substring(0, LoverOneUsername.Length / 2);
			if(LoverTwoUsername.Length != 1)
				LoverTwoUsername = LoverTwoUsername.Substring(LoverTwoUsername.Length / 2, (int)Math.Ceiling(LoverTwoUsername.Length / 2f));

			LoverOneUsername = Format.Sanitize(LoverOneUsername);
			LoverTwoUsername = Format.Sanitize(LoverTwoUsername);

			EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).WithTitle(string.Empty).WithColor(new Color(221, 46, 68));

			string ProgressBar = string.Empty;
			for (int i = 0; i < ShipSegments.Length; i++)
			{
				if (Percentage < ShipSegments[i])
				{
					if (i == 0) ProgressBar += Settings.Instance.LoadedConfig.ShipBarStartEmpty;
					else if (i == ShipSegments.Length - 1) ProgressBar += Settings.Instance.LoadedConfig.ShipBarEndEmpty;
					else ProgressBar += Settings.Instance.LoadedConfig.ShipBarHalfEmpty;
				}
				else
				{
					if (i == 0) ProgressBar += Settings.Instance.LoadedConfig.ShipBarStartFull;
					else if (i == ShipSegments.Length - 1) ProgressBar += Settings.Instance.LoadedConfig.ShipBarEndFull;
					else ProgressBar += Settings.Instance.LoadedConfig.ShipBarHalfFull;
				}
			}

			ReplyEmbed.Description += $"🔀 **Ship Name**: {LoverOneUsername}{LoverTwoUsername}\n";
			ReplyEmbed.Description += $"{ProgressBar} **{Percentage}%** {PercentageText}";

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync($"💘 **THE SHIP-O-MATIC 5000** 💘\n" +
				$"🔹 {Format.Sanitize(LoverOne.GetUsernameOrNick())}\n" +
				$"🔹 {Format.Sanitize(LoverTwo.GetUsernameOrNick())}\n", embed: ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("kill")]
		[Alias("murder")]
		[Summary("Commit first degree murder, fuck it.")]
		[MustRunInGuild]
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
