using Newtonsoft.Json;

namespace SammBot.Bot.Rest.OpenWeather.Forecast;

public class MainForecast
{
    [JsonProperty("temp")]
    public float Temperature { get; set; }
    [JsonProperty("feels_like")]
    public float FeelsLike { get; set; }
    [JsonProperty("pressure")]
    public float Pressure { get; set; }
    [JsonProperty("humidity")]
    public float Humidity { get; set; }
    [JsonProperty("temp_min")]
    public float MinimumTemperature { get; set; }
    [JsonProperty("temp_max")]
    public float MaximumTemperature { get; set; }
    [JsonProperty("sea_level")]
    public float SeaLevelPressure { get; set; }
    [JsonProperty("grnd_level")]
    public float GroundLevelPressure { get; set; }
}