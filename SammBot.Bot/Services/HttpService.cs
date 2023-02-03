#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021-2022 AestheticalZ
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

using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using SammBot.Bot.Core;
using SammBot.Bot.Extensions;

namespace SammBot.Bot.Services;

/// <summary>
/// Service containing utility methods for HTTP requests.
/// </summary>
public class HttpService
{
    /// <summary>
    /// The client used for making HTTP requests inside the service.
    /// </summary>
    public HttpClient Client { get; private set; }

    public HttpService()
    {
        Client = new HttpClient();
        
        Client.DefaultRequestHeaders.Add("User-Agent", SettingsManager.Instance.LoadedConfig.HttpUserAgent);
    }

    /// <summary>
    /// Retrieves a JSON string from <paramref name="Url"/>, appending <paramref name="Parameters"/> as a query string if not null, and
    /// returns them as <typeparamref name="T"/>.
    /// </summary>
    /// <param name="Url">The URL to retrieve the JSON data from.</param>
    /// <param name="Parameters">The parameters object that will get turned into a query string.</param>
    /// <typeparam name="T">The type of the object to be returned.</typeparam>
    /// <exception cref="ArgumentException">Thrown if <paramref name="Url"/> is empty or null.</exception>
    /// <returns>An object of type <typeparamref name="T"/> containing the deserialized data.</returns>
    public async Task<T?> GetObjectFromJsonAsync<T>(string Url, object? Parameters = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(Url, nameof(Url));
        
        UriBuilder uriBuilder = new UriBuilder(Url);

        if (Parameters != null)
        {
            NameValueCollection uriQuery = HttpUtility.ParseQueryString(uriBuilder.Query);
            NameValueCollection newQuery = HttpUtility.ParseQueryString(Parameters.ToQueryString());
            
            uriQuery.Add(newQuery);

            uriBuilder.Query = uriQuery.ToString();
        }

        string jsonReply;
        using (HttpResponseMessage responseMessage = await Client.GetAsync(uriBuilder.ToString()))
        {
            jsonReply = await responseMessage.Content.ReadAsStringAsync();
        }

        T? parsedReply = JsonConvert.DeserializeObject<T>(jsonReply);

        return parsedReply;
    }
}