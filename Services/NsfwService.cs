using System;
using System.Net.Http;

namespace SammBotNET.Services
{
    public class NsfwService
    {
        public readonly HttpClient NsfwClient = new()
        {
            BaseAddress = new Uri("https://rule34.xxx/")
        };
    }
}
