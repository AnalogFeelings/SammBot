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

using SammBot.Library;
using SammBot.Library.Components;
using SammBot.Library.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

namespace SammBot.Services;

/// <summary>
/// A service containing utility methods for HTTP requests.
/// </summary>
public class HttpService
{
    private readonly HttpClient _client;
    private readonly ConcurrentDictionary<string, TaskQueue> _queueDictionary;

    /// <summary>
    /// Creates a new <see cref="HttpService"/>.
    /// </summary>
    /// <param name="services">The current active service provider.</param>
    public HttpService(IServiceProvider services)
    {
        _client = new HttpClient();
        _queueDictionary = new ConcurrentDictionary<string, TaskQueue>();
        
        SettingsService settingsService = services.GetRequiredService<SettingsService>();

        _client.DefaultRequestHeaders.Add("User-Agent", settingsService.Settings!.HttpUserAgent);
    }

    /// <summary>
    /// Adds a domain to the queue dictionary to allow for custom
    /// waiting times for each domain.
    /// </summary>
    /// <param name="domain">The domain name of the website.</param>
    /// <param name="concurrentRequests">
    /// The amount of requests to let through before
    /// holding a queue.
    /// </param>
    /// <param name="releaseAfter">How much time to wait before opening the queue.</param>
    /// <remarks>
    /// If a domain is already added to the dictionary, the queue will be replaced with a new one.
    /// </remarks>
    public void RegisterDomainQueue(string domain, int concurrentRequests, TimeSpan releaseAfter)
    {
        // Invalid domain.
        if (Uri.CheckHostName(domain) == UriHostNameType.Unknown)
            return;

        TaskQueue newQueue = new TaskQueue(concurrentRequests, releaseAfter);

        _queueDictionary.AddOrUpdate(domain, newQueue, (_, _) => newQueue);
    }

    /// <summary>
    /// Removes a domain from the queue dictionary.
    /// </summary>
    /// <param name="domain">The domain name of the website.</param>
    public void UnregisterDomainQueue(string domain)
    {
        // Invalid domain.
        if (Uri.CheckHostName(domain) == UriHostNameType.Unknown)
            return;

        if (_queueDictionary.ContainsKey(domain))
            _queueDictionary.TryRemove(domain, out _);
    }

    /// <summary>
    /// Retrieves a JSON string from <paramref name="url"/>, appending <paramref name="parameters"/> as a query string if not null, and
    /// returns them as <typeparamref name="T"/>.
    /// </summary>
    /// <param name="url">The URL to retrieve the JSON data from.</param>
    /// <param name="parameters">The parameters object that will get turned into a query string.</param>
    /// <typeparam name="T">The type of the object to be returned.</typeparam>
    /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is empty or null.</exception>
    /// <returns>An object of type <typeparamref name="T"/> containing the deserialized data.</returns>
    public async Task<T?> GetObjectFromJsonAsync<T>(string url, object? parameters = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(url, nameof(url));

        UriBuilder uriBuilder = new UriBuilder(url);

        if (parameters != null)
        {
            NameValueCollection uriQuery = HttpUtility.ParseQueryString(uriBuilder.Query);
            NameValueCollection newQuery = HttpUtility.ParseQueryString(parameters.ToQueryString());

            uriQuery.Add(newQuery);

            uriBuilder.Query = uriQuery.ToString();
        }

        // This domain has a queue.
        if (_queueDictionary.TryGetValue(uriBuilder.Host, out TaskQueue? queue))
            return await queue.Enqueue(GetJsonRemote, CancellationToken.None);

        return await GetJsonRemote();

        async Task<T?> GetJsonRemote()
        {
            string jsonReply = await GetStringFromRemoteAsync(uriBuilder.ToString());
            T? parsedReply = JsonSerializer.Deserialize<T>(jsonReply, Constants.JsonSettings);

            return parsedReply;
        }
    }

    /// <summary>
    /// Retrieves a byte array from <paramref name="url"/>, appending <paramref name="parameters"/> as a query string if not null, and
    /// returns them as a <see cref="MemoryStream"/>.
    /// </summary>
    /// <param name="url">The URL to retrieve the byte array from.</param>
    /// <param name="parameters">The parameters object that will get turned into a query string.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is empty or null.</exception>
    /// <returns>An object of type <see cref="MemoryStream"/> containing the downloaded byte array.</returns>
    public async Task<MemoryStream> GetBytesFromRemoteAsync(string url, object? parameters = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(url, nameof(url));

        UriBuilder uriBuilder = new UriBuilder(url);

        if (parameters != null)
        {
            NameValueCollection uriQuery = HttpUtility.ParseQueryString(uriBuilder.Query);
            NameValueCollection newQuery = HttpUtility.ParseQueryString(parameters.ToQueryString());

            uriQuery.Add(newQuery);

            uriBuilder.Query = uriQuery.ToString();
        }
        
        // This domain has a queue.
        if (_queueDictionary.TryGetValue(uriBuilder.Host, out TaskQueue? queue))
            return await queue.Enqueue(GetStreamRemote, CancellationToken.None);

        return await GetStreamRemote();

        async Task<MemoryStream> GetStreamRemote()
        {
            byte[] rawData = await _client.GetByteArrayAsync(uriBuilder.ToString());

            return new MemoryStream(rawData);
        }
    }

    /// <summary>
    /// Retrieves a plain string from <paramref name="remoteUrl"/>.
    /// </summary>
    /// <param name="remoteUrl">The URL to retrieve the string from.</param>
    /// <returns>The returned string.</returns>
    private async Task<string> GetStringFromRemoteAsync(string remoteUrl)
    {
        string stringReply;

        using (HttpResponseMessage responseMessage = await _client.GetAsync(remoteUrl))
        {
            stringReply = await responseMessage.Content.ReadAsStringAsync();
        }

        return stringReply;
    }
}