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

namespace SammBot.Library.Attributes;

/// <summary>
/// An attribute that denotes the extended description of a method or property.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public class DetailedDescription : Attribute
{
    /// <summary>
    /// The description of the method or property.
    /// </summary>
    public readonly string Description;

    /// <summary>
    /// Creates a new instance of the <see cref="DetailedDescription"/> class.
    /// </summary>
    /// <param name="Description">The extended description of the method or property.</param>
    public DetailedDescription(string Description) => 
        this.Description = Description;
}