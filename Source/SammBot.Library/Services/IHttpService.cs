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

namespace SammBot.Library.Services;

/// <summary>
/// A service containing utility methods for HTTP requests.
/// </summary>
public interface IHttpService
{
    /// <summary>
    /// The client used for making HTTP requests inside the service.
    /// </summary>
    public HttpClient Client { get; init; }

    /// <summary>
    /// Adds a domain-specific queue.
    /// </summary>
    /// <param name="domain">The domain name of the website.</param>
    /// <param name="concurrentRequests">The amount of requests to let through beforeholding a queue.</param>
    /// <param name="releaseAfter">How much time to wait before opening the queue.</param>
    public void RegisterDomainQueue(string domain, int concurrentRequests, TimeSpan releaseAfter);
    
    /// <summary>
    /// Removes a domain-specific queue.
    /// </summary>
    /// <param name="domain">The domain name of the website.</param>
    public void UnregisterDomainQueue(string domain);

    /// <summary>
    /// Retrieves a JSON string from <paramref name="Url"/>, appending <paramref name="Parameters"/> as a query string if not null, and
    /// returns them as <typeparamref name="T"/>.
    /// </summary>
    /// <param name="Url">The URL to retrieve the JSON data from.</param>
    /// <param name="Parameters">The parameters object that will get turned into a query string.</param>
    /// <typeparam name="T">The type of the object to be returned.</typeparam>
    /// <exception cref="ArgumentException">Thrown if <paramref name="Url"/> is empty or null.</exception>
    /// <returns>An object of type <typeparamref name="T"/> containing the deserialized data.</returns>
    public Task<T?> GetObjectFromJsonAsync<T>(string Url, object? Parameters = null);
}