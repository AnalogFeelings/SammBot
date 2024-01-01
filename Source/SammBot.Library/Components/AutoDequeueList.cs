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

namespace SammBot.Library.Components;

/// <summary>
/// A <see cref="LinkedList{T}"/> that automatically removes the last element after
/// reaching a set capacity.
/// </summary>
/// <typeparam name="T">The type of the <see cref="LinkedList{T}"/>.</typeparam>
public class AutoDequeueList<T> : LinkedList<T>
{
    /// <summary>
    /// The capacity of the list.
    /// </summary>
    private readonly int _MaxSize;

    /// <summary>
    /// Creates a new instance of the <see cref="AutoDequeueList{T}"/> class.
    /// </summary>
    /// <param name="MaxSize">The capacity of the list.</param>
    public AutoDequeueList(int MaxSize) => 
        this._MaxSize = MaxSize;

    /// <summary>
    /// Pushes an item to the beginning of the list.
    /// <para/>
    /// If the list has reached maximum capacity, the last item will be removed.
    /// </summary>
    /// <param name="Item">The item to add to the list.</param>
    public void Push(T Item)
    {
        this.AddFirst(Item);

        if (this.Count > _MaxSize) 
            this.RemoveLast();
    }
}