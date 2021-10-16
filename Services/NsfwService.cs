using System;
using System.Net.Http;

namespace SammBotNET.Services
{
    public class NsfwService
    {
        public readonly HttpClient NsfwClient = new() { BaseAddress = new Uri("rule34.xxx/index.php?page=dapi&s=post&q=index") };
    }
}
