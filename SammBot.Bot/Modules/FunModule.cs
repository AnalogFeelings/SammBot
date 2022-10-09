﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using SammBot.Bot.Classes;
using SammBot.Bot.Core;
using SammBot.Bot.Database;
using SammBot.Bot.RestDefinitions;
using SammBot.Bot.Services;
using SkiaSharp;
using Svg.Skia;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SammBot.Bot.Modules
{
    [Name("Fun")]
    [Group("fun")]
    [Summary("Games and fun!")]
    [ModuleEmoji("\U0001F3B2")]
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        public Logger Logger { get; set; }
        public FunService FunService { get; set; }

        private readonly int[] _ShipSegments = new int[10]
        {
            10, 20, 30, 40, 50, 60, 70, 80, 90, 100
        };

        private const string _TWEMOJI_ASSETS = "https://raw.githubusercontent.com/twitter/twemoji/ad3d3d669bb3697946577247ebb15818f09c6c91/assets/svg/";

        [Command("8ball")]
        [Alias("ask", "8")]
        [Summary("Ask the magic 8-ball!")]
        [FullDescription("Ask a question to the magic 8-ball! Not guaranteed to answer first try!")]
        [RateLimit(2, 1)]
        public async Task<RuntimeResult> MagicBallAsync([Summary("The question you want to ask to the magic 8-ball.")] [Remainder] string Question)
        {
            string chosenAnswer = Settings.Instance.LoadedConfig.MagicBallAnswers.PickRandom();

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            IUserMessage replyMessage = await ReplyAsync(":8ball: Asking the magic 8-ball...", allowedMentions: allowedMentions, messageReference: messageReference);

            using (Context.Channel.EnterTypingState()) await Task.Delay(2000);

            await replyMessage.ModifyAsync(x => x.Content = $"The magic 8-ball has answered!\n`{chosenAnswer}`");

            return ExecutionResult.Succesful();
        }

        [Command("activity")]
        [Alias("game", "vcgame")]
        [Summary("Creates an invite for a voice channel activity!")]
        [FullDescription("Creates an activity invite for your current voice channel. Read [this](https://discordnet.dev/api/Discord.DefaultApplications.html) for" +
                         " a list of the available activities.")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.CreateInstantInvite)]
        [RequireUserPermission(GuildPermission.CreateInstantInvite)]
        [RateLimit(6, 1)]
        public async Task<RuntimeResult> CreateActivityAsync([Summary("The name of the activity you want to start.")] DefaultApplications ActivityType)
        {
            SocketGuildUser author = Context.User as SocketGuildUser;
            if (author.VoiceChannel == null)
                return ExecutionResult.FromError("You must be in a voice channel to create an activity!");

            IInviteMetadata invite = await author.VoiceChannel.CreateInviteToApplicationAsync(ActivityType);

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync($":warning: **Most activities only work if the server has a Nitro Boost level of at least 1.**\n\n" +
                $"{invite.Url}", allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("dice")]
        [Alias("roll")]
        [Summary("Roll the dice, and get a random number!")]
        [FullDescription("Roll the dice! It returns a random number between 1 and **FaceCount**. **FaceCount** must be larger than 3!")]
        [RateLimit(2, 1)]
        public async Task<RuntimeResult> RollDiceAsync([Summary("The amount of faces the die will have.")] int FaceCount = 6)
        {
            if (FaceCount < 3)
                return ExecutionResult.FromError("The dice must have at least 3 faces!");

            int chosenNumber = Random.Shared.Next(1, FaceCount + 1);

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            IUserMessage replyMessage = await ReplyAsync(":game_die: Rolling the dice...", allowedMentions: allowedMentions, messageReference: messageReference);

            using (Context.Channel.EnterTypingState()) await Task.Delay(1500);

            await replyMessage.ModifyAsync(x => x.Content = $"The dice landed on **{chosenNumber}**!");

            return ExecutionResult.Succesful();
        }

        [Command("hug")]
        [Alias("cuddle")]
        [Summary("Hug a user!")]
        [FullDescription("Hugs are good for everyone! Spread the joy with this command.")]
        [RequireContext(ContextType.Guild)]
        [RateLimit(3, 1)]
        public async Task<RuntimeResult> HugUserAsync([Summary("The user you want to hug.")] IUser User)
        {
            string chosenKaomoji = Settings.Instance.LoadedConfig.HugKaomojis.PickRandom();

            SocketGuildUser authorGuildUser = Context.Message.Author as SocketGuildUser;

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync($"Warm hugs from **{authorGuildUser.GetUsernameOrNick()}**!\n{chosenKaomoji} <@{User.Id}>", allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("pat")]
        [Summary("Pats a user!")]
        [FullDescription("Pets are ALSO good for everyone! Spread the joy with this command.")]
        [RequireContext(ContextType.Guild)]
        [RateLimit(3, 1)]
        public async Task<RuntimeResult> PatUserAsync([Summary("The user you want to pat.")] IUser User)
        {
            SocketGuildUser authorGuildUser = Context.Message.Author as SocketGuildUser;

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync($"Pats from **{authorGuildUser.GetUsernameOrNick()}**!\n(c・_・)ノ”<@{User.Id}>", allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("dox")]
        [Alias("doxx")]
        [Summary("Leak someone's (fake) IP address!")]
        [FullDescription("Dox someone! Not guaranteed to be the user's actual IP.")]
        [RequireContext(ContextType.Guild)]
        [RateLimit(3, 1)]
        public async Task<RuntimeResult> DoxUserAsync([Summary("The user you want to \"dox\".")] SocketGuildUser User)
        {
            int firstSegment = Random.Shared.Next(0, 256);
            int secondSegment = Random.Shared.Next(0, 256);
            int thirdSegment = Random.Shared.Next(0, 256);
            int fourthSegment = Random.Shared.Next(0, 256);

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync($"**{User.GetUsernameOrNick()}**'s IPv4 address: `{firstSegment}.{secondSegment}.{thirdSegment}.{fourthSegment}`", allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("kill")]
        [Alias("murder")]
        [Summary("Commit first degree murder, fuck it.")]
        [FullDescription("Commit first degree murder! Don't worry, its fictional, the police isn't after you.")]
        [RequireContext(ContextType.Guild)]
        [RateLimit(4, 1)]
        public async Task<RuntimeResult> FirstDegreeMurderAsync([Summary("The user you want to kill.")] SocketGuildUser TargetUser)
        {
            SocketGuildUser authorUser = Context.Message.Author as SocketGuildUser;

            Pronoun authorPronouns = await authorUser.GetUserPronouns();
            Pronoun targetPronouns = await TargetUser.GetUserPronouns();

            string chosenMessage = Settings.Instance.LoadedConfig.KillMessages.PickRandom();
            chosenMessage = chosenMessage.Replace("{Murderer}", $"**{authorUser.GetUsernameOrNick()}**");
            chosenMessage = chosenMessage.Replace("{mPrnSub}", authorPronouns.Subject);
            chosenMessage = chosenMessage.Replace("{mPrnObj}", authorPronouns.Object);
            chosenMessage = chosenMessage.Replace("{mPrnDepPos}", authorPronouns.DependentPossessive);
            chosenMessage = chosenMessage.Replace("{mPrnIndepPos}", authorPronouns.IndependentPossessive);
            chosenMessage = chosenMessage.Replace("{mPrnRefSing}", authorPronouns.ReflexiveSingular);
            chosenMessage = chosenMessage.Replace("{mPrnRefPlur}", authorPronouns.ReflexivePlural);

            chosenMessage = chosenMessage.Replace("{Victim}", $"**{TargetUser.GetUsernameOrNick()}**");
            chosenMessage = chosenMessage.Replace("{vPrnSub}", targetPronouns.Subject);
            chosenMessage = chosenMessage.Replace("{vPrnObj}", targetPronouns.Object);
            chosenMessage = chosenMessage.Replace("{vPrnDepPos}", targetPronouns.DependentPossessive);
            chosenMessage = chosenMessage.Replace("{vPrnIndepPos}", targetPronouns.IndependentPossessive);
            chosenMessage = chosenMessage.Replace("{vPrnRefSing}", targetPronouns.ReflexiveSingular);
            chosenMessage = chosenMessage.Replace("{vPrnRefPlur}", targetPronouns.ReflexivePlural);

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync(chosenMessage, allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("ship")]
        [Alias("loverating, shiprating")]
        [Summary("Ship 2 users together! Awww!")]
        [FullDescription("The Ship-O-Matic 5000 is here! If **SecondUser** is left empty, you will be shipped with **FirstUser**.")]
        [RequireContext(ContextType.Guild)]
        [RateLimit(5, 1)]
        public async Task<RuntimeResult> ShipUsersAsync([Summary("The first user you want to ship.")] SocketGuildUser FirstUser,
                                                        [Summary("The second user you want to ship.")] SocketGuildUser SecondUser = null)
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
            int percentage = Random.Shared.Next(0, 101);
            string percentageText = string.Empty;
            string percentageEmoji = string.Empty;

            switch (percentage)
            {
                case 0:
                    percentageText = "Incompatible!";
                    percentageEmoji = "❌";
                    break;
                case > 0 and < 25:
                    percentageText = "Awful!";
                    percentageEmoji = "💔";
                    break;
                case >= 25 and < 50:
                    percentageText = "Not Bad!";
                    percentageEmoji = "❤";
                    break;
                case >= 50 and < 75:
                    percentageText = "Decent!";
                    percentageEmoji = "💝";
                    break;
                case >= 75 and < 85:
                    percentageText = "True Love!";
                    percentageEmoji = "💖";
                    break;
                case >= 85 and < 100:
                    percentageText = "AMAZING!";
                    percentageEmoji = "💛";
                    break;
                case 100:
                    percentageText = "INSANE!";
                    percentageEmoji = "💗";
                    break;
            }

            //Split usernames into halves, then sanitize them.
            string firstUserName = FirstUser.GetUsernameOrNick();
            string secondUserName = SecondUser.GetUsernameOrNick();

            string nameFirstHalf = string.Empty;
            string nameSecondHalf = string.Empty;

            //Do the actual splitting.
            if (firstUserName.Length != 1)
                nameFirstHalf = firstUserName.Substring(0, firstUserName.Length / 2);
            if (secondUserName.Length != 1)
                nameSecondHalf = secondUserName.Substring(secondUserName.Length / 2, (int)Math.Ceiling(secondUserName.Length / 2f));

            //Sanitize splitted halves.
            nameFirstHalf = Format.Sanitize(nameFirstHalf);
            nameSecondHalf = Format.Sanitize(nameSecondHalf);

            //Sanitize usernames now, if we do it earlier, it would mess up the splitting code.
            firstUserName = Format.Sanitize(firstUserName);
            secondUserName = Format.Sanitize(secondUserName);

            //Fill up ship progress bar.
            string progressBar = string.Empty;
            for (int i = 0; i < _ShipSegments.Length; i++)
            {
                if (percentage < _ShipSegments[i])
                {
                    if (i == 0)
                        progressBar += Settings.Instance.LoadedConfig.ShipBarStartEmpty;
                    else if (i == _ShipSegments.Length - 1)
                        progressBar += Settings.Instance.LoadedConfig.ShipBarEndEmpty;
                    else
                        progressBar += Settings.Instance.LoadedConfig.ShipBarHalfEmpty;
                }
                else
                {
                    if (i == 0)
                        progressBar += Settings.Instance.LoadedConfig.ShipBarStartFull;
                    else if (i == _ShipSegments.Length - 1)
                        progressBar += Settings.Instance.LoadedConfig.ShipBarEndFull;
                    else
                        progressBar += Settings.Instance.LoadedConfig.ShipBarHalfFull;
                }
            }

            //Twemoji's repository expects filenames in big endian UTF-32, with no leading zeroes AND in PNG format.
            Encoding emojiEncoding = new UTF32Encoding(true, false);
            byte[] emojiBytes = emojiEncoding.GetBytes(percentageEmoji);
            string hexString = Convert.ToHexString(emojiBytes);
            string emojiUrl = _TWEMOJI_ASSETS + hexString.TrimStart('0').ToLower() + ".svg";

            //Image generation code is so fucking ugly.
            //Buckle up, this is a bumpy ride.

            //Create image resolution information.
            SKImageInfo imageInfo = new SKImageInfo(1024, 512);

            //Download their profile pictures and store into memory stream.
            //Also download the emoji from Twemoji's GitHub.
            using (MemoryStream firstUserAvatarStream = await DownloadToMemoryStream(FirstUser.GetGuildGlobalOrDefaultAvatar(2048)))
            using (MemoryStream secondUserAvatarStream = await DownloadToMemoryStream(SecondUser.GetGuildGlobalOrDefaultAvatar(2048)))
            //Create the actual drawing surface.
            using (SKSurface surface = SKSurface.Create(imageInfo))
            {
                string emojiData = await DownloadToString(emojiUrl);
                
                surface.Canvas.Clear(SKColors.Transparent);

                using (SKBitmap firstUserAvatar = SKBitmap.Decode(firstUserAvatarStream))
                using (SKBitmap secondUserAvatar = SKBitmap.Decode(secondUserAvatarStream))
                using (SKSvg emojiSvg = new SKSvg())
                using (SKPath loversClipPath = new SKPath())
                {
                    emojiSvg.FromSvg(emojiData);
                    
                    const int emojiSize = 96;
                    using (SKBitmap emojiBitmap = emojiSvg.Picture.ToBitmap(SKColor.Empty, emojiSize, emojiSize,
                               SKColorType.Rgba8888, SKAlphaType.Unpremul, SKColorSpace.CreateSrgb()))
                    {
                        //Add the two "Windows" to the clip path. They have their origin in the center, not the top left corner.
                        loversClipPath.AddCircle(imageInfo.Width / 4, imageInfo.Height / 2, imageInfo.Height / 2);
                        loversClipPath.AddCircle((int)(imageInfo.Width / 1.3333f), imageInfo.Height / 2, imageInfo.Height / 2);

                        //Save canvas state.
                        surface.Canvas.Save();
                        
                        //Create the target rects for the profile pictures.
                        SKRect firstUserRect = new SKRect()
                        {
                            Left = 0,
                            Top = 0,
                            Right = imageInfo.Width / 2,
                            Bottom = imageInfo.Height
                        };
                        SKRect secondUserRect = new SKRect()
                        {
                            Left = imageInfo.Width / 2,
                            Top = 0,
                            Right = imageInfo.Width,
                            Bottom = imageInfo.Height
                        };

                        //Set clip path and draw the 2 profile pictures.
                        surface.Canvas.ClipPath(loversClipPath, SKClipOperation.Intersect, true);
                        surface.Canvas.DrawBitmap(firstUserAvatar, firstUserRect);
                        surface.Canvas.DrawBitmap(secondUserAvatar, secondUserRect);

                        //Restore the canvas state, currently the only way to remove a clip path.
                        surface.Canvas.Restore();

                        //Use a custom filter with a drop shadow effect.
                        using (SKPaint emojiPaint = new SKPaint())
                        {
                            emojiPaint.IsAntialias = true;
                            emojiPaint.FilterQuality = SKFilterQuality.High;
                            emojiPaint.ImageFilter = SKImageFilter.CreateDropShadow(0, 0, 5, 5, SKColors.Black.WithAlpha(220));
                            
                            //Do some math trickery to get it centered since bitmaps have their origin in the top left corner.
                            SKRect emojiRect = new SKRect()
                            {
                                Left = imageInfo.Width / 2 - emojiSize,
                                Top = imageInfo.Height / 2 - emojiSize,
                                Right = imageInfo.Width / 2 + emojiSize,
                                Bottom = imageInfo.Height / 2 + emojiSize
                            };

                            //Draw the emoji.
                            surface.Canvas.DrawBitmap(emojiBitmap, emojiRect, emojiPaint);
                        }
                    }
                }

                //Take snapshot, encode it into PNG, store it into MemoryStream to be uploaded to Discord.
                using (SKImage surfaceSnapshot = surface.Snapshot())
                using (SKData imageData = surfaceSnapshot.Encode(SKEncodedImageFormat.Png, 100))
                using (MemoryStream finalImageStream = new MemoryStream((int)imageData.Size))
                {
                    //Save the actual image into the stream.
                    imageData.SaveTo(finalImageStream);

                    //Build the message itself.
                    //Start by creating an embed with no title and the color of the red heart emoji.
                    EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).WithTitle(string.Empty).WithColor(new Color(221, 46, 68));

                    //Tell Discord that the image will be uploaded from the local storage.
                    replyEmbed.ImageUrl = $"attachment://shipImage.png";

                    replyEmbed.Description += $":twisted_rightwards_arrows:  **Ship Name**: {nameFirstHalf}{nameSecondHalf}\n";
                    replyEmbed.Description += $"{progressBar} **{percentage}%** - {percentageEmoji} {percentageText}";

                    //Set the raw text outisde the embed.
                    string preEmbedText = ":cupid: **THE SHIP-O-MATIC 5000** :cupid:\n";
                    preEmbedText += $":small_blue_diamond: {firstUserName}\n";
                    preEmbedText += $":small_blue_diamond: {secondUserName}\n";

                    MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
                    AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

                    //Use SendFileAsync to be able to upload the stream to Discord's servers. The file name has to be the same as the one set in ImageUrl.
                    await Context.Channel.SendFileAsync(finalImageStream, "shipImage.png", preEmbedText,
                        embed: replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);
                }
            }

            return ExecutionResult.Succesful();
        }

        private async Task<MemoryStream> DownloadToMemoryStream(string Url)
        {
            byte[] rawData = await FunService.FunHttpClient.GetByteArrayAsync(Url);

            return new MemoryStream(rawData);
        }

        private async Task<string> DownloadToString(string Url)
        {
            string downloadedData = await FunService.FunHttpClient.GetStringAsync(Url);

            return downloadedData;
        }

        [Command("urban")]
        [Summary("Gets a definition from the urban dictionary!")]
        [FullDescription("Gets a definition from the urban dictionary. Click the embed's title to open the definition in your browser.")]
        [RateLimit(6, 1)]
        public async Task<RuntimeResult> UrbanAsync([Summary("The term you want to search.")] [Remainder] string Term)
        {
            UrbanSearchParams searchParameters = new()
            {
                term = Term
            };

            UrbanDefinitionList urbanDefinitions = null;
            using (Context.Channel.EnterTypingState()) urbanDefinitions = await GetUrbanDefinitionAsync(searchParameters);

            if (urbanDefinitions == null || urbanDefinitions.List.Count == 0)
                return ExecutionResult.FromError($"Urban Dictionary returned no definitions for \"{Term}\"!");

            UrbanDefinition chosenDefinition = urbanDefinitions.List.First();

            string embedDescription = $"**Definition** : *{chosenDefinition.Definition.Truncate(1024)}*\n\n";
            embedDescription += $"**Example** : {(string.IsNullOrEmpty(chosenDefinition.Example) ? "No Example" : chosenDefinition.Example)}\n\n";
            embedDescription += $"**Author** : {chosenDefinition.Author}\n";
            embedDescription += $"**Thumbs Up** : {chosenDefinition.ThumbsUp}\n";
            embedDescription += $"**Thumbs Down** : {chosenDefinition.ThumbsDown}\n";

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, Description: embedDescription);
            replyEmbed.ChangeTitle($"URBAN DEFINITION OF \"{chosenDefinition.Word}\"");
            replyEmbed.WithUrl(chosenDefinition.Permalink);

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync(null, false, replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        public async Task<UrbanDefinitionList> GetUrbanDefinitionAsync(UrbanSearchParams searchParams)
        {
            string queryString = searchParams.ToQueryString();
            string jsonReply = string.Empty;

            using (HttpResponseMessage responseMessage = await FunService.FunHttpClient.GetAsync($"https://api.urbandictionary.com/v0/define?{queryString}"))
            {
                jsonReply = await responseMessage.Content.ReadAsStringAsync();
            }

            UrbanDefinitionList definitionReply = JsonConvert.DeserializeObject<UrbanDefinitionList>(jsonReply);

            return definitionReply;
        }
    }
}