using Newtonsoft.Json;

namespace SammBot.Bot.Rest.OpenWeather.Forecast;

public class SnowForecast
{
    [JsonProperty("1h")]
    public float OneHour { get; set; }
    [JsonProperty("3h")]
    public float ThreeHour { get; set; }
}