using System;
using System.Collections.Generic;
using System.Net.Http;

namespace SammBotNET.Services
{
    public class NsfwService
    {
        public Dictionary<ulong, List<Rule34Post>> ActivePostLists { get; set; }

        public readonly HttpClient NsfwClient = new()
        {
            BaseAddress = new Uri("https://rule34.xxx/")
        };
    }
}
