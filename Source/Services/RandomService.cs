﻿using SharpCat.Requester.Cat;
using SharpCat.Requester.Dog;
using System.Net.Http;

namespace SammBotNET.Services
{
	public class RandomService
	{
		public SharpCatRequester CatRequester;
		public SharpDogRequester DogRequester;

		public AutodeqList<string> RecentPeoneImages;

		public readonly HttpClient RandomClient;

		public RandomService()
		{
			RecentPeoneImages = new AutodeqList<string>(Settings.Instance.LoadedConfig.PeoneRecentQueueSize);
			CatRequester = new SharpCatRequester(Settings.Instance.LoadedConfig.CatKey);
			DogRequester = new SharpDogRequester(Settings.Instance.LoadedConfig.DogKey);
			RandomClient = new HttpClient();
		}
	}
}
