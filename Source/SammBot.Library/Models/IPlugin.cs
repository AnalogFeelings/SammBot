#region License Information (GPLv3)
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

using Microsoft.Extensions.DependencyInjection;

namespace SammBot.Library.Models;

/// <summary>
/// Defines an interface for a plugin.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Initializes the plugin, with a provided service collection.
    /// </summary>
    /// <param name="serviceCollection">A service collection that can be modified.</param>
    public void Initialize(ServiceCollection serviceCollection);
}