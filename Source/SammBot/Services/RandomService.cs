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

using System;
using Microsoft.Extensions.DependencyInjection;
using SammBot.Modules;
using SharpCat.Requester.Cat;
using SharpCat.Requester.Dog;

namespace SammBot.Services;

/// <summary>
/// Stores permanent data for <see cref="RandomModule"/>.
/// </summary>
public class RandomService
{
    public readonly SharpCatRequester CatRequester;
    public readonly SharpDogRequester DogRequester;

    /// <summary>
    /// Creates a new <see cref="RandomService"/>.
    /// </summary>
    /// <param name="services">The current active service provider.</param>
    public RandomService(IServiceProvider services)
    {
        SettingsService settingsService = services.GetRequiredService<SettingsService>();
        
        CatRequester = new SharpCatRequester(settingsService.Settings.CatKey);
        DogRequester = new SharpDogRequester(settingsService.Settings.DogKey);
    }
}