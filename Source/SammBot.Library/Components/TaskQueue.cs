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

namespace SammBot.Library.Components;

/// <summary>
/// Implements a queue of <see cref="Task{TResult}"/> for concurrency purposes.
/// </summary>
public class TaskQueue
{
    private readonly SemaphoreSlim _Semaphore;

    /// <summary>
    /// Creates a new <see cref="TaskQueue"/> instance.
    /// </summary>
    /// <param name="concurrentRequests">The number of requests to let through before the queue is held.</param>
    public TaskQueue(int concurrentRequests)
    {
        _Semaphore = new SemaphoreSlim(concurrentRequests);
    }
    
    /// <summary>
    /// Adds a <see cref="Task{TResult}"/> to the queue and waits if the queue is held.
    /// </summary>
    /// <param name="taskSource">A <see cref="Func{TResult}"/> that contains the Task to enqueue.</param>
    /// <param name="releaseAfter">How much time to wait before opening the queue.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel the queue wait.</param>
    /// <typeparam name="T">The return type of the enqueued Task.</typeparam>
    /// <returns>The result of the Task.</returns>
    public async Task<T> Enqueue<T>(Func<Task<T>> taskSource, TimeSpan releaseAfter, CancellationToken cancellationToken)
    {
        await _Semaphore.WaitAsync(cancellationToken);
        
        try
        {
            return await taskSource();
        }
        finally
        {
            _ = Task.Run(() => TimedRelease(releaseAfter), CancellationToken.None);
        }
    }
    
    /// <summary>
    /// Adds a <see cref="Task"/> to the queue and waits if the queue is held.
    /// </summary>
    /// <param name="taskSource">A <see cref="Func{TResult}"/> that contains the Task to enqueue.</param>
    /// <param name="releaseAfter">How much time to wait before opening the queue.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel the queue wait.</param>
    public async Task Enqueue(Func<Task> taskSource, TimeSpan releaseAfter, CancellationToken cancellationToken)
    {
        await _Semaphore.WaitAsync(cancellationToken);
        
        try
        {
            await taskSource();
        }
        finally
        {
            _ = Task.Run(() => TimedRelease(releaseAfter), CancellationToken.None);
        }
    }

    /// <summary>
    /// Releases the semaphore after a period of time.
    /// </summary>
    /// <param name="releaseAfter">How much time to wait before releasing.</param>
    private async Task TimedRelease(TimeSpan releaseAfter)
    {
        if(releaseAfter != TimeSpan.Zero)
            await Task.Delay(releaseAfter);

        _Semaphore.Release();
    }
}