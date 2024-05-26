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
using System.Threading.Tasks;

namespace SammBot.Bot.Modules;

[PrettyName("Utilities")]
[Group("utils", "Miscellaneous utilities.")]
[ModuleEmoji("\U0001f527")]
public class UtilsModule : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly HttpService _httpService;
    private readonly SettingsService _settingsService;

    public UtilsModule(IServiceProvider provider)
    {
        _httpService = provider.GetRequiredService<HttpService>();
        _settingsService = provider.GetRequiredService<SettingsService>();
    }

    [SlashCommand("viewhex", "Displays a HEX color, and converts it in other formats.")]
    [DetailedDescription("Sends an image with the provided color as background, and a piece of text with the color written in the middle. " +
                         "Also converts it to RGB, CMYK, HSV and HSL.")]
    [RateLimit(3, 2)]
    public async Task<RuntimeResult> VisualizeColorHex
    (
        [Summary("Color", "The color you want to view, in hex format.")]
        string hexColor
    )
    {
        const string fileName = "colorView.png";

        SKImageInfo imageInfo = new SKImageInfo(512, 512);
        using (SKSurface surface = SKSurface.Create(imageInfo))
        {
            SKColor parsedColor = SKColor.Parse(hexColor);

            surface.Canvas.Clear(parsedColor);

            using (SKPaint paint = new SKPaint())
            {
                paint.TextSize = 48;
                paint.IsAntialias = true;
                paint.TextAlign = SKTextAlign.Center;

                //Use black or white depending on background color.
                if ((parsedColor.Red * 0.299f + parsedColor.Green * 0.587f + parsedColor.Blue * 0.114f) > 149)
                    paint.Color = SKColors.Black;
                else
                    paint.Color = SKColors.White;

                //thanks stack overflow lol
                int textPosVertical = imageInfo.Height / 2;
                float textX = imageInfo.Width / 2f;
                float textY = textPosVertical + (((-paint.FontMetrics.Ascent + paint.FontMetrics.Descent) / 2) - paint.FontMetrics.Descent);

                SKFont textFont = new SKFont(SKTypeface.FromFamilyName("JetBrains Mono"), 48);

                surface.Canvas.DrawText(parsedColor.ToHexString(), textX, textY, textFont, paint);
            }

            using (SKImage image = surface.Snapshot())
            using (SKData imageData = image.Encode(SKEncodedImageFormat.Png, 100))
            using (MemoryStream stream = new MemoryStream((int)imageData.Size))
            {
                imageData.SaveTo(stream);

                EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

                replyEmbed.Title = $"\U0001f3a8 Visualization Of {parsedColor.ToHexString()}";

                replyEmbed.ImageUrl = $"attachment://{fileName}";
                replyEmbed.Color = new Color(parsedColor.Red, parsedColor.Green, parsedColor.Blue);

                replyEmbed.Description += $"• **RGB**: {parsedColor.ToRgbString()}\n";
                replyEmbed.Description += $"• **CMYK**: {parsedColor.ToCmykString()}\n";
                replyEmbed.Description += $"• **HSV**: {parsedColor.ToHsvString()}\n";
                replyEmbed.Description += $"• **HSL**: {parsedColor.ToHslString()}\n";

                await RespondWithFileAsync(stream, fileName, embed: replyEmbed.Build(), allowedMentions: Constants.AllowOnlyUsers);
            }
        }

        return ExecutionResult.Succesful();
    }

    [SlashCommand("viewrgb", "Displays an RGB color, and converts it in other formats.")]
    [DetailedDescription("Sends an image with the provided color as background, and a piece of text with the color written in the middle. " +
                         "Also converts it to HEX, CMYK, HSV and HSL.")]
    [RateLimit(3, 2)]
    public async Task<RuntimeResult> VisualizeColorRgb
    (
        [Summary("Red", "The amount of red. Ranges between 0 to 255.")]
        byte red,
        [Summary("Green", "The amount of green. Ranges between 0 to 255.")]
        byte green,
        [Summary("Blue", "The amount of blue. Ranges between 0 to 255.")]
        byte blue
    )
    {
        const string fileName = "colorView.png";

        SKImageInfo imageInfo = new SKImageInfo(512, 512);
        using (SKSurface surface = SKSurface.Create(imageInfo))
        {
            SKColor parsedColor = new SKColor(red, green, blue);
            surface.Canvas.Clear(parsedColor);

            using (SKPaint paint = new SKPaint())
            {
                paint.TextSize = 42;
                paint.IsAntialias = true;
                paint.TextAlign = SKTextAlign.Center;

                //Use black or white depending on background color.
                if ((parsedColor.Red * 0.299f + parsedColor.Green * 0.587f + parsedColor.Blue * 0.114f) > 149)
                    paint.Color = SKColors.Black;
                else
                    paint.Color = SKColors.White;

                //thanks stack overflow lol
                int textPosVertical = imageInfo.Height / 2;
                float textX = imageInfo.Width / 2f;
                float textY = textPosVertical + (((-paint.FontMetrics.Ascent + paint.FontMetrics.Descent) / 2) - paint.FontMetrics.Descent);

                SKFont textFont = new SKFont(SKTypeface.FromFamilyName("JetBrains Mono"), 42);

                surface.Canvas.DrawText(parsedColor.ToRgbString(), textX, textY, textFont, paint);
            }

            using (SKImage image = surface.Snapshot())
            using (SKData imageData = image.Encode(SKEncodedImageFormat.Png, 100))
            using (MemoryStream stream = new MemoryStream((int)imageData.Size))
            {
                imageData.SaveTo(stream);

                EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

                replyEmbed.Title = $"\U0001f3a8 Visualization Of {parsedColor.ToRgbString()}";

                replyEmbed.ImageUrl = $"attachment://{fileName}";
                replyEmbed.Color = new Color(parsedColor.Red, parsedColor.Green, parsedColor.Blue);

                replyEmbed.Description += $"• **HEX**: {parsedColor.ToHexString()}\n";
                replyEmbed.Description += $"• **CMYK**: {parsedColor.ToCmykString()}\n";
                replyEmbed.Description += $"• **HSV**: {parsedColor.ToHsvString()}\n";
                replyEmbed.Description += $"• **HSL**: {parsedColor.ToHslString()}\n";

                await RespondWithFileAsync(stream, fileName, embed: replyEmbed.Build(), allowedMentions: Constants.AllowOnlyUsers);
            }
        }

        return ExecutionResult.Succesful();
    }

    [SlashCommand("avatar", "Gets the avatar of a user.")]
    [DetailedDescription("Gets the avatar of a user. If **User** is a server user, it will display the per-guild avatar " +
                         "(if they have any), and send a link to the global one in the embed description.")]
    [RateLimit(3, 2)]
    public async Task<RuntimeResult> GetProfilePicAsync
    (
        [Summary("User", "Leave empty to get your own profile picture.")]
        SocketUser? user = null
    )
    {
        SocketUser targetUser = user ?? Context.Interaction.User;

        EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

        replyEmbed.Title = "\U0001f464 User Profile Picture";
        replyEmbed.Color = new Color(34, 102, 153);

        replyEmbed.Description = $"This is the profile picture of {targetUser.Mention}.";

        string userAvatar = targetUser.GetAvatarUrl(size: 2048);

        if (targetUser is SocketGuildUser)
        {
            SocketGuildUser guildUser = (targetUser as SocketGuildUser)!;

            string serverAvatar = guildUser.GetGuildAvatarUrl(size: 2048);
            if (serverAvatar != null)
            {
                //The user doesnt have a global avatar? Thats fine, we still have the server-specific one.
                if (userAvatar != null)
                {
                    replyEmbed.Description = $"This is the server profile picture of {guildUser.Mention}.\n" +
                                             $"[Global Profile Picture]({userAvatar})";
                }

                replyEmbed.ImageUrl = serverAvatar;

                await RespondAsync(embed: replyEmbed.Build(), allowedMentions: Constants.AllowOnlyUsers);

                return ExecutionResult.Succesful();
            }
        }

        //The user doesnt have a server-specific avatar, and doesnt have a global avatar either. Exit!
        if (userAvatar == null)
            return ExecutionResult.FromError("This user does not have an avatar!");

        replyEmbed.ImageUrl = userAvatar;

        await RespondAsync(embed: replyEmbed.Build(), allowedMentions: Constants.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }
}