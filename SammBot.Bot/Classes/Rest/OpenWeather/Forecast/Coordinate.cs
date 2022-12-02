using Newtonsoft.Json;

namespace SammBot.Bot.Rest.OpenWeather.Forecast;

public class Coordinate
{
    [JsonProperty("lat")]
    public float Latitude { get; set; }
    [JsonProperty("lon")]
    public float Longitude { get; set; }
}