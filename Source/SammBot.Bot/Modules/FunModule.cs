#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021-2024 Analog Feelings
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SammBot.Bot.Services;
using SammBot.Library;
using SammBot.Library.Attributes;
using SammBot.Library.Extensions;
using SammBot.Library.Models;
using SammBot.Library.Preconditions;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SammBot.Bot.Modules;

[PrettyName("Fun")]
[Group("fun", "Games and fun!")]
[ModuleEmoji("\U0001F3B2")]
public class FunModule : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly HttpService _httpService;
    private readonly SettingsService _settingsService;

    private readonly int[] _shipSegments =
    [
        10, 20, 30, 40, 50, 60, 70, 80, 90, 100
    ];

    private readonly string[] _magicBallAnswers =
    [
        "It is certain.",
        "As I see it, yes.",
        "Reply hazy, try again.",
        "Don't count on it.",
        "It is decidedly so.",
        "Most likely.",
        "Ask again later.",
        "My reply is no.",
        "Without a doubt.",
        "Outlook good.",
        "Better not tell you now.",
        "My sources say no.",
        "Yes definitely.",
        "Yes.",
        "Cannot predict now.",
        "Outlook not so good.",
        "You may rely on it.",
        "Signs point to yes.",
        "Concentrate and ask again.",
        "Very doubtful."
    ];

    private const int _EMOJI_SIZE = 96;

    public FunModule(IServiceProvider provider)
    {
        _httpService = provider.GetRequiredService<HttpService>();
        _settingsService = provider.GetRequiredService<SettingsService>();
    }

    [SlashCommand("8ball", "Ask the magic 8-ball!")]
    [DetailedDescription("Ask a question to the magic 8-ball! Not guaranteed to answer first try!")]
    [RateLimit(2, 1)]
    public async Task<RuntimeResult> MagicBallAsync
    (
        [Summary("Question", "The question you want to ask to the magic 8-ball.")]
        string question
    )
    {
        string chosenAnswer = _magicBallAnswers.PickRandom();

        await RespondAsync($":8ball: Asking the magic 8-ball...\n" +
                           $"**• Question**: {question}", allowedMentions: Constants.AllowOnlyUsers);

        await Task.Delay(2000);

        await ModifyOriginalResponseAsync(x => x.Content = $":8ball: The magic 8-ball has answered!\n" +
                                                           $"**• Question**: {question}\n" +
                                                           $"**• Answer**: {chosenAnswer}");

        return ExecutionResult.Succesful();
    }

    [SlashCommand("dice", "Roll the dice, and get a random number!")]
    [DetailedDescription("Roll the dice! It returns a random number between 1 and **FaceCount**. **FaceCount** must be larger than 3!")]
    [RateLimit(2, 1)]
    public async Task<RuntimeResult> RollDiceAsync
    (
        [Summary("FaceCount", "The amount of faces the die will have.")]
        int faceCount = 6
    )
    {
        if (faceCount < 3)
            return ExecutionResult.FromError("The die must have at least 3 faces!");

        int chosenNumber = Random.Shared.Next(1, faceCount + 1);

        await RespondAsync(":game_die: Rolling the die...", allowedMentions: Constants.AllowOnlyUsers);

        await Task.Delay(1500);

        await ModifyOriginalResponseAsync(x => x.Content = $"The die landed on **{chosenNumber}**!");

        return ExecutionResult.Succesful();
    }

    [SlashCommand("hug", "Hug a user!")]
    [DetailedDescription("Hugs are good for everyone! Spread the joy with this command.")]
    [RateLimit(3, 1)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> HugUserAsync
    (
        [Summary("User", "The user you want to hug.")]
        SocketGuildUser targetUser
    )
    {
        string chosenKaomoji = _settingsService.Settings.HugKaomojis.PickRandom();

        SocketGuildUser authorGuildUser = (Context.Interaction.User as SocketGuildUser)!;

        await RespondAsync($"Warm hugs from **{authorGuildUser.DisplayName}**!\n{chosenKaomoji} <@{targetUser.Id}>",
                           allowedMentions: Constants.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    [SlashCommand("pat", "Pats a user!")]
    [DetailedDescription("Pets are ALSO good for everyone! Spread the joy with this command.")]
    [RateLimit(3, 1)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> PatUserAsync
    (
        [Summary("User", "The user you want to pat.")]
        SocketGuildUser targetUser
    )
    {
        await RespondAsync($"Pats from **{targetUser.DisplayName}**!\n(c・_・)ノ”<@{targetUser.Id}>", allowedMentions: Constants.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    [SlashCommand("dox", "Leak someone's (fake) IP address!")]
    [DetailedDescription("Dox someone! Not guaranteed to be the user's actual IP.")]
    [RateLimit(3, 1)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> DoxUserAsync
    (
        [Summary("User", "The user you want to \"dox\".")]
        SocketGuildUser targetUser
    )
    {
        int firstSegment = Random.Shared.Next(0, 256);
        int secondSegment = Random.Shared.Next(0, 256);
        int thirdSegment = Random.Shared.Next(0, 256);
        int fourthSegment = Random.Shared.Next(0, 256);

        await RespondAsync($"**{targetUser.DisplayName}**'s IPv4 address: `{firstSegment}.{secondSegment}.{thirdSegment}.{fourthSegment}`",
                           allowedMentions: Constants.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    [SlashCommand("kill", "Commit first degree murder, fuck it.")]
    [DetailedDescription("Commit first degree murder! Don't worry, its fictional, the police isn't after you.")]
    [RateLimit(4, 1)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> FirstDegreeMurderAsync
    (
        [Summary("User", "The user you want to kill.")]
        SocketGuildUser targetUser
    )
    {
        SocketGuildUser authorUser = (Context.Interaction.User as SocketGuildUser)!;

        string chosenMessage = _settingsService.Settings.KillMessages.PickRandom();
        chosenMessage = chosenMessage.Replace("{Murderer}", $"**{authorUser.DisplayName}**");
        chosenMessage = chosenMessage.Replace("{Victim}", $"**{targetUser.DisplayName}**");

        await RespondAsync(chosenMessage, allowedMentions: Constants.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    [SlashCommand("ship", "Ship 2 users together! Awww!")]
    [DetailedDescription("The Ship-O-Matic 5000 is here! If **SecondUser** is left empty, you will be shipped with **FirstUser**. If both are empty, " +
                         "you will be shipped with a random user from the server.")]
    [RateLimit(5, 1)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(GuildPermission.UseExternalEmojis)]
    public async Task<RuntimeResult> ShipUsersAsync
    (
        [Summary("FirstUser", "The first user you want to ship.")]
        SocketGuildUser? firstUser = null,
        [Summary("SecondUser", "The second user you want to ship.")]
        SocketGuildUser? secondUser = null
    )
    {
        await DeferAsync();

        // If both users are null, ship the author with a random user.
        if (firstUser == null && secondUser == null)
        {
            if (Context.Guild.Users.Count != Context.Guild.MemberCount) await Context.Guild.DownloadUsersAsync();

            SocketGuildUser chosenUser = Context.Guild.Users.Where(x => x.Id != Context.Interaction.User.Id).ToList().PickRandom();

            secondUser = chosenUser;
            firstUser = Context.Interaction.User as SocketGuildUser;
        }
        else if (firstUser != null && secondUser == null) //If only the second user is null, ship the author with the first user.
        {
            secondUser = firstUser;
            firstUser = Context.Interaction.User as SocketGuildUser;
        }

        // Do not allow people to ship the same 2 people, thats Sweetheart from OMORI levels of weird.
        if (firstUser!.Id == secondUser!.Id)
            return ExecutionResult.FromError("You can't ship the same 2 people!");

        // Get random ship percentage and text.
        int percentage = Random.Shared.Next(0, 101);
        string percentageText = string.Empty;
        string percentageEmoji = string.Empty;

        switch (percentage)
        {
            case 0:
                percentageText = "Incompatible!";
                percentageEmoji = "\u274C";

                break;
            case < 25:
                percentageText = "Awful!";
                percentageEmoji = "\U0001f494";

                break;
            case < 50:
                percentageText = "Not Bad!";
                percentageEmoji = "\u2764\uFE0F";

                break;
            case < 75:
                percentageText = "Decent!";
                percentageEmoji = "\U0001f49d";

                break;
            case < 85:
                percentageText = "True Love!";
                percentageEmoji = "\U0001f496";

                break;
            case < 100:
                percentageText = "AMAZING!";
                percentageEmoji = "\U0001f49b";

                break;
            case 100:
                percentageText = "INSANE!";
                percentageEmoji = "\U0001f497";

                break;
        }

        // Split usernames into halves, then sanitize them.
        string firstUserName = firstUser.DisplayName;
        string secondUserName = secondUser.DisplayName;

        string nameFirstHalf = string.Empty;
        string nameSecondHalf = string.Empty;

        // Do the actual splitting.
        if (firstUserName.Length != 1)
            nameFirstHalf = firstUserName.Substring(0, firstUserName.Length / 2);
        if (secondUserName.Length != 1)
            nameSecondHalf = secondUserName.Substring(secondUserName.Length / 2, (int)Math.Ceiling(secondUserName.Length / 2f));

        // Sanitize splitted halves.
        nameFirstHalf = Format.Sanitize(nameFirstHalf);
        nameSecondHalf = Format.Sanitize(nameSecondHalf);

        // Sanitize usernames now, if we do it earlier, it would mess up the splitting code.
        firstUserName = Format.Sanitize(firstUserName);
        secondUserName = Format.Sanitize(secondUserName);

        // Fill up ship progress bar.
        string progressBar = string.Empty;
        for (int i = 0; i < _shipSegments.Length; i++)
        {
            if (percentage < _shipSegments[i])
            {
                if (i == 0)
                    progressBar += _settingsService.Settings.ShipBarStartEmpty;
                else if (i == _shipSegments.Length - 1)
                    progressBar += _settingsService.Settings.ShipBarEndEmpty;
                else
                    progressBar += _settingsService.Settings.ShipBarHalfEmpty;
            }
            else
            {
                if (i == 0)
                    progressBar += _settingsService.Settings.ShipBarStartFull;
                else if (i == _shipSegments.Length - 1)
                    progressBar += _settingsService.Settings.ShipBarEndFull;
                else
                    progressBar += _settingsService.Settings.ShipBarHalfFull;
            }
        }

        // Twemoji's repository expects filenames in big endian UTF-32, with no leading zeroes, and no tailing variant selectors.
        Encoding emojiEncoding = new UTF32Encoding(true, false);
        string variantTrimmed = percentageEmoji.TrimEnd('\uFE0F');
        string hexString = Convert.ToHexString(emojiEncoding.GetBytes(variantTrimmed));
        string emojiFilename = "./Resources/Twemoji/" + hexString.TrimStart('0').ToLower() + ".png";

        // Image generation code is so fucking ugly.
        // Buckle up, this is a bumpy ride.

        // Create image resolution information.
        SKImageInfo imageInfo = new SKImageInfo(1024, 512);

        // Download their profile pictures and store into memory stream.
        // Then, load the emoji file into a stream.
        using (MemoryStream firstUserAvatarStream = await DownloadToMemoryStream(firstUser.GetDisplayAvatarUrl(size: 2048)))
        using (MemoryStream secondUserAvatarStream = await DownloadToMemoryStream(secondUser.GetDisplayAvatarUrl(size: 2048)))
        using (FileStream emojiStream = File.Open(emojiFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (SKSurface surface = SKSurface.Create(imageInfo))
        {
            surface.Canvas.Clear(SKColors.Transparent);

            using (SKBitmap firstUserAvatar = SKBitmap.Decode(firstUserAvatarStream))
            using (SKBitmap secondUserAvatar = SKBitmap.Decode(secondUserAvatarStream))
            using (SKBitmap emojiBitmap = SKBitmap.Decode(emojiStream))
            using (SKPath loversClipPath = new SKPath())
            {
                // Add the two "Windows" to the clip path. They have their origin in the center, not the top left corner.
                loversClipPath.AddCircle(imageInfo.Width / 4f, imageInfo.Height / 2f, imageInfo.Height / 2f);
                loversClipPath.AddCircle((int)(imageInfo.Width / 1.3333f), imageInfo.Height / 2f, imageInfo.Height / 2f);

                // Save canvas state.
                surface.Canvas.Save();

                // Create the target rects for the profile pictures.
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

                // Set clip path and draw the 2 profile pictures.
                surface.Canvas.ClipPath(loversClipPath, SKClipOperation.Intersect, true);
                surface.Canvas.DrawBitmap(firstUserAvatar, firstUserRect);
                surface.Canvas.DrawBitmap(secondUserAvatar, secondUserRect);

                // Restore the canvas state, currently the only way to remove a clip path.
                surface.Canvas.Restore();

                // Use a custom filter with a drop shadow effect.
                using (SKPaint emojiPaint = new SKPaint())
                {
                    emojiPaint.IsAntialias = true;
                    emojiPaint.FilterQuality = SKFilterQuality.High;
                    emojiPaint.ImageFilter = SKImageFilter.CreateDropShadow(0, 0, 4, 4, SKColors.Black.WithAlpha(255));

                    // Do some math trickery to get it centered since bitmaps have their origin in the top left corner.
                    SKRect emojiRect = new SKRect()
                    {
                        Left = imageInfo.Width / 2 - _EMOJI_SIZE,
                        Top = imageInfo.Height / 2 - _EMOJI_SIZE,
                        Right = imageInfo.Width / 2 + _EMOJI_SIZE,
                        Bottom = imageInfo.Height / 2 + _EMOJI_SIZE
                    };

                    // Draw the emoji.
                    surface.Canvas.DrawBitmap(emojiBitmap, emojiRect, emojiPaint);
                }
            }

            // Take snapshot, encode it into PNG, store it into MemoryStream to be uploaded to Discord.
            using (SKImage surfaceSnapshot = surface.Snapshot())
            using (SKData imageData = surfaceSnapshot.Encode(SKEncodedImageFormat.Png, 100))
            using (MemoryStream finalImageStream = new MemoryStream((int)imageData.Size))
            {
                // Save the actual image into the stream.
                imageData.SaveTo(finalImageStream);

                // Build the message itself.
                // Start by creating an embed with no title and the color of the red heart emoji.
                EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context).WithTitle(string.Empty).WithColor(new Color(221, 46, 68));

                // Tell Discord that the image will be uploaded from the local storage.
                replyEmbed.ImageUrl = $"attachment://shipImage.png";

                replyEmbed.Description += $":twisted_rightwards_arrows: **Ship Name**: {nameFirstHalf}{nameSecondHalf}\n";
                replyEmbed.Description += $"{progressBar} **{percentage}%** - {percentageEmoji} {percentageText}";

                // Set the raw text outisde the embed.
                string preEmbedText = ":cupid: **THE SHIP-O-MATIC 5000** :cupid:\n";
                preEmbedText += $":small_blue_diamond: {firstUserName}\n";
                preEmbedText += $":small_blue_diamond: {secondUserName}\n";

                // Use SendFileAsync to be able to upload the stream to Discord's servers. The file name has to be the same as the one set in ImageUrl.
                await FollowupWithFileAsync(finalImageStream, "shipImage.png", preEmbedText,
                                            embed: replyEmbed.Build(), allowedMentions: Constants.AllowOnlyUsers);
            }
        }

        return ExecutionResult.Succesful();
    }

    private async Task<MemoryStream> DownloadToMemoryStream(string url)
    {
        byte[] rawData = await _httpService.Client.GetByteArrayAsync(url);

        return new MemoryStream(rawData);
    }
}