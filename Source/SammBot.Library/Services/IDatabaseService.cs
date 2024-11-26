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

using Microsoft.EntityFrameworkCore;
using SammBot.Library.Models.Database;

namespace SammBot.Library.Services;

/// <summary>
/// Defines an interface for a database service.
/// </summary>
public interface IDatabaseService : IDisposable
{
    public DbSet<UserTag> UserTags { get; set; }
    public DbSet<UserWarning> UserWarnings { get; set; }
    public DbSet<GuildConfig> GuildConfigs { get; set; }
}