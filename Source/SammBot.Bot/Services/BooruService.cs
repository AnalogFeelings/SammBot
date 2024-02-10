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

namespace SammBot.Bot.Services;

public class BooruService
{
    private const string _R34_DOMAIN = "api.rule34.xxx";
    private const string _E621_DOMAIN = "e621.net";

    public BooruService(HttpService httpService)
    {
        httpService.RegisterDomainQueue(_R34_DOMAIN, 3, TimeSpan.FromSeconds(2));
        httpService.RegisterDomainQueue(_E621_DOMAIN, 1, TimeSpan.FromSeconds(1));
    }
}