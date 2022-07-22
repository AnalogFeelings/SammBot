using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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

		private readonly int[] ShipSegments = new int[10]
		{
			10, 20, 30, 40, 50, 60, 70, 80, 90, 100
		};

		private const string TWEMOJI_ASSETS = "https://raw.githubusercontent.com/twitter/twemoji/ad3d3d669bb3697946577247ebb15818f09c6c91/assets/72x72/";

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
			string ImageFilename = "shipImage.png";

			//If the second user is null, ship the author with the first user.
			if (SecondUser == null)
			{
				SecondUser = FirstUser;
				FirstUser = Context.Message.Author as SocketGuildUser;
			}

			//Get random ship percentage and text.
			int Percentage = Settings.Instance.GlobalRng.Next(0, 101);
			string PercentageText = string.Empty;
			string PercentageEmoji = string.Empty;

			switch(Percentage)
			{
				case 0:
					PercentageText = "Incompatible!";
					PercentageEmoji = "❌";
					break;
				case > 0 and <= 25:
					PercentageText = "Awful!";
					PercentageEmoji = "💔";
					break;
				case > 25 and <= 50:
					PercentageText = "Not bad!";
					PercentageEmoji = "❤️";
					break;
				case > 50 and <= 75:
					PercentageText = "Decent!";
					PercentageEmoji = "💝";
					break;
				case > 75 and <= 85:
					PercentageText = "True love!";
					PercentageEmoji = "💖";
					break;
				case > 85 and < 100:
					PercentageText = "AMAZING!";
					PercentageEmoji = "💛";
					break;
				case 100:
					PercentageText = "INSANE!";
					PercentageEmoji = "💗";
					break;
			}

			//Split usernames into halves, then sanitize them.
			string FirstUserName = FirstUser.GetUsernameOrNick();
			string SecondUserName = SecondUser.GetUsernameOrNick();

			string NameFirstHalf = string.Empty;
			string NameSecondHalf = string.Empty;

			if(FirstUserName.Length != 1)
				NameFirstHalf = FirstUserName.Substring(0, FirstUserName.Length / 2);
			if(SecondUserName.Length != 1)
				NameSecondHalf = SecondUserName.Substring(SecondUserName.Length / 2, (int)Math.Ceiling(SecondUserName.Length / 2f));

			NameFirstHalf = Format.Sanitize(NameFirstHalf);
			NameSecondHalf = Format.Sanitize(NameSecondHalf);

			//Sanitize usernames now, if we do it earlier, it would mess up the splitting code.
			FirstUserName = Format.Sanitize(FirstUserName);
			SecondUserName = Format.Sanitize(SecondUserName);

			//Fill up ship progress bar.
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

			//Download their profile pictures and store into memory stream.
			//Also download the emoji from Twemoji's GitHub.
			byte[] FirstUserAvatarRaw = await FunService.FunHttpClient.GetByteArrayAsync(FirstUser.GetGuildGlobalOrDefaultAvatar(2048));
			byte[] SecondUserAvatarRaw = await FunService.FunHttpClient.GetByteArrayAsync(SecondUser.GetGuildGlobalOrDefaultAvatar(2048));

			Encoding EmojiEncoding = new UTF32Encoding(true, false);
			string EmojiUrl = TWEMOJI_ASSETS + Convert.ToHexString(EmojiEncoding.GetBytes(PercentageEmoji)).TrimStart('0').ToLower() + ".png";

			byte[] EmojiImageRaw = await FunService.FunHttpClient.GetByteArrayAsync(EmojiUrl);

			MemoryStream FirstUserAvatarStream = new MemoryStream(FirstUserAvatarRaw);
			MemoryStream SecondUserAvatarStream = new MemoryStream(SecondUserAvatarRaw);
			MemoryStream EmojiStream = new MemoryStream(EmojiImageRaw);

			SKImageInfo ImageInfo = new SKImageInfo(384, 192);
			SKSurface Surface = SKSurface.Create(ImageInfo);

			SKBitmap EmojiBitmap = SKBitmap.Decode(EmojiStream);
			SKBitmap FirstUserAvatar = SKBitmap.Decode(FirstUserAvatarStream);
			SKBitmap SecondUserAvatar = SKBitmap.Decode(SecondUserAvatarStream);

			SKPath LoversClipPath = new SKPath();
			LoversClipPath.AddCircle(ImageInfo.Width / 4, ImageInfo.Height / 2, ImageInfo.Height / 2);
			LoversClipPath.AddCircle((int)(ImageInfo.Width / 1.3333f), ImageInfo.Height / 2, ImageInfo.Height / 2);

			SKPaint EmojiPaint = new SKPaint
			{
				IsAntialias = true,
				FilterQuality = SKFilterQuality.High,
				ImageFilter = SKImageFilter.CreateDropShadow(0, 0, 5, 5, SKColors.Black.WithAlpha(192))
			};

			SKImage Image = null;
			SKData Data = null;
			MemoryStream TargetStream = null;
			try
			{
				Surface.Canvas.Clear(SKColors.Transparent);

				Surface.Canvas.Save();

				Surface.Canvas.ClipPath(LoversClipPath, SKClipOperation.Intersect, true);
				Surface.Canvas.DrawBitmap(FirstUserAvatar, new SKRect(0, 0, ImageInfo.Width / 2, ImageInfo.Height));
				Surface.Canvas.DrawBitmap(SecondUserAvatar, new SKRect(ImageInfo.Width / 2, 0, ImageInfo.Width, ImageInfo.Height));

				Surface.Canvas.Restore();

				int EmojiSize = 32;
				Surface.Canvas.DrawBitmap(EmojiBitmap, new SKRect(ImageInfo.Width / 2 - EmojiSize, ImageInfo.Height / 2 - EmojiSize,
					ImageInfo.Width / 2 + EmojiSize, ImageInfo.Height / 2 + EmojiSize), EmojiPaint);

				Image = Surface.Snapshot();
				Data = Image.Encode(SKEncodedImageFormat.Png, 100);
				TargetStream = new MemoryStream((int)Data.Size);

				Data.SaveTo(TargetStream);

				EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).WithTitle(string.Empty).WithColor(new Color(221, 46, 68));

				ReplyEmbed.ImageUrl = $"attachment://{ImageFilename}";

				ReplyEmbed.Description += $"🔀 **Ship Name**: {NameFirstHalf}{NameSecondHalf}\n";
				ReplyEmbed.Description += $"{ProgressBar} **{Percentage}%** - {PercentageEmoji} {PercentageText}";

				MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
				AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
				await Context.Channel.SendFileAsync(TargetStream, ImageFilename, $"💘 **THE SHIP-O-MATIC 5000** 💘\n" +
					$"🔹 {FirstUserName}\n" +
					$"🔹 {SecondUserName}\n", embed: ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);
			}
			finally
			{
				if (FirstUserAvatarStream != null) FirstUserAvatarStream.Dispose();
				if (SecondUserAvatarStream != null) SecondUserAvatarStream.Dispose();
				if (Surface != null) Surface.Dispose();
				if (EmojiStream != null) EmojiStream.Dispose();
				if (EmojiBitmap != null) EmojiBitmap.Dispose();
				if (FirstUserAvatar != null) FirstUserAvatar.Dispose();
				if (SecondUserAvatar != null) SecondUserAvatar.Dispose();
				if (LoversClipPath != null) LoversClipPath.Dispose();
				if (EmojiPaint != null) EmojiPaint.Dispose();
				if (Image != null) Image.Dispose();
				if (Data != null) Data.Dispose();
				if (TargetStream != null) TargetStream.Dispose();
			}

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

			using (HttpResponseMessage Response = await FunService.FunHttpClient.GetAsync($"https://api.urbandictionary.com/v0/define?{QueryString}"))
			{
				JsonReply = await Response.Content.ReadAsStringAsync();
			}

			UrbanDefinitionList DefinitionReply = JsonConvert.DeserializeObject<UrbanDefinitionList>(JsonReply);

			return DefinitionReply;
		}
	}
}
