#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2022 AestheticalZ
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
using SammBot.Bot.Classes;
using SkiaSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Interactions;
using SammBot.Bot.Attributes;
using SammBot.Bot.Core;
using SammBot.Bot.Extensions;
using SammBot.Bot.Preconditions;
using SammBot.Bot.Rest.OpenWeather;
using SammBot.Bot.Rest.OpenWeather.Forecast;
using SammBot.Bot.Rest.OpenWeather.Geolocation;
using SammBot.Bot.Services;

namespace SammBot.Bot.Modules;

[PrettyName("Utilities")]
[Group("utils", "Miscellaneous utilities.")]
[ModuleEmoji("\U0001f527")]
public class UtilsModule : InteractionModuleBase<ShardedInteractionContext>
{
    public UtilsService UtilsService { get; set; }

    [SlashCommand("viewhex", "Displays a HEX color, and converts it in other formats.")]
    [DetailedDescription("Sends an image with the provided color as background, and a piece of text with the color written in the middle. " +
                         "Also converts it to RGB, CMYK, HSV and HSL.")]
    [RateLimit(3, 2)]
    public async Task<RuntimeResult> VisualizeColorHex([Summary(description: "The color you want to visualize, in hexadecimal format.")] string HexColor)
    {
        string fileName = "colorView.png";
        SKColor parsedColor;

        SKImageInfo imageInfo = new SKImageInfo(512, 512);
        using (SKSurface surface = SKSurface.Create(imageInfo))
        {
            parsedColor = SKColor.Parse(HexColor);

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
                float textY = textPosVertical + (((-paint.FontMetrics.Ascent + paint.FontMetrics.Descent) / 2) - paint.FontMetrics.Descent);

                surface.Canvas.DrawText(parsedColor.ToHexString().ToUpper(), imageInfo.Width / 2f, textY,
                    new SKFont(SKTypeface.FromFamilyName("JetBrains Mono"), 48), paint);
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

                await RespondWithFileAsync(stream, fileName, embed: replyEmbed.Build(), allowedMentions: BotGlobals.Instance.AllowOnlyUsers);
            }
        }

        return ExecutionResult.Succesful();
    }

    [SlashCommand("viewrgb", "Displays an RGB color, and converts it in other formats.")]
    [DetailedDescription("Sends an image with the provided color as background, and a piece of text with the color written in the middle. " +
                         "Also converts it to HEX, CMYK, HSV and HSL.")]
    [RateLimit(3, 2)]
    public async Task<RuntimeResult> VisualizeColorRgb([Summary(description: "The amount of red. Ranges between 0 to 255.")] byte Red, 
        [Summary(description: "The amount of green. Ranges between 0 to 255.")] byte Green,
        [Summary(description: "The amount of blue. Ranges between 0 to 255.")] byte Blue)
    {
        string fileName = "colorView.png";
        SKColor parsedColor;
        SKImageInfo imageInfo = new SKImageInfo(512, 512);
            
        using (SKSurface surface = SKSurface.Create(imageInfo))
        {
            parsedColor = new SKColor(Red, Green, Blue);
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
                float textY = textPosVertical + (((-paint.FontMetrics.Ascent + paint.FontMetrics.Descent) / 2) - paint.FontMetrics.Descent);

                surface.Canvas.DrawText(parsedColor.ToRgbString().ToUpper(), imageInfo.Width / 2f, textY,
                    new SKFont(SKTypeface.FromFamilyName("JetBrains Mono"), 42), paint);
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

                await RespondWithFileAsync(stream, fileName, embed: replyEmbed.Build(), allowedMentions: BotGlobals.Instance.AllowOnlyUsers);
            }
        }

        return ExecutionResult.Succesful();
    }

    [SlashCommand("avatar", "Gets the avatar of a user.")]
    [DetailedDescription("Gets the avatar of a user. If **User** is a server user, it will display the per-guild avatar (if they have any), and send a link to the global one in " +
                         "the embed description.")]
    [RateLimit(3, 2)]
    public async Task<RuntimeResult> GetProfilePicAsync([Summary(description: "Leave empty to get your own profile picture.")] SocketUser User = null)
    {
        SocketUser targetUser = User ?? Context.Interaction.User;
            
        EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

        replyEmbed.Title = "\U0001f464 User Profile Picture";
        replyEmbed.Color = new Color(34, 102, 153);

        replyEmbed.Description = $"This is the profile picture of {targetUser.Mention}.";

        string userAvatar = targetUser.GetAvatarUrl(size: 2048);

        if (Context.User is SocketGuildUser)
        {
            SocketGuildUser guildUser = targetUser as SocketGuildUser;

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

                await RespondAsync(null, embed: replyEmbed.Build(), allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

                return ExecutionResult.Succesful();
            }
        }

        //The user doesnt have a server-specific avatar, and doesnt have a global avatar either. Exit!
        if (userAvatar == null)
            return ExecutionResult.FromError("This user does not have an avatar!");

        replyEmbed.ImageUrl = userAvatar;

        await RespondAsync(null, embed: replyEmbed.Build(), allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    [SlashCommand("weather", "Gets the current weather for your city.")]
    [DetailedDescription("Gets the current weather forecast for your city. May not have all the information available, and the location may not be accurate.")]
    [RateLimit(3, 2)]
    public async Task<RuntimeResult> GetWeatherAsync([Summary(description: "The name of the city you want to get the weather forecast for.")] string City)
    {
        await DeferAsync();
            
        List<GeolocationLocation> retrievedLocations = await GetWeatherLocationsAsync(City);
        if (retrievedLocations.Count == 0)
            return ExecutionResult.FromError("That location does not exist.");

        GeolocationLocation finalLocation = retrievedLocations.First();
        CompleteForecast retrievedWeather = await GetCurrentWeatherAsync(finalLocation);
        Condition actualWeather = retrievedWeather.Weather[0];

        //Fucking christ. https://openweathermap.org/weather-conditions
        //Good luck.
        string conditionEmoji = actualWeather.Id switch
        {
            //2XX Thunderbolt group.
            (>= 200 and <= 202) or (>= 230 and <= 232) => "\u26C8\uFE0F",
            >= 210 and <= 221 => "\U0001f329\uFE0F",

            //3XX Drizzle group.
            >= 300 and <= 321 => "\U0001f327\uFE0F",

            //5XX Rain group.
            >= 500 and <= 501 => "\U0001f326\uFE0F",
            (>= 502 and <= 504) or (>= 520 and <= 531) => "\U0001f327\uFE0F",
            >= 511 and < 520 => "\u2744\uFE0F",

            //6XX Snow group.
            >= 600 and <= 622 => "\U0001f328\uFE0F",

            //7XX Atmosphere group.
            >= 701 and <= 711 => "\U0001f32b\uFE0F",
            >= 781 and < 782 => "\U0001f32a\uFE0F",

            //800 Clear group.
            >= 800 and < 801 => "\u2600\uFE0F",

            //8XX Clouds group.
            >= 801 and < 802 => "\U0001f324\uFE0F",
            >= 802 and < 803 => "\u26C5",
            >= 803 and < 804 => "\U0001f325\uFE0F",
            >= 804 and < 805 => "\u2601\uFE0F",

            //Unknown group id.
            _ => "\u2754"
        };

        float cloudiness = retrievedWeather.Clouds.Cloudiness;

        float temperature = retrievedWeather.Information.Temperature.RoundTo(1);
        float temperatureMax = retrievedWeather.Information.MaximumTemperature.RoundTo(1);
        float temperatureMin = retrievedWeather.Information.MinimumTemperature.RoundTo(1);
        float feelsLike = retrievedWeather.Information.FeelsLike.RoundTo(1);

        float pressure = retrievedWeather.Information.Pressure.RoundTo(2);
        float humidity = retrievedWeather.Information.Humidity;

        float windSpeed = retrievedWeather.Wind.Speed.MpsToKmh();
        float windDirection = retrievedWeather.Wind.Degrees;
        float windGust = retrievedWeather.Wind.Gust.MpsToKmh();

        long sunrise = retrievedWeather.Location.Sunrise;
        long sunset = retrievedWeather.Location.Sunset;

        string embedDescription = $"\u26A0\uFE0F Some data may be missing due to an API limitation.\n";
            
        embedDescription += $"\u26A0\uFE0F The location may be inaccurate due to an API limitation as well.\n\n";

        embedDescription += $"\U0001f4cd Location: {retrievedWeather.Name}, {retrievedWeather.Location.Country.CountryCodeToFlag()}.\n\n";

        embedDescription += $"{conditionEmoji} **{actualWeather.Description.CapitalizeFirst()}**\n";
        embedDescription += $"\u2601\uFE0F Cloudiness: **{cloudiness}**%\n\n";

        embedDescription += $"\U0001f321\uFE0F Temperature: **{temperature}**°C, **{temperature.ToFahrenheit()}**°F\n";
        embedDescription += $"\u2B06\uFE0F Temperature Max: **{temperatureMax}**°C, **{temperatureMax.ToFahrenheit()}**°F\n";
        embedDescription += $"\u2B07\uFE0F Temperature Min: **{temperatureMin}**°C, **{temperatureMin.ToFahrenheit()}**°F\n";
        embedDescription += $"\U0001f464 Feels like: **{feelsLike}**°C, **{feelsLike.ToFahrenheit()}**°F\n\n";

        embedDescription += $"\U0001f30d Atmospheric pressure: **{pressure}**hPa, **{pressure.ToPsi()}**psi\n";
        embedDescription += $"\U0001f4a7 Humidity: **{humidity}**%\n\n";

        embedDescription += $"\U0001f4a8 Wind speed: **{windSpeed}**km/h, **{windSpeed.KmhToMph()}mph**\n";
        embedDescription += $"\U0001f9ed Wind direction: **{windDirection}**°\n";
        embedDescription += $"\U0001f32c\uFE0F Wind gust: **{windGust}**km/h, **{windGust.KmhToMph()}**mph\n\n";

        embedDescription += "\u26A0\uFE0F Sunrise and sunset times are adjusted to your computer's timezone.\n";
        embedDescription += $"\U0001f305 Sunrise: <t:{sunrise}:t>\n";
        embedDescription += $"\U0001f307 Sunset: <t:{sunset}:t>\n";

        //============================EMBED BUILDING============================

        EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

        replyEmbed.Title = "\U0001f6f0\uFE0F Weather Forecast";
        replyEmbed.Color = new Color(85, 172, 238);

        replyEmbed.Description = embedDescription;

        await FollowupAsync(null, embed: replyEmbed.Build(), allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    public async Task<List<GeolocationLocation>> GetWeatherLocationsAsync(string City)
    {
        string locationReply = string.Empty;
        GeolocationParameters geolocationParameters = new GeolocationParameters()
        {
            Location = City,
            AppId = SettingsManager.Instance.LoadedConfig.OpenWeatherKey,
            Limit = 1,
        };
        string locationQuery = geolocationParameters.ToQueryString();

        using (HttpResponseMessage responseMessage = await UtilsService.WeatherClient.GetAsync($"geo/1.0/direct?{locationQuery}"))
        {
            locationReply = await responseMessage.Content.ReadAsStringAsync();
        }
        List<GeolocationLocation> retrievedLocations = JsonConvert.DeserializeObject<List<GeolocationLocation>>(locationReply);

        return retrievedLocations;
    }

    public async Task<CompleteForecast> GetCurrentWeatherAsync(GeolocationLocation Location)
    {
        string weatherReply = string.Empty;
        WeatherParameters weatherParams = new WeatherParameters()
        {
            Latitude = Location.Latitude,
            Longitude = Location.Longitude,
            AppId = SettingsManager.Instance.LoadedConfig.OpenWeatherKey,
            Units = "metric"
        };
        string weatherQuery = weatherParams.ToQueryString();

        using (HttpResponseMessage responseMessage = await UtilsService.WeatherClient.GetAsync($"data/2.5/weather?{weatherQuery}"))
        {
            weatherReply = await responseMessage.Content.ReadAsStringAsync();
        }
        CompleteForecast retrievedWeather = JsonConvert.DeserializeObject<CompleteForecast>(weatherReply);

        return retrievedWeather;
    }
}