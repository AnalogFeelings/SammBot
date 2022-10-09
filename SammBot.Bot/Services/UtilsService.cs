using System;
using System.Net.Http;

namespace SammBot.Bot.Services
{
    public class UtilsService
    {
        public readonly HttpClient WeatherClient;

        public UtilsService()
        {
            WeatherClient = new HttpClient() { BaseAddress = new Uri("http://api.openweathermap.org/") };
        }
    }
}
