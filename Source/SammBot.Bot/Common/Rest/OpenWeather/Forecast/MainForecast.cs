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

using Newtonsoft.Json;

namespace SammBot.Bot.Common.Rest.OpenWeather.Forecast;

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