﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using SammBotNET.Classes.ClassExtensions;
using SammBotNET.Extensions.ClassExtensions;
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
	[ModuleEmoji("🔧")]
	public class UtilsModule : ModuleBase<SocketCommandContext>
	{
		public UtilsService UtilsService { get; set; }

		[Command("viewhex")]
		[Alias("hex")]
		[Summary("Displays a HEX color, and converts it in other formats.")]
		[FullDescription("Sends an image with the provided color as background, and a piece of text with the color written in the middle. " +
			"Also converts it to RGB, CMYK, HSV and HSL.")]
		public async Task<RuntimeResult> VisualizeColorHex([Remainder] string HexColor)
		{
			string Filename = "colorView.png";
			SKColor ParsedColor;

			SKImageInfo ImageInfo = new SKImageInfo(512, 512);
			using (SKSurface Surface = SKSurface.Create(ImageInfo))
			{
				ParsedColor = SKColor.Parse(HexColor);

				Surface.Canvas.Clear(ParsedColor);

				using (SKPaint Paint = new SKPaint())
				{
					Paint.TextSize = 48;
					Paint.IsAntialias = true;
					Paint.TextAlign = SKTextAlign.Center;

					//Use black or white depending on background color.
					if ((ParsedColor.Red * 0.299f + ParsedColor.Green * 0.587f + ParsedColor.Blue * 0.114f) > 149)
						Paint.Color = SKColors.Black;
					else
						Paint.Color = SKColors.White;

					//thanks stack overflow lol
					int TextPosVertical = ImageInfo.Height / 2;
					float TextY = TextPosVertical + (((-Paint.FontMetrics.Ascent + Paint.FontMetrics.Descent) / 2) - Paint.FontMetrics.Descent);

					Surface.Canvas.DrawText(ParsedColor.ToHexString().ToUpper(), ImageInfo.Width / 2f, TextY,
						new SKFont(SKTypeface.FromFamilyName("JetBrains Mono"), 48), Paint);
				}

				using (SKImage Image = Surface.Snapshot())
				using (SKData Data = Image.Encode(SKEncodedImageFormat.Png, 100))
				using (MemoryStream Stream = new MemoryStream((int)Data.Size))
				{
					Data.SaveTo(Stream);

					try
					{
						EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context)
							.ChangeTitle($"Color Visualization Of {ParsedColor.ToHexString()}");

						ReplyEmbed.ImageUrl = $"attachment://{Filename}";
						ReplyEmbed.Color = new Color(ParsedColor.Red, ParsedColor.Green, ParsedColor.Blue);

						ReplyEmbed.Description += $"**__RGB__**: {ParsedColor.ToRgbString()}\n";
						ReplyEmbed.Description += $"**__CMYK__**: {ParsedColor.ToCmykString()}\n";
						ReplyEmbed.Description += $"**__HSV__**: {ParsedColor.ToHsvString()}\n";
						ReplyEmbed.Description += $"**__HSL__**: {ParsedColor.ToHslString()}\n";

						MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
						AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

						await Context.Channel.SendFileAsync(Stream, Filename, embed: ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);
					}
					finally
					{
						File.Delete(Filename);
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
		public async Task<RuntimeResult> VisualizeColorRgb(byte Red, byte Green, byte Blue)
		{
			string Filename = "colorView.png";
			SKColor ParsedColor;
			SKImageInfo ImageInfo = new SKImageInfo(512, 512);
			using (SKSurface Surface = SKSurface.Create(ImageInfo))
			{
				ParsedColor = new SKColor(Red, Green, Blue);
				Surface.Canvas.Clear(ParsedColor);

				using (SKPaint Paint = new SKPaint())
				{
					Paint.TextSize = 48;
					Paint.IsAntialias = true;
					Paint.TextAlign = SKTextAlign.Center;

					//Use black or white depending on background color.
					if ((ParsedColor.Red * 0.299f + ParsedColor.Green * 0.587f + ParsedColor.Blue * 0.114f) > 149)
						Paint.Color = SKColors.Black;
					else
						Paint.Color = SKColors.White;

					//thanks stack overflow lol
					int TextPosVertical = ImageInfo.Height / 2;
					float TextY = TextPosVertical + (((-Paint.FontMetrics.Ascent + Paint.FontMetrics.Descent) / 2) - Paint.FontMetrics.Descent);

					Surface.Canvas.DrawText(ParsedColor.ToRgbString().ToUpper(), ImageInfo.Width / 2f, TextY,
						new SKFont(SKTypeface.FromFamilyName("JetBrains Mono"), 48), Paint);
				}

				using (SKImage Image = Surface.Snapshot())
				using (SKData Data = Image.Encode(SKEncodedImageFormat.Png, 100))
				using (MemoryStream Stream = new MemoryStream((int)Data.Size))
				{
					Data.SaveTo(Stream);

					try
					{
						EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context)
							.ChangeTitle($"Color Visualization Of {ParsedColor.ToRgbString()}");

						ReplyEmbed.ImageUrl = $"attachment://{Filename}";
						ReplyEmbed.Color = new Color(ParsedColor.Red, ParsedColor.Green, ParsedColor.Blue);

						ReplyEmbed.Description += $"**__HEX__**: {ParsedColor.ToHexString()}\n";
						ReplyEmbed.Description += $"**__CMYK__**: {ParsedColor.ToCmykString()}\n";
						ReplyEmbed.Description += $"**__HSV__**: {ParsedColor.ToHsvString()}\n";
						ReplyEmbed.Description += $"**__HSL__**: {ParsedColor.ToHslString()}\n";

						MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
						AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

						await Context.Channel.SendFileAsync(Stream, Filename, embed: ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);
					}
					finally
					{
						File.Delete(Filename);
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
		public async Task<RuntimeResult> GetProfilePicAsync(IUser User)
		{
			EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

			ReplyEmbed.ChangeTitle($"{User.Username}'s Profile Picture");

			string UserAvatar = User.GetAvatarUrl(size: 2048);

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

			if (Context.User is SocketGuildUser)
			{
				SocketGuildUser Target = User as SocketGuildUser;

				string ServerAvatar = Target.GetGuildAvatarUrl(size: 2048);
				if (ServerAvatar != null)
				{
					//The user doesnt have a global avatar? Thats fine, we still have the server-specific one.
					if (UserAvatar != null)
					{
						ReplyEmbed.Description = $"[Global Profile Picture]({UserAvatar})";
					}

					ReplyEmbed.ImageUrl = ServerAvatar;

					await ReplyAsync(null, false, ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);

					return ExecutionResult.Succesful();
				}
			}

			//The user doesnt have a server-specific avatar, and doesnt have a global avatar either. Exit!
			if (UserAvatar == null)
				return ExecutionResult.FromError("This user does not have an avatar!");

			ReplyEmbed.ImageUrl = UserAvatar;

			await ReplyAsync(null, false, ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("weather")]
		[Summary("Gets the current weather for your city.")]
		[FullDescription("Gets the current weather forecast for your city. May not have all the information available, and the location may not be accurate.")]
		public async Task<RuntimeResult> GetWeatherAsync([Remainder] string City)
		{
			List<Location> RetrievedLocations = await GetWeatherLocationsAsync(City);
			if (RetrievedLocations.Count == 0)
				return ExecutionResult.FromError("That location does not exist.");

			Location FinalLocation = RetrievedLocations.First();
			CurrentWeather RetrievedWeather = await GetCurrentWeatherAsync(FinalLocation);
			Weather ActualWeather = RetrievedWeather.Weather[0];

			//Fucking christ. https://openweathermap.org/weather-conditions
			//Good luck.
			string ConditionEmoji = ActualWeather.Id switch
			{
				//2XX Thunderbolt group.
				(>= 200 and <= 202) or (>= 230 and <= 232) => "⛈️",
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
				>= 800 and < 801 => "☀️",

				//8XX Clouds group.
				>= 801 and < 802 => "🌤",
				>= 802 and < 803 => "⛅",
				>= 803 and < 804 => "🌥",
				>= 804 and < 805 => "☁️",

				//Unknown group id.
				_ => "❔"
			};

			float Cloudiness = RetrievedWeather.Clouds.Cloudiness;

			float Temperature = RetrievedWeather.Information.Temperature.RoundTo(1);
			float TemperatureMax = RetrievedWeather.Information.MaximumTemperature.RoundTo(1);
			float TemperatureMin = RetrievedWeather.Information.MinimumTemperature.RoundTo(1);
			float FeelsLike = RetrievedWeather.Information.FeelsLike.RoundTo(1);

			float Pressure = RetrievedWeather.Information.Pressure.RoundTo(2);
			float Humidity = RetrievedWeather.Information.Humidity;

			float WindSpeed = RetrievedWeather.Wind.Speed.MpsToKmh();
			float WindDirection = RetrievedWeather.Wind.Degrees;
			float WindGust = RetrievedWeather.Wind.Gust.MpsToKmh();

			long Sunrise = RetrievedWeather.System.Sunrise;
			long Sunset = RetrievedWeather.System.Sunset;

			string EmbedDescription = $"⚠️ Some data may be missing due to an API limitation.\n\n";

			EmbedDescription += $"{ConditionEmoji} **{ActualWeather.Description.CapitalizeFirst()}**\n";
			EmbedDescription += $"☁️ Cloudiness: **{Cloudiness}**%\n\n";

			EmbedDescription += $"🌡 Temperature: **{Temperature}**°C, **{Temperature.ToFahrenheit()}**°F\n";
			EmbedDescription += $"⬆ Temperature Max: **{TemperatureMax}**°C, **{TemperatureMax.ToFahrenheit()}**°F\n";
			EmbedDescription += $"⬇ Temperature Min: **{TemperatureMin}**°C, **{TemperatureMin.ToFahrenheit()}**°F\n";
			EmbedDescription += $"👤 Feels like: **{FeelsLike}**°C, **{FeelsLike.ToFahrenheit()}**°F\n\n";

			EmbedDescription += $"🌍 Atmospheric pressure: **{Pressure}**hPa, **{Pressure.ToPsi()}**psi\n";
			EmbedDescription += $"💧 Humidity: **{Humidity}**%\n\n";

			EmbedDescription += $"💨 Wind speed: **{WindSpeed}**km/h, **{WindSpeed.KmhToMph()}mph**\n";
			EmbedDescription += $"🧭 Wind direction: **{WindDirection}**°\n";
			EmbedDescription += $"🌬 Wind gust: **{WindGust}**km/h, **{WindGust.KmhToMph()}**mph\n\n";

			EmbedDescription += "⚠️ Sunrise and sunset times are adjusted to your computer's timezone.\n";
			EmbedDescription += $"🌅 Sunrise: <t:{Sunrise}:t>\n";
			EmbedDescription += $"🌇 Sunset: <t:{Sunset}:t>\n";

			//============================EMBED BUILDING============================

			EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context)
				.ChangeTitle($"Weather for {RetrievedWeather.Name}, {RetrievedWeather.System.Country.CountryCodeToFlag()}");

			ReplyEmbed.Description = EmbedDescription;

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync(null, false, ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		public async Task<List<Location>> GetWeatherLocationsAsync(string City)
		{
			string LocationReply = string.Empty;
			GeolocationParams GeolocationParameters = new GeolocationParams()
			{
				q = City,
				appid = Settings.Instance.LoadedConfig.OpenWeatherKey,
				limit = 1,
			};
			string LocationQuery = GeolocationParameters.ToQueryString();

			using (HttpResponseMessage HttpResponse = await UtilsService.WeatherClient.GetAsync($"geo/1.0/direct?{LocationQuery}"))
			{
				LocationReply = await HttpResponse.Content.ReadAsStringAsync();
			}
			List<Location> RetrievedLocations = JsonConvert.DeserializeObject<List<Location>>(LocationReply);

			return RetrievedLocations;
		}

		public async Task<CurrentWeather> GetCurrentWeatherAsync(Location Location)
		{
			string WeatherReply = string.Empty;
			WeatherParams WeatherParams = new WeatherParams()
			{
				lat = Location.Latitude,
				lon = Location.Longitude,
				appid = Settings.Instance.LoadedConfig.OpenWeatherKey,
				units = "metric"
			};
			string WeatherQuery = WeatherParams.ToQueryString();

			using (HttpResponseMessage HttpResponse = await UtilsService.WeatherClient.GetAsync($"data/2.5/weather?{WeatherQuery}"))
			{
				WeatherReply = await HttpResponse.Content.ReadAsStringAsync();
			}
			CurrentWeather RetrievedWeather = JsonConvert.DeserializeObject<CurrentWeather>(WeatherReply);

			return RetrievedWeather;
		}
	}
}
