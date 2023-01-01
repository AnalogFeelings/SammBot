#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2023 AestheticalZ
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
#endregion

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