using Newtonsoft.Json;

namespace SammBot.Bot.Rest.OpenWeather.Forecast;

public class ForecastLocation
{
    [JsonProperty("country")]
    public string Country { get; set; }
    [JsonProperty("sunrise")]
    public long Sunrise { get; set; }
    [JsonProperty("sunset")]
    public long Sunset { get; set; }
}