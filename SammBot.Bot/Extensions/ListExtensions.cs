using System;
using System.Collections.Generic;

namespace SammBot.Bot.Extensions
{
    public static class ListExtensions
    {
        public static T PickRandom<T>(this IList<T> TargetList)
        {
            return TargetList[Random.Shared.Next(TargetList.Count)];
        }
    }
}
