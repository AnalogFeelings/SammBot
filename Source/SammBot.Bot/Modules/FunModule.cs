#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2023 AestheticalZ
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
#endregion

using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using SkiaSharp;
using Svg.Skia;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord.Interactions;
using SammBot.Bot.Common;
using SammBot.Bot.Common.Attributes;
using SammBot.Bot.Common.Preconditions;
using SammBot.Bot.Common.Rest.UrbanDictionary;
using SammBot.Bot.Core;
using SammBot.Bot.Database;
using SammBot.Bot.Database.Models;
using SammBot.Bot.Extensions;
using SammBot.Bot.Services;

namespace SammBot.Bot.Modules;

[PrettyName("Fun")]
[Group("fun", "Games and fun!")]
[ModuleEmoji("\U0001F3B2")]
public class FunModule : InteractionModuleBase<ShardedInteractionContext>
{
    public Logger Logger { get; set; }
    public HttpService HttpService { get; set; }

    private readonly int[] _ShipSegments = new int[]
    {
        10, 20, 30, 40, 50, 60, 70, 80, 90, 100
    };

    private const string _TWEMOJI_ASSETS = "https://raw.githubusercontent.com/twitter/twemoji/ad3d3d669bb3697946577247ebb15818f09c6c91/assets/svg/";

