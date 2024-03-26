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

using Discord.Interactions;
using SammBot.Library.Attributes;
using SammBot.Library.Preconditions;
using System.Threading.Tasks;

namespace SammBot.Bot.Modules;

public partial class BooruModule
{
    [SlashCommand("r34", "Gets a list of images from rule34.")]
    [DetailedDescription("Gets a list of images from rule34. Maximum amount is 1000 images per command.")]
    [RateLimit(3, 2)]
    [RequireNsfw]
    public partial Task<RuntimeResult> GetRule34Async([Summary("Tags", "The tags you want to use for the search.")] string postTags);
    
    [SlashCommand("e621", "Gets a list of images from e621.")]
    [DetailedDescription("Gets a list of images from e621. Maximum amount is 320 images per command.")]
    [RateLimit(2, 1)]
    [RequireNsfw]
    public partial Task<RuntimeResult> GetE621Async([Summary("Tags", "The tags you want to use for the search.")] string postTags);
}