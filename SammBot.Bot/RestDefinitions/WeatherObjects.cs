using Newtonsoft.Json;
using System.Collections.Generic;

namespace SammBot.Bot.RestDefinitions
{
    public class GeolocationParams
    {
        public string q { get; set; }
        public string appid { get; set; }
        public int limit { get; set; }
    }

    public class WeatherParams
    {
        public float lat { get; set; }
        public float lon { get; set; }
        public string appid { get; set; }
        public string units { get; set; }
    }

    public class Coord
    {
        [JsonProperty("lat")]
        public float Latitude { get; set; }
        [JsonProperty("lon")]
        public float Longitude { get; set; }
    }

    public class Weather
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("main")]
        public string Main { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("icon")]
        public string Icon { get; set; }
    }

    public class Main
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

    public class Clouds
    {
        [JsonProperty("all")]
        public float Cloudiness { get; set; }
    }

    public class Rain
    {
        [JsonProperty("1h")]
        public float OneHour { get; set; }
        [JsonProperty("3h")]
        public float ThreeHour { get; set; }
    }

    public class Snow
    {
        [JsonProperty("1h")]
        public float OneHour { get; set; }
        [JsonProperty("3h")]
        public float ThreeHour { get; set; }
    }

    public class Sys
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

    public class Wind
    {
        [JsonProperty("speed")]
        public float Speed { get; set; }
        [JsonProperty("degrees")]
        public float Degrees { get; set; }
        [JsonProperty("gust")]
        public float Gust { get; set; }
    }

    public class CurrentWeather
    {
        [JsonProperty("coord")]
        public Coord Coordinates { get; set; }
        [JsonProperty("weather")]
        public List<Weather> Weather { get; set; }
        [JsonProperty("main")]
        public Main Information { get; set; }
        [JsonProperty("visibility")]
        public float Visibility { get; set; }
        [JsonProperty("wind")]
        public Wind Wind { get; set; }
        [JsonProperty("clouds")]
        public Clouds Clouds { get; set; }
        [JsonProperty("rain")]
        public Rain Rain { get; set; }
        [JsonProperty("snow")]
        public Snow Snow { get; set; }
        [JsonProperty("dt")]
        public long CalculationTime { get; set; }
        [JsonProperty("sys")]
        public Sys System { get; set; }
        [JsonProperty("timezone")]
        public int TimeShift { get; set; }
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("cod")]
        public int Cod { get; set; }
    }

    public class Location
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("lat")]
        public float Latitude { get; set; }
        [JsonProperty("lon")]
        public float Longitude { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
    }
}
