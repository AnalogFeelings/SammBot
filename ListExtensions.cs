using System;
using System.Collections.Generic;

namespace SammBotNET
{
    public static class ListExtensions
    {
        private static Random _rnd = new Random();

        public static T PickRandom<T>(this IList<T> items)
        {
            return items[_rnd.Next(items.Count)];
        }
    }
}
