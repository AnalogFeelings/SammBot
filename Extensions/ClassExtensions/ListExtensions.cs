using System.Collections.Generic;

namespace SammBotNET.Extensions
{
    public static class ListExtensions
    {
        public static T PickRandom<T>(this IList<T> items)
        {
            return items[BotCore.Instance.GlobalRng.Next(items.Count)];
        }
    }
}
