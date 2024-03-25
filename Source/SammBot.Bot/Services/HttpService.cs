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

using SammBot.Bot.Settings;
using SammBot.Library;
using SammBot.Library.Components;
using SammBot.Library.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SammBot.Bot.Services;

/// <summary>
/// A service containing utility methods for HTTP requests.
/// </summary>
public class HttpService
{
    /// <summary>
    /// The client used for making HTTP requests inside the service.
    /// </summary>
    public HttpClient Client { get; init; }

    /// <summary>
    /// A concurrent dictionary that contains a list of domain names and
    /// their corresponding task queues.
    /// </summary>
    private readonly ConcurrentDictionary<string, TaskQueue> _QueueDictionary;

    public HttpService()
    {
        Client = new HttpClient();
        _QueueDictionary = new ConcurrentDictionary<string, TaskQueue>();

        Client.DefaultRequestHeaders.Add("User-Agent", SettingsManager.Instance.LoadedConfig.HttpUserAgent);
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

        _QueueDictionary.AddOrUpdate(domain, newQueue, (_, _) => newQueue);
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

        if (_QueueDictionary.ContainsKey(domain))
            _QueueDictionary.TryRemove(domain, out _);
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
        if (_QueueDictionary.TryGetValue(uriBuilder.Host, out TaskQueue? queue) && queue != default)
            return await queue.Enqueue(GetJsonRemote, CancellationToken.None);

        return await GetJsonRemote();

        async Task<T?> GetJsonRemote()
        {
            string jsonReply = await GetStringFromRemote(uriBuilder.ToString());
            T? parsedReply = JsonSerializer.Deserialize<T>(jsonReply, Constants.JsonSettings);

            return parsedReply;
        }
    }

    /// <summary>
    /// Retrieves a plain string from <paramref name="remoteUrl"/>.
    /// </summary>
    /// <param name="remoteUrl">The URL to retrieve the string from.</param>
    /// <returns>The returned string.</returns>
    private async Task<string> GetStringFromRemote(string remoteUrl)
    {
        string stringReply;

        using (HttpResponseMessage responseMessage = await Client.GetAsync(remoteUrl))
        {
            stringReply = await responseMessage.Content.ReadAsStringAsync();
        }

        return stringReply;
    }
}