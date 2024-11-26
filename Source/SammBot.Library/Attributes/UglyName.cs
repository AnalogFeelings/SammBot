﻿#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021 Analog Feelings
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

namespace SammBot.Library.Attributes;

/// <summary>
/// An attribute used to denote an internal "ugly" name for a property.
/// <para/>
/// An example would be when exposing REST parameters to C#.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class UglyName : Attribute
{
    /// <summary>
    /// The ugly name of the property.
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// Creates a new instance of the <see cref="UglyName"/> class.
    /// </summary>
    /// <param name="name">The ugly name of the property.</param>
    public UglyName(string name) => 
        this.Name = name;
}