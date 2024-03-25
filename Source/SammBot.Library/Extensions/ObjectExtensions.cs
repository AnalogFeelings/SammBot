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

using System.Reflection;
using System.Web;
using SammBot.Library.Attributes;
using System.Collections;

namespace SammBot.Library.Extensions;

/// <summary>
/// Contains extension methods for objects.
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Converts an object into a URL query string representation.
    /// </summary>
    /// <param name="targetObject">The object to convert.</param>
    /// <returns>The resulting query string.</returns>
    /// <remarks>The returned string does not include the "?" prefix.</remarks>
    public static string ToQueryString(this object targetObject)
    {
        Type targetType = targetObject.GetType();
        IEnumerable<string> formattedFields = from p in targetType.GetFields()
                                              let value = p.GetValue(targetObject)
                                              where value != null
                                              let uglyName = p.GetCustomAttribute<UglyName>()
                                              where uglyName != null
                                              select uglyName.Name + "=" + HttpUtility.UrlEncode(value.ToString());
        IEnumerable<string> formattedProperties = from p in targetType.GetProperties()
                                                  let value = p.GetValue(targetObject)
                                                  where value != null
                                                  let uglyName = p.GetCustomAttribute<UglyName>()
                                                  where uglyName != null
                                                  select uglyName.Name + "=" + HttpUtility.UrlEncode(value.ToString());

        IEnumerable<string> formattedMembers = formattedFields.Concat(formattedProperties);

        return string.Join("&", formattedMembers);
    }
}