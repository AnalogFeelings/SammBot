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
	[ModuleEmoji("\U0001F3B2")]
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
		[Summary("Ask the magic 8-ball!")]
		[FullDescription("Ask a question to the magic 8-ball! Not guaranteed to answer first try!")]
		[RateLimit(2, 1)]
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

		[Command("activity")]
		[Alias("game", "vcgame")]
		[Summary("Creates an invite for a voice channel activity!")]
		[FullDescription("Creates an invite for a voice channel activity. Read [this](https://discordnet.dev/api/Discord.DefaultApplications.html) for" +
			" a list of the available activities.")]
		[RequireContext(ContextType.Guild)]
		[RequireBotPermission(GuildPermission.CreateInstantInvite)]
		[RequireUserPermission(GuildPermission.CreateInstantInvite)]
		[RateLimit(6, 1)]
		public async Task<RuntimeResult> CreateActivityAsync(DefaultApplications ActivityType)
		{
			SocketGuildUser Author = Context.User as SocketGuildUser;
			if (Author.VoiceChannel == null)
				return ExecutionResult.FromError("You must be in a voice channel to create an activity!");

			IInviteMetadata Invite = await Author.VoiceChannel.CreateInviteToApplicationAsync(ActivityType);

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync($":warning: **Most activities only work if the server has a Nitro Boost level of at least 1.**\n\n" +
				$"{Invite.Url}", allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("dice")]
		[Alias("roll")]
		[Summary("Roll the dice, and get a random number!")]
		[FullDescription("Roll the dice! It returns a random number between 1 and **FaceCount**. **FaceCount** must be larger than 3!")]
		[RateLimit(2, 1)]
		public async Task<RuntimeResult> RollDiceAsync(int FaceCount = 6)
		{
			if (FaceCount < 3)
				return ExecutionResult.FromError("The dice must have at least 3 faces!");

			int ChosenNumber = Random.Shared.Next(1, FaceCount + 1);

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
		[FullDescription("Hugs are good for everyone! Spread the joy with this command.")]
		[RequireContext(ContextType.Guild)]
		[RateLimit(3, 1)]
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
		[FullDescription("Pets are ALSO good for everyone! Spread the joy with this command.")]
		[RequireContext(ContextType.Guild)]
		[RateLimit(3, 1)]
		public async Task<RuntimeResult> PatUserAsync(IUser User)
		{
			SocketGuildUser AuthorGuildUser = Context.Message.Author as SocketGuildUser;

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync($"Pats from **{AuthorGuildUser.GetUsernameOrNick()}**!\n(c・_・)ノ”<@{User.Id}>", allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("dox")]
		[Alias("doxx")]
		[Summary("Leak someone's (fake) IP address!")]
		[FullDescription("Dox someone! Not guaranteed to be the user's actual IP.")]
		[RequireContext(ContextType.Guild)]
		[RateLimit(3, 1)]
		public async Task<RuntimeResult> DoxUserAsync(SocketGuildUser User)
		{
			int FirstSegment = Random.Shared.Next(0, 256);
			int SecondSegment = Random.Shared.Next(0, 256);
			int ThirdSegment = Random.Shared.Next(0, 256);
			int FourthSegment = Random.Shared.Next(0, 256);

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync($"**{User.GetUsernameOrNick()}**'s IPv4 address: `{FirstSegment}.{SecondSegment}.{ThirdSegment}.{FourthSegment}`", allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("kill")]
		[Alias("murder")]
		[Summary("Commit first degree murder, fuck it.")]
		[FullDescription("Commit first degree murder! Don't worry, its fictional, the police isn't after you.")]
		[RequireContext(ContextType.Guild)]
		[RateLimit(4, 1)]
		public async Task<RuntimeResult> FirstDegreeMurderAsync(SocketGuildUser TargetUser)
		{
			SocketGuildUser AuthorUser = Context.Message.Author as SocketGuildUser;

			Pronoun AuthorPronouns = await AuthorUser.GetUserPronouns();
			Pronoun TargetPronouns = await TargetUser.GetUserPronouns();

			string ChosenMessage = Settings.Instance.LoadedConfig.KillMessages.PickRandom();
			ChosenMessage = ChosenMessage.Replace("{Murderer}", $"**{AuthorUser.GetUsernameOrNick()}**");
			ChosenMessage = ChosenMessage.Replace("{mPrnSub}", AuthorPronouns.Subject);
			ChosenMessage = ChosenMessage.Replace("{mPrnObj}", AuthorPronouns.Object);
			ChosenMessage = ChosenMessage.Replace("{mPrnDepPos}", AuthorPronouns.DependentPossessive);
			ChosenMessage = ChosenMessage.Replace("{mPrnIndepPos}", AuthorPronouns.IndependentPossessive);
			ChosenMessage = ChosenMessage.Replace("{mPrnRefSing}", AuthorPronouns.ReflexiveSingular);
			ChosenMessage = ChosenMessage.Replace("{mPrnRefPlur}", AuthorPronouns.ReflexivePlural);

			ChosenMessage = ChosenMessage.Replace("{Victim}", $"**{TargetUser.GetUsernameOrNick()}**");
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

		[Command("ship")]
		[Alias("loverating, shiprating")]
		[Summary("Ship 2 users together! Awww!")]
		[FullDescription("The Ship-O-Matic 5000 is here! If **SecondUser** is left empty, you will be shipped with **FirstUser**.")]
		[RequireContext(ContextType.Guild)]
		[RateLimit(5, 1)]
		public async Task<RuntimeResult> ShipUsersAsync(SocketGuildUser FirstUser, SocketGuildUser SecondUser = null)
		{
			//If the second user is null, ship the author with the first user.
			if (SecondUser == null)
			{
				SecondUser = FirstUser;
				FirstUser = Context.Message.Author as SocketGuildUser;
			}

			//Do not allow people to ship themselves, thats Sweetheart from OMORI levels of weird.
			if (FirstUser.Id == SecondUser.Id)
				return ExecutionResult.FromError("You can't ship yourself!");

			//Get random ship percentage and text.
			int Percentage = Random.Shared.Next(0, 101);
			string PercentageText = string.Empty;
			string PercentageEmoji = string.Empty;

			switch (Percentage)
			{
				case 0:
					PercentageText = "Incompatible!";
					PercentageEmoji = "❌";
					break;
				case > 0 and < 25:
					PercentageText = "Awful!";
					PercentageEmoji = "💔";
					break;
				case >= 25 and < 50:
					PercentageText = "Not Bad!";
					PercentageEmoji = "❤";
					break;
				case >= 50 and < 75:
					PercentageText = "Decent!";
					PercentageEmoji = "💝";
					break;
				case >= 75 and < 85:
					PercentageText = "True Love!";
					PercentageEmoji = "💖";
					break;
				case >= 85 and < 100:
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

			//Do the actual splitting.
			if (FirstUserName.Length != 1)
				NameFirstHalf = FirstUserName.Substring(0, FirstUserName.Length / 2);
			if (SecondUserName.Length != 1)
				NameSecondHalf = SecondUserName.Substring(SecondUserName.Length / 2, (int)Math.Ceiling(SecondUserName.Length / 2f));

			//Sanitize splitted halves.
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
					if (i == 0)
						ProgressBar += Settings.Instance.LoadedConfig.ShipBarStartEmpty;
					else if (i == ShipSegments.Length - 1)
						ProgressBar += Settings.Instance.LoadedConfig.ShipBarEndEmpty;
					else
						ProgressBar += Settings.Instance.LoadedConfig.ShipBarHalfEmpty;
				}
				else
				{
					if (i == 0)
						ProgressBar += Settings.Instance.LoadedConfig.ShipBarStartFull;
					else if (i == ShipSegments.Length - 1)
						ProgressBar += Settings.Instance.LoadedConfig.ShipBarEndFull;
					else
						ProgressBar += Settings.Instance.LoadedConfig.ShipBarHalfFull;
				}
			}

			//Twemoji's repository expects filenames in big endian UTF-32, with no leading zeroes AND in PNG format.
			Encoding EmojiEncoding = new UTF32Encoding(true, false);
			string EmojiUrl = TWEMOJI_ASSETS + Convert.ToHexString(EmojiEncoding.GetBytes(PercentageEmoji)).TrimStart('0').ToLower() + ".png";

			//Image generation code is so fucking ugly.
			//Buckle up, this is a bumpy ride.

			//Create image resolution information.
			SKImageInfo ImageInfo = new SKImageInfo(384, 192);

			//Download their profile pictures and store into memory stream.
			//Also download the emoji from Twemoji's GitHub.
			using (MemoryStream FirstUserAvatarStream = await DownloadToMemoryStream(FirstUser.GetGuildGlobalOrDefaultAvatar(2048)))
			using (MemoryStream SecondUserAvatarStream = await DownloadToMemoryStream(SecondUser.GetGuildGlobalOrDefaultAvatar(2048)))
			using (MemoryStream EmojiStream = await DownloadToMemoryStream(EmojiUrl))

			//Create the actual drawing surface.
			using (SKSurface Surface = SKSurface.Create(ImageInfo))
			{
				Surface.Canvas.Clear(SKColors.Transparent);

				using (SKBitmap FirstUserAvatar = SKBitmap.Decode(FirstUserAvatarStream))
				using (SKBitmap SecondUserAvatar = SKBitmap.Decode(SecondUserAvatarStream))
				using (SKBitmap EmojiBitmap = SKBitmap.Decode(EmojiStream))
				using (SKPath LoversClipPath = new SKPath())
				{
					//Add the two "Windows" to the clip path. They have their origin in the center, not the top left corner.
					LoversClipPath.AddCircle(ImageInfo.Width / 4, ImageInfo.Height / 2, ImageInfo.Height / 2);
					LoversClipPath.AddCircle((int)(ImageInfo.Width / 1.3333f), ImageInfo.Height / 2, ImageInfo.Height / 2);

					//Save canvas state.
					Surface.Canvas.Save();

					//Set clip path and draw the 2 profile pictures.
					Surface.Canvas.ClipPath(LoversClipPath, SKClipOperation.Intersect, true);
					Surface.Canvas.DrawBitmap(FirstUserAvatar, new SKRect(0, 0, ImageInfo.Width / 2, ImageInfo.Height));
					Surface.Canvas.DrawBitmap(SecondUserAvatar, new SKRect(ImageInfo.Width / 2, 0, ImageInfo.Width, ImageInfo.Height));

					//Restore the canvas state, currently the only way to remove a clip path.
					Surface.Canvas.Restore();

					//Use a custom filter with a drop shadow effect.
					using (SKPaint EmojiPaint = new SKPaint())
					{
						EmojiPaint.IsAntialias = true;
						EmojiPaint.FilterQuality = SKFilterQuality.High;
						EmojiPaint.ImageFilter = SKImageFilter.CreateDropShadow(0, 0, 5, 5, SKColors.Black.WithAlpha(192));

						//Draw the emoji in the middle of the image, do some math trickery to get it perfectly centered
						//since bitmaps have their origin in their top left corner.
						int EmojiSize = 32;
						Surface.Canvas.DrawBitmap(EmojiBitmap, new SKRect(ImageInfo.Width / 2 - EmojiSize, ImageInfo.Height / 2 - EmojiSize,
							ImageInfo.Width / 2 + EmojiSize, ImageInfo.Height / 2 + EmojiSize), EmojiPaint);
					}
				}

				//Take snapshot, encode it into PNG, store it into MemoryStream to be uploaded to Discord.
				using (SKImage SurfaceSnapshot = Surface.Snapshot())
				using (SKData ImageData = SurfaceSnapshot.Encode(SKEncodedImageFormat.Png, 100))
				using (MemoryStream FinalImageStream = new MemoryStream((int)ImageData.Size))
				{
					//Save the actual image into the stream.
					ImageData.SaveTo(FinalImageStream);

					//Build the message itself.
					//Start by creating an embed with no title and the color of the red heart emoji.
					EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).WithTitle(string.Empty).WithColor(new Color(221, 46, 68));

					//Tell Discord that the image will be uploaded from the local storage.
					ReplyEmbed.ImageUrl = $"attachment://shipImage.png";

					ReplyEmbed.Description += $":twisted_rightwards_arrows:  **Ship Name**: {NameFirstHalf}{NameSecondHalf}\n";
					ReplyEmbed.Description += $"{ProgressBar} **{Percentage}%** - {PercentageEmoji} {PercentageText}";

					//Set the raw text outisde the embed.
					string PreEmbedText = ":cupid: **THE SHIP-O-MATIC 5000** :cupid:\n";
					PreEmbedText += $":small_blue_diamond: {FirstUserName}\n";
					PreEmbedText += $":small_blue_diamond: {SecondUserName}\n";

					MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
					AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

					//Use SendFileAsync to be able to upload the stream to Discord's servers. The file name has to be the same as the one set in ImageUrl.
					await Context.Channel.SendFileAsync(FinalImageStream, "shipImage.png", PreEmbedText,
						embed: ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);
				}
			}

			return ExecutionResult.Succesful();
		}

		public async Task<MemoryStream> DownloadToMemoryStream(string Url)
		{
			byte[] RawData = await FunService.FunHttpClient.GetByteArrayAsync(Url);

			return new MemoryStream(RawData);
		}

		[Command("urban")]
		[Summary("Gets a definition from the urban dictionary!")]
		[FullDescription("Gets a definition from the urban dictionary. Click the embed's title to open the definition in your browser.")]
		[RateLimit(6, 1)]
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
