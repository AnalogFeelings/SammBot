using SammBotNET.Extensions;
using SharpCat.Requester.Cat;
using SharpCat.Requester.Dog;

namespace SammBotNET.Services
{
    public class RandomService
    {
        public SharpCatRequester CatRequester = new("0c1d1991-e890-4c04-8fe4-e97390999ad6");
        public SharpDogRequester DogRequester = new("887787e5-9363-47b1-8cde-39706c529598");

        public AutodeqList<string> RecentPeoneImages = new(10);
    }
}
