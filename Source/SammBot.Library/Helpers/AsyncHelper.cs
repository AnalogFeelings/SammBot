﻿#region License Information (MIT)

/*
 * Modified from ASP.NET Identity, made by Microsoft Corporation.
 * https://github.com/aspnet/AspNetIdentity/blob/main/src/Microsoft.AspNet.Identity.Core/AsyncHelper.cs
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

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
    /// <param name="Function">The function to call.</param>
    /// <returns>The returned object of the async method.</returns>
    public static T RunSync<T>(Func<Task<T>> Function)
    {
        CultureInfo uiCulture = CultureInfo.CurrentUICulture;
        CultureInfo generalCulture = CultureInfo.CurrentCulture;

        return _HelperTaskFactory.StartNew(() =>
        {
            Thread.CurrentThread.CurrentUICulture = uiCulture;
            Thread.CurrentThread.CurrentCulture = generalCulture;

            return Function();
        }).Unwrap().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Runs an async method under a synchronous context.
    /// </summary>
    /// <param name="Function">The function to call.</param>
    public static void RunSync(Func<Task> Function)
    {
        CultureInfo uiCulture = CultureInfo.CurrentUICulture;
        CultureInfo generalCulture = CultureInfo.CurrentCulture;

        _HelperTaskFactory.StartNew(() =>
        {
            Thread.CurrentThread.CurrentUICulture = uiCulture;
            Thread.CurrentThread.CurrentCulture = generalCulture;

            return Function();
        }).Unwrap().GetAwaiter().GetResult();
    }
}