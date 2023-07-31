#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021-2023 Analog Feelings
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using SammBot.Library.Components;

namespace SammBot.Tests.Components;

[TestClass]
public class TaskQueueTests
{
    private readonly TaskQueue _TaskQueue;

    public TaskQueueTests()
    {
        _TaskQueue = new TaskQueue(1);
    }

    [TestMethod]
    public async Task ConcurrencyTest()
    {
        for (int i = 0; i < 10; i++)
        {
            Task firstEnqueue = _TaskQueue.Enqueue(() => Task.Run(() =>
            {
                Task.Delay(750);
            }), CancellationToken.None);
            Task secondEnqueue = _TaskQueue.Enqueue(() => Task.Run(() =>
            {
                Task.Delay(500);
            }), CancellationToken.None);
            Task thirdEnqueue = _TaskQueue.Enqueue(() => Task.Run(() =>
            {
                Task.Delay(800);
            }), CancellationToken.None);

            Task finishedTask = await Task.WhenAny(firstEnqueue, secondEnqueue, thirdEnqueue);
        
            Assert.IsTrue(finishedTask == firstEnqueue, $"Second task finished before the first at attempt {i}.");
        }
    }
}