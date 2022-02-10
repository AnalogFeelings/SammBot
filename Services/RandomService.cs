using SharpCat.Requester.Cat;
using SharpCat.Requester.Dog;

namespace SammBotNET.Services
{
    public class RandomService
    {
        public SharpCatRequester CatRequester;
        public SharpDogRequester DogRequester;

        public AutodeqList<string> RecentPeoneImages;

		public RandomService()
		{
			RecentPeoneImages = new(BotCore.Instance.LoadedConfig.PeoneRecentQueueSize);
			CatRequester = new(BotCore.Instance.LoadedConfig.CatKey);
			DogRequester = new(BotCore.Instance.LoadedConfig.DogKey);
		}
    }
}
