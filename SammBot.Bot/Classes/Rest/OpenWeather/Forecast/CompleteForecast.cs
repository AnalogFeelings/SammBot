using System.Collections.Generic;
using Newtonsoft.Json;

namespace SammBot.Bot.Rest.OpenWeather.Forecast;

public class CompleteForecast
{
    [JsonProperty("coord")]
    public Coordinate Coordinates { get; set; }
    [JsonProperty("weather")]
    public List<Condition> Weather { get; set; }
    [JsonProperty("main")]
    public MainForecast Information { get; set; }
    [JsonProperty("visibility")]
    public float Visibility { get; set; }
    [JsonProperty("wind")]
    public WindForecast Wind { get; set; }
    [JsonProperty("clouds")]
    public CloudForecast Clouds { get; set; }
    [JsonProperty("rain")]
    public RainForecast Rain { get; set; }
    [JsonProperty("snow")]
    public SnowForecast Snow { get; set; }
    [JsonProperty("dt")]
    public long CalculationTime { get; set; }
    [JsonProperty("sys")]
    public ForecastLocation Location { get; set; }
    [JsonProperty("timezone")]
    public int TimeShift { get; set; }
    [JsonProperty("id")]
    public long Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
}