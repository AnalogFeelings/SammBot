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

using System.Globalization;

namespace SammBot.Library.Helpers;

/// <summary>
/// Helper class to run async methods inside synchronous methods in a clean way.
/// </summary>
public static class AsyncHelper
{
    private static readonly TaskFactory _HelperTaskFactory = new TaskFactory(CancellationToken.None, TaskCreationOptions.None,
        TaskContinuationOptions.None, TaskScheduler.Default);

    /// <summary>
    /// Runs an async method under a synchronous context.
    /// </summary>
    /// <typeparam name="T">The return type of the async function.</typeparam>
    /// <param name="function">The function to call.</param>
    /// <returns>The returned object of the async method.</returns>
    public static T RunSync<T>(Func<Task<T>> function)
    {
        CultureInfo uiCulture = CultureInfo.CurrentUICulture;
        CultureInfo generalCulture = CultureInfo.CurrentCulture;

        return _HelperTaskFactory.StartNew(() =>
        {
            Thread.CurrentThread.CurrentUICulture = uiCulture;
            Thread.CurrentThread.CurrentCulture = generalCulture;

            return function();
        }).Unwrap().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Runs an async method under a synchronous context.
    /// </summary>
    /// <param name="function">The function to call.</param>
    public static void RunSync(Func<Task> function)
    {
        CultureInfo uiCulture = CultureInfo.CurrentUICulture;
        CultureInfo generalCulture = CultureInfo.CurrentCulture;

        _HelperTaskFactory.StartNew(() =>
        {
            Thread.CurrentThread.CurrentUICulture = uiCulture;
            Thread.CurrentThread.CurrentCulture = generalCulture;

            return function();
        }).Unwrap().GetAwaiter().GetResult();
    }
}