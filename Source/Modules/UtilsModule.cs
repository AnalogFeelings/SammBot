using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using SkiaSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Utilities")]
    [Group("utils")]
    [Summary("Miscellaneous utilities.")]
    [ModuleEmoji("\U0001f527")]
    public class UtilsModule : ModuleBase<SocketCommandContext>
    {
        public UtilsService UtilsService { get; set; }

        [Command("viewhex")]
        [Alias("hex")]
        [Summary("Displays a HEX color, and converts it in other formats.")]
        [FullDescription("Sends an image with the provided color as background, and a piece of text with the color written in the middle. " +
            "Also converts it to RGB, CMYK, HSV and HSL.")]
        [RateLimit(3, 2)]
        public async Task<RuntimeResult> VisualizeColorHex([Summary("The color you want to visualize, in hexadecimal format.")] [Remainder] string HexColor)
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

                    try
                    {
                        EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context)
                            .ChangeTitle($"Color Visualization Of {parsedColor.ToHexString()}");

                        replyEmbed.ImageUrl = $"attachment://{fileName}";
                        replyEmbed.Color = new Color(parsedColor.Red, parsedColor.Green, parsedColor.Blue);

                        replyEmbed.Description += $"**__RGB__**: {parsedColor.ToRgbString()}\n";
                        replyEmbed.Description += $"**__CMYK__**: {parsedColor.ToCmykString()}\n";
                        replyEmbed.Description += $"**__HSV__**: {parsedColor.ToHsvString()}\n";
                        replyEmbed.Description += $"**__HSL__**: {parsedColor.ToHslString()}\n";

                        MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
                        AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

                        await Context.Channel.SendFileAsync(stream, fileName, embed: replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);
                    }
                    finally
                    {
                        File.Delete(fileName);
                    }
                }
            }

            return ExecutionResult.Succesful();
        }

        [Command("viewrgb")]
        [Alias("rgb")]
        [Summary("Displays an RGB color, and converts it in other formats.")]
        [FullDescription("Sends an image with the provided color as background, and a piece of text with the color written in the middle. " +
            "Also converts it to HEX, CMYK, HSV and HSL.")]
        [RateLimit(3, 2)]
        public async Task<RuntimeResult> VisualizeColorRgb([Summary("The amount of red. Ranges between 0 to 255.")] byte Red, 
                                                           [Summary("The amount of green. Ranges between 0 to 255.")] byte Green,
                                                           [Summary("The amount of blue. Ranges between 0 to 255.")] byte Blue)
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

                    surface.Canvas.DrawText(parsedColor.ToRgbString().ToUpper(), imageInfo.Width / 2f, textY,
                        new SKFont(SKTypeface.FromFamilyName("JetBrains Mono"), 48), paint);
                }

                using (SKImage image = surface.Snapshot())
                using (SKData imageData = image.Encode(SKEncodedImageFormat.Png, 100))
                using (MemoryStream stream = new MemoryStream((int)imageData.Size))
                {
                    imageData.SaveTo(stream);

                    try
                    {
                        EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context)
                            .ChangeTitle($"Color Visualization Of {parsedColor.ToRgbString()}");

                        replyEmbed.ImageUrl = $"attachment://{fileName}";
                        replyEmbed.Color = new Color(parsedColor.Red, parsedColor.Green, parsedColor.Blue);

                        replyEmbed.Description += $"**__HEX__**: {parsedColor.ToHexString()}\n";
                        replyEmbed.Description += $"**__CMYK__**: {parsedColor.ToCmykString()}\n";
                        replyEmbed.Description += $"**__HSV__**: {parsedColor.ToHsvString()}\n";
                        replyEmbed.Description += $"**__HSL__**: {parsedColor.ToHslString()}\n";

                        MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
                        AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

                        await Context.Channel.SendFileAsync(stream, fileName, embed: replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);
                    }
                    finally
                    {
                        File.Delete(fileName);
                    }
                }
            }

            return ExecutionResult.Succesful();
        }

        [Command("avatar")]
        [Alias("pfp", "pic", "userpic")]
        [Summary("Gets the avatar of a user.")]
        [FullDescription("Gets the avatar of a user. If **User** is a server user, it will display the per-guild avatar (if they have any), and send a link to the global one in " +
            "the embed description.")]
        [RateLimit(3, 2)]
        public async Task<RuntimeResult> GetProfilePicAsync([Summary("The user you want to get the profile picture from.")] IUser User)
        {
            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

            replyEmbed.ChangeTitle($"{User.Username}'s Profile Picture");

            string userAvatar = User.GetAvatarUrl(size: 2048);

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

            if (Context.User is SocketGuildUser)
            {
                SocketGuildUser targetUser = User as SocketGuildUser;

                string serverAvatar = targetUser.GetGuildAvatarUrl(size: 2048);
                if (serverAvatar != null)
                {
                    //The user doesnt have a global avatar? Thats fine, we still have the server-specific one.
                    if (userAvatar != null)
                    {
                        replyEmbed.Description = $"[Global Profile Picture]({userAvatar})";
                    }

                    replyEmbed.ImageUrl = serverAvatar;

                    await ReplyAsync(null, false, replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);

                    return ExecutionResult.Succesful();
                }
            }

            //The user doesnt have a server-specific avatar, and doesnt have a global avatar either. Exit!
            if (userAvatar == null)
                return ExecutionResult.FromError("This user does not have an avatar!");

            replyEmbed.ImageUrl = userAvatar;

            await ReplyAsync(null, false, replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("weather")]
        [Summary("Gets the current weather for your city.")]
        [FullDescription("Gets the current weather forecast for your city. May not have all the information available, and the location may not be accurate.")]
        [RateLimit(3, 2)]
        public async Task<RuntimeResult> GetWeatherAsync([Summary("The name of the city you want to get the weather forecast for.")] [Remainder] string City)
        {
            List<Location> retrievedLocations = await GetWeatherLocationsAsync(City);
            if (retrievedLocations.Count == 0)
                return ExecutionResult.FromError("That location does not exist.");

            Location finalLocation = retrievedLocations.First();
            CurrentWeather retrievedWeather = await GetCurrentWeatherAsync(finalLocation);
            Weather actualWeather = retrievedWeather.Weather[0];

            //Fucking christ. https://openweathermap.org/weather-conditions
            //Good luck.
            string conditionEmoji = actualWeather.Id switch
            {
                //2XX Thunderbolt group.
                (>= 200 and <= 202) or (>= 230 and <= 232) => "⛈",
                >= 210 and <= 221 => "🌩",

                //3XX Drizzle group.
                >= 300 and <= 321 => "🌧",

                //5XX Rain group.
                >= 500 and <= 501 => "🌦",
                (>= 502 and <= 504) or (>= 520 and <= 531) => "🌧",
                >= 511 and < 520 => "❄️",

                //6XX Snow group.
                >= 600 and <= 622 => "🌨",

                //7XX Atmosphere group.
                >= 701 and <= 711 => "🌫",
                >= 781 and < 782 => "🌪",

                //800 Clear group.
                >= 800 and < 801 => "☀",

                //8XX Clouds group.
                >= 801 and < 802 => "🌤",
                >= 802 and < 803 => "⛅",
                >= 803 and < 804 => "🌥",
                >= 804 and < 805 => "☁",

                //Unknown group id.
                _ => "❔"
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

            long sunrise = retrievedWeather.System.Sunrise;
            long sunset = retrievedWeather.System.Sunset;

            string embedDescription = $"⚠️ Some data may be missing due to an API limitation.\n\n";

            embedDescription += $"{conditionEmoji} **{actualWeather.Description.CapitalizeFirst()}**\n";
            embedDescription += $"☁️ Cloudiness: **{cloudiness}**%\n\n";

            embedDescription += $"🌡 Temperature: **{temperature}**°C, **{temperature.ToFahrenheit()}**°F\n";
            embedDescription += $"⬆ Temperature Max: **{temperatureMax}**°C, **{temperatureMax.ToFahrenheit()}**°F\n";
            embedDescription += $"⬇ Temperature Min: **{temperatureMin}**°C, **{temperatureMin.ToFahrenheit()}**°F\n";
            embedDescription += $"👤 Feels like: **{feelsLike}**°C, **{feelsLike.ToFahrenheit()}**°F\n\n";

            embedDescription += $"🌍 Atmospheric pressure: **{pressure}**hPa, **{pressure.ToPsi()}**psi\n";
            embedDescription += $"💧 Humidity: **{humidity}**%\n\n";

            embedDescription += $"💨 Wind speed: **{windSpeed}**km/h, **{windSpeed.KmhToMph()}mph**\n";
            embedDescription += $"🧭 Wind direction: **{windDirection}**°\n";
            embedDescription += $"🌬 Wind gust: **{windGust}**km/h, **{windGust.KmhToMph()}**mph\n\n";

            embedDescription += "⚠️ Sunrise and sunset times are adjusted to your computer's timezone.\n";
            embedDescription += $"🌅 Sunrise: <t:{sunrise}:t>\n";
            embedDescription += $"🌇 Sunset: <t:{sunset}:t>\n";

            //============================EMBED BUILDING============================

            EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context)
                .ChangeTitle($"Weather for {retrievedWeather.Name}, {retrievedWeather.System.Country.CountryCodeToFlag()}");

            replyEmbed.Description = embedDescription;

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync(null, false, replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        public async Task<List<Location>> GetWeatherLocationsAsync(string City)
        {
            string locationReply = string.Empty;
            GeolocationParams geolocationParameters = new GeolocationParams()
            {
                q = City,
                appid = Settings.Instance.LoadedConfig.OpenWeatherKey,
                limit = 1,
            };
            string locationQuery = geolocationParameters.ToQueryString();

            using (HttpResponseMessage responseMessage = await UtilsService.WeatherClient.GetAsync($"geo/1.0/direct?{locationQuery}"))
            {
                locationReply = await responseMessage.Content.ReadAsStringAsync();
            }
            List<Location> retrievedLocations = JsonConvert.DeserializeObject<List<Location>>(locationReply);

            return retrievedLocations;
        }

        public async Task<CurrentWeather> GetCurrentWeatherAsync(Location Location)
        {
            string weatherReply = string.Empty;
            WeatherParams weatherParams = new WeatherParams()
            {
                lat = Location.Latitude,
                lon = Location.Longitude,
                appid = Settings.Instance.LoadedConfig.OpenWeatherKey,
                units = "metric"
            };
            string weatherQuery = weatherParams.ToQueryString();

            using (HttpResponseMessage responseMessage = await UtilsService.WeatherClient.GetAsync($"data/2.5/weather?{weatherQuery}"))
            {
                weatherReply = await responseMessage.Content.ReadAsStringAsync();
            }
            CurrentWeather retrievedWeather = JsonConvert.DeserializeObject<CurrentWeather>(weatherReply);

            return retrievedWeather;
        }
    }
}
