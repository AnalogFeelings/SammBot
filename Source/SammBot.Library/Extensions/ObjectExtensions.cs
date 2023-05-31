#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2023 Analog Feelings
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
#endregion

using System.Reflection;
using System.Web;
using SammBot.Library.Attributes;

namespace SammBot.Library.Extensions;

public static class ObjectExtensions
{
    public static string ToQueryString(this object TargetObject)
    {
        IEnumerable<string> formattedProperties = from p in TargetObject.GetType().GetProperties()
            where p.GetValue(TargetObject, null) != null
            where p.GetCustomAttribute<UglyName>() != null
            select p.GetCustomAttribute<UglyName>()!.Name + "=" + HttpUtility.UrlEncode(p.GetValue(TargetObject, null).ToString());

        return string.Join("&", formattedProperties.ToArray());
    }
}