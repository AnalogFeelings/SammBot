﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using SammBotNET.Extensions.ClassExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
	[Name("Utils")]
	[Group("utils")]
	[Summary("Moderation commands & misc.")]
	[ModuleEmoji("🔧")]
	public class UtilsModule : ModuleBase<SocketCommandContext>
	{
		public UtilsService UtilsService { get; set; }

		[Command("ban")]
		[Alias("toss", "bonk")]
		[Summary("Bans a user with a reason.")]
		[RequireBotPermission(GuildPermission.BanMembers)]
		[RequireUserPermission(GuildPermission.BanMembers)]
		public async Task<RuntimeResult> BanUserAsync(IUser User, int PruneDays, string Reason = null)
		{
			string BanReason = Reason ?? "No reason specified.";

			using (Context.Channel.EnterTypingState())
			{
				await Context.Guild.AddBanAsync(User, PruneDays, BanReason);

				await ReplyAsync($":hammer: **Banned user \"{User.Username}\" from this server.**\n" +
					$"Reason: *{BanReason}*");
			}

			return ExecutionResult.Succesful();
		}

		[Command("kick")]
		[Alias("boot", "exile")]
		[Summary("Kicks a user with a reason.")]
		[RequireBotPermission(GuildPermission.KickMembers)]
		[RequireUserPermission(GuildPermission.KickMembers)]
		public async Task<RuntimeResult> KickUserAsync(IUser User, string Reason = null)
		{
			string KickReason = Reason ?? "No reason specified.";
			IGuildUser TargetUser = Context.Guild.GetUser(User.Id);

			using (Context.Channel.EnterTypingState())
			{
				await TargetUser.KickAsync(KickReason);

				await ReplyAsync($":boot: **Kicked user \"{User.Username}\" from this server.**\n" +
					$"Reason: *{KickReason}*");
			}

			return ExecutionResult.Succesful();
		}

		[Command("avatar")]
		[Alias("pfp", "pic", "userpic")]
		[Summary("Gets the avatar of a user.")]
		public async Task<RuntimeResult> GetProfilePicAsync(IUser User)
		{
			EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

			ReplyEmbed.ChangeTitle($"{User.Username}'s Profile Picture");

			string UserAvatar = User.GetAvatarUrl(size: 2048);

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

					await Context.Channel.SendMessageAsync("", false, ReplyEmbed.Build());

					return ExecutionResult.Succesful();
				}
			}

			//The user doesnt have a server-specific avatar, and doesnt have a global avatar either. Exit!
			if (UserAvatar == null)
				return ExecutionResult.FromError("This user does not have an avatar!");

			ReplyEmbed.ImageUrl = UserAvatar;

			await Context.Channel.SendMessageAsync("", false, ReplyEmbed.Build());

			return ExecutionResult.Succesful();
		}

		[Command("weather")]
		[Summary("Gets the current weather for your city.")]
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

			await Context.Channel.SendMessageAsync("", false, ReplyEmbed.Build());

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
