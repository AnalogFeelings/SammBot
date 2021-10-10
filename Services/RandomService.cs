using SharpCat.Requester.Cat;
using SharpCat.Requester.Dog;
using System;

namespace SammBotNET.Services
{
    public class RandomService
    {
        public SharpCatRequester requesterCat = new SharpCatRequester("0c1d1991-e890-4c04-8fe4-e97390999ad6");
        public SharpDogRequester requesterDog = new SharpDogRequester("887787e5-9363-47b1-8cde-39706c529598");
        public Random random = new Random();
    }
}