    [SlashCommand("8ball", "Ask the magic 8-ball!")]
    [DetailedDescription("Ask a question to the magic 8-ball! Not guaranteed to answer first try!")]
    [RateLimit(2, 1)]
    public async Task<RuntimeResult> MagicBallAsync([Summary(description: "The question you want to ask to the magic 8-ball.")] string Question)
    {
        string chosenAnswer = SettingsManager.Instance.LoadedConfig.MagicBallAnswers.PickRandom();

        await RespondAsync($":8ball: Asking the magic 8-ball...\n" +
                           $"**• Question**: {Question}", allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

        await Task.Delay(2000);

        await ModifyOriginalResponseAsync(x => x.Content = $":8ball: The magic 8-ball has answered!\n" +
                                                           $"**• Question**: {Question}\n" +
                                                           $"**• Answer**: {chosenAnswer}");
            
        return ExecutionResult.Succesful();
    }

    [SlashCommand("activity", "Creates an invite for a voice channel activity!")]
    [DetailedDescription("Creates an activity invite for your current voice channel.")]
    [RateLimit(6, 1)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(GuildPermission.CreateInstantInvite)]
    [RequireUserPermission(GuildPermission.CreateInstantInvite)]
    public async Task<RuntimeResult> CreateActivityAsync([Summary(description: "The name of the activity you want to start.")] DefaultApplications ActivityType)
    {
        SocketGuildUser author = (Context.User as SocketGuildUser)!;
        
        if (author.VoiceChannel == null)
            return ExecutionResult.FromError("You must be in a voice channel to create an activity!");
        
        IInviteMetadata invite = await author.VoiceChannel.CreateInviteToApplicationAsync(ActivityType);
        
        await RespondAsync($":warning: **Most activities only work if the server has a Nitro Boost level of at least 1.**\n\n" +
                           $"{invite.Url}", allowedMentions: BotGlobals.Instance.AllowOnlyUsers);
        
        return ExecutionResult.Succesful();
    }
        
    [SlashCommand("dice", "Roll the dice, and get a random number!")]
    [DetailedDescription("Roll the dice! It returns a random number between 1 and **FaceCount**. **FaceCount** must be larger than 3!")]
    [RateLimit(2, 1)]
    public async Task<RuntimeResult> RollDiceAsync([Summary(description: "The amount of faces the die will have.")] int FaceCount = 6)
    {
        if (FaceCount < 3)
            return ExecutionResult.FromError("The dice must have at least 3 faces!");
        
        int chosenNumber = Random.Shared.Next(1, FaceCount + 1);
        
        await RespondAsync(":game_die: Rolling the dice...", allowedMentions: BotGlobals.Instance.AllowOnlyUsers);
        
        await Task.Delay(1500);
        
        await ModifyOriginalResponseAsync(x => x.Content = $"The dice landed on **{chosenNumber}**!");
        
        return ExecutionResult.Succesful();
    }
        
    [SlashCommand("hug", "Hug a user!")]
    [DetailedDescription("Hugs are good for everyone! Spread the joy with this command.")]
    [RateLimit(3, 1)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> HugUserAsync([Summary(description: "The user you want to hug.")] IUser User)
    {
        string chosenKaomoji = SettingsManager.Instance.LoadedConfig.HugKaomojis.PickRandom();
        
        SocketGuildUser authorGuildUser = (Context.Interaction.User as SocketGuildUser)!;
        
        await RespondAsync($"Warm hugs from **{authorGuildUser.GetUsernameOrNick()}**!\n{chosenKaomoji} <@{User.Id}>",
            allowedMentions: BotGlobals.Instance.AllowOnlyUsers);
        
        return ExecutionResult.Succesful();
    }
        
    [SlashCommand("pat", "Pats a user!")]
    [DetailedDescription("Pets are ALSO good for everyone! Spread the joy with this command.")]
    [RateLimit(3, 1)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> PatUserAsync([Summary(description: "The user you want to pat.")] IUser User)
    {
        SocketGuildUser authorGuildUser = (Context.Interaction.User as SocketGuildUser)!;
        
        await RespondAsync($"Pats from **{authorGuildUser.GetUsernameOrNick()}**!\n(c・_・)ノ”<@{User.Id}>", allowedMentions: BotGlobals.Instance.AllowOnlyUsers);
        
        return ExecutionResult.Succesful();
    }
        
    [SlashCommand("dox", "Leak someone's (fake) IP address!")]
    [DetailedDescription("Dox someone! Not guaranteed to be the user's actual IP.")]
    [RateLimit(3, 1)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> DoxUserAsync([Summary(description: "The user you want to \"dox\".")] SocketGuildUser User)
    {
        int firstSegment = Random.Shared.Next(0, 256);
        int secondSegment = Random.Shared.Next(0, 256);
        int thirdSegment = Random.Shared.Next(0, 256);
        int fourthSegment = Random.Shared.Next(0, 256);
        
        await RespondAsync($"**{User.GetUsernameOrNick()}**'s IPv4 address: `{firstSegment}.{secondSegment}.{thirdSegment}.{fourthSegment}`",
            allowedMentions: BotGlobals.Instance.AllowOnlyUsers);
        
        return ExecutionResult.Succesful();
    }
        
    [SlashCommand("kill", "Commit first degree murder, fuck it.")]
    [DetailedDescription("Commit first degree murder! Don't worry, its fictional, the police isn't after you.")]
    [RateLimit(4, 1)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> FirstDegreeMurderAsync([Summary(description: "The user you want to kill.")] SocketGuildUser TargetUser)
    {
        SocketGuildUser authorUser = (Context.Interaction.User as SocketGuildUser)!;
        
        Pronoun authorPronouns = await authorUser.GetUserPronouns();
        Pronoun targetPronouns = await TargetUser.GetUserPronouns();
        
        string chosenMessage = SettingsManager.Instance.LoadedConfig.KillMessages.PickRandom();
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
        
        await RespondAsync(chosenMessage, allowedMentions: BotGlobals.Instance.AllowOnlyUsers);
        
        return ExecutionResult.Succesful();
    }
        
    [SlashCommand("ship", "Ship 2 users together! Awww!")]
    [DetailedDescription("The Ship-O-Matic 5000 is here! If **SecondUser** is left empty, you will be shipped with **FirstUser**. If both are empty, " +
                         "you will be shipped with a random user from the server.")]
    [RateLimit(5, 1)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(GuildPermission.UseExternalEmojis)]
    public async Task<RuntimeResult> ShipUsersAsync([Summary(description: "The first user you want to ship.")] SocketGuildUser? FirstUser = null,
        [Summary(description: "The second user you want to ship.")] SocketGuildUser? SecondUser = null)
    {
        //If both users are null, ship the author with a random user.
        if(FirstUser == null && SecondUser == null)
        {
            if(Context.Guild.Users.Count != Context.Guild.MemberCount) await Context.Guild.DownloadUsersAsync();
        
            SocketGuildUser chosenUser = Context.Guild.Users.Where(x => x.Id != Context.Interaction.User.Id).ToList().PickRandom();
        
            SecondUser = chosenUser;
            FirstUser = Context.Interaction.User as SocketGuildUser;
        }
        else if (FirstUser != null && SecondUser == null) //If only the second user is null, ship the author with the first user.
        {
            SecondUser = FirstUser;
            FirstUser = Context.Interaction.User as SocketGuildUser;
        }
        
        //Do not allow people to ship the same 2 people, thats Sweetheart from OMORI levels of weird.
        if (FirstUser!.Id == SecondUser!.Id)
            return ExecutionResult.FromError("You can't ship the same 2 people!");

        await DeferAsync();
        
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
                    progressBar += SettingsManager.Instance.LoadedConfig.ShipBarStartEmpty;
                else if (i == _ShipSegments.Length - 1)
                    progressBar += SettingsManager.Instance.LoadedConfig.ShipBarEndEmpty;
                else
                    progressBar += SettingsManager.Instance.LoadedConfig.ShipBarHalfEmpty;
            }
            else
            {
                if (i == 0)
                    progressBar += SettingsManager.Instance.LoadedConfig.ShipBarStartFull;
                else if (i == _ShipSegments.Length - 1)
                    progressBar += SettingsManager.Instance.LoadedConfig.ShipBarEndFull;
                else
                    progressBar += SettingsManager.Instance.LoadedConfig.ShipBarHalfFull;
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
                using (SKBitmap emojiBitmap = emojiSvg.Picture!.ToBitmap(SKColor.Empty, emojiSize, emojiSize,
                           SKColorType.Rgba8888, SKAlphaType.Unpremul, SKColorSpace.CreateSrgb())!)
                {
                    //Add the two "Windows" to the clip path. They have their origin in the center, not the top left corner.
                    loversClipPath.AddCircle(imageInfo.Width / 4f, imageInfo.Height / 2f, imageInfo.Height / 2f);
                    loversClipPath.AddCircle((int)(imageInfo.Width / 1.3333f), imageInfo.Height / 2f, imageInfo.Height / 2f);
        
                    //Save canvas state.
                    surface.Canvas.Save();
                        
                    //Create the target rects for the profile pictures.
                    SKRect firstUserRect = new SKRect()
                    {
                        Left = 0,
                        Top = 0,
                        Right = imageInfo.Width / 2f,
                        Bottom = imageInfo.Height
                    };
                    SKRect secondUserRect = new SKRect()
                    {
                        Left = imageInfo.Width / 2f,
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
        
                //Use SendFileAsync to be able to upload the stream to Discord's servers. The file name has to be the same as the one set in ImageUrl.
                await FollowupWithFileAsync(finalImageStream, "shipImage.png", preEmbedText,
                    embed: replyEmbed.Build(), allowedMentions: BotGlobals.Instance.AllowOnlyUsers);
            }
        }
        
        return ExecutionResult.Succesful();
    }
        
    private async Task<MemoryStream> DownloadToMemoryStream(string Url)
    {
        byte[] rawData = await HttpService.Client.GetByteArrayAsync(Url);
        
        return new MemoryStream(rawData);
    }
        
    private async Task<string> DownloadToString(string Url)
    {
        string downloadedData = await HttpService.Client.GetStringAsync(Url);
        
        return downloadedData;
    }
        
    [SlashCommand("urban", "Gets a definition from the urban dictionary!")]
    [DetailedDescription("Gets a definition from the urban dictionary. Click the embed's title to open the definition in your browser.")]
    [RateLimit(6, 1)]
    public async Task<RuntimeResult> UrbanAsync([Summary(description: "The term you want to search.")] string Term)
    {
        UrbanSearchParameters searchParameters = new()
        {
            Term = Term
        };

        await DeferAsync();
        
        UrbanDefinitionList? urbanDefinitions = await HttpService.GetObjectFromJsonAsync<UrbanDefinitionList>("https://api.urbandictionary.com/v0/define", searchParameters);
        
        if (urbanDefinitions == null || urbanDefinitions.List.Count == 0)
            return ExecutionResult.FromError($"Urban Dictionary returned no definitions for \"{Term}\"!");
        
        UrbanDefinition chosenDefinition = urbanDefinitions.List.PickRandom();
        
        chosenDefinition.Definition = chosenDefinition.Definition.Replace("[", "");
        chosenDefinition.Definition = chosenDefinition.Definition.Replace("]", "");
        
        if (!string.IsNullOrEmpty(chosenDefinition.Example))
        {
            chosenDefinition.Example = chosenDefinition.Example.Replace("[", "");
            chosenDefinition.Example = chosenDefinition.Example.Replace("]", "");
        }
        
        string embedDescription = $"\U0001f4c4 **Definition** : *{chosenDefinition.Definition.Truncate(1024)}*\n\n";
        embedDescription += $"\U0001f4dd **Example** : {(string.IsNullOrEmpty(chosenDefinition.Example) ? "No Example" : chosenDefinition.Example)}\n\n";
        embedDescription += $"\U0001f464 **Author** : {chosenDefinition.Author}\n";
        embedDescription += $"\U0001f44d **Thumbs Up** : {chosenDefinition.ThumbsUp}\n";
        embedDescription += $"\U0001f44e **Thumbs Down** : {chosenDefinition.ThumbsDown}\n";
        
        EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, Description: embedDescription);
            
        replyEmbed.Title = $"\U0001f4d6 Urban Definition Of \"{chosenDefinition.Word}\"";
        replyEmbed.Color = new Color(34, 102, 153);
            
        replyEmbed.WithUrl(chosenDefinition.Permalink);
        
        await FollowupAsync(embed: replyEmbed.Build(), allowedMentions: BotGlobals.Instance.AllowOnlyUsers);
        
        return ExecutionResult.Succesful();
    }
}