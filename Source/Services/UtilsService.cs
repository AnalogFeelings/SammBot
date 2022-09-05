using System;
using System.Net.Http;

namespace SammBotNET.Services
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
