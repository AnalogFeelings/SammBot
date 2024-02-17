#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021-2024 Analog Feelings
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

namespace SammBot.Library.Extensions;

/// <summary>
/// Contains extension methods for lists.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Picks a random element in <paramref name="targetList"/> and returns it.
    /// </summary>
    /// <param name="targetList">The list to grab the element from.</param>
    /// <returns>The picked element.</returns>
    public static T PickRandom<T>(this IList<T> targetList)
    {
        return targetList[Random.Shared.Next(targetList.Count)];
    }
}