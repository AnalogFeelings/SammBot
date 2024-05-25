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
using SharpCat.Requester.Cat;
using SharpCat.Requester.Dog;

namespace SammBot.Bot.Services;

public class RandomService
{
    public readonly SharpCatRequester CatRequester;
    public readonly SharpDogRequester DogRequester;

    public RandomService(IServiceProvider provider)
    {
        SettingsService settingsService = provider.GetRequiredService<SettingsService>();
        
        CatRequester = new SharpCatRequester(settingsService.Settings.CatKey);
        DogRequester = new SharpDogRequester(settingsService.Settings.DogKey);
    }
}