using Newtonsoft.Json;

namespace SammBot.Bot.Rest.OpenWeather.Forecast;

public class WindForecast
{
    [JsonProperty("speed")]
    public float Speed { get; set; }
    [JsonProperty("degrees")]
    public float Degrees { get; set; }
    [JsonProperty("gust")]
    public float Gust { get; set; }
}