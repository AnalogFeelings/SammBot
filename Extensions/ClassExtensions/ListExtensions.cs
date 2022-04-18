using System.Collections.Generic;

namespace SammBotNET.Extensions
{
	public static class ListExtensions
	{
		public static T PickRandom<T>(this IList<T> TargetList)
		{
			return TargetList[Settings.Instance.GlobalRng.Next(TargetList.Count)];
		}
	}
}
