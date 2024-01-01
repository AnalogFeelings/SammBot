#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021-2024 Analog Feelings
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Text.Json.Serialization;

namespace SammBot.Library.Rest.OpenWeather.Forecast;

public class CompleteForecast
{
    [JsonPropertyName("coord")]
    public Coordinate Coordinates { get; set; }
    [JsonPropertyName("weather")]
    public List<Condition> Weather { get; set; }
    [JsonPropertyName("main")]
    public MainForecast Information { get; set; }
    [JsonPropertyName("visibility")]
    public float Visibility { get; set; }
    [JsonPropertyName("wind")]
    public WindForecast Wind { get; set; }
    [JsonPropertyName("clouds")]
    public CloudForecast Clouds { get; set; }
    [JsonPropertyName("rain")]
    public RainForecast Rain { get; set; }
    [JsonPropertyName("snow")]
    public SnowForecast Snow { get; set; }
    [JsonPropertyName("dt")]
    public long CalculationTime { get; set; }
    [JsonPropertyName("sys")]
    public ForecastLocation Location { get; set; }
    [JsonPropertyName("timezone")]
    public int TimeShift { get; set; }
    [JsonPropertyName("id")]
    public long Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
}