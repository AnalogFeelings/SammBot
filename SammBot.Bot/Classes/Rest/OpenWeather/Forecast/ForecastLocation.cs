using Newtonsoft.Json;

namespace SammBot.Bot.Rest.OpenWeather.Forecast;

public class ForecastLocation
{
    [JsonProperty("type")]
    public int Type { get; set; }
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("message")]
    public float Message { get; set; }
    [JsonProperty("country")]
    public string Country { get; set; }
    [JsonProperty("sunrise")]
    public long Sunrise { get; set; }
    [JsonProperty("sunset")]
    public long Sunset { get; set; }
}