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

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SammBot.Bot.Core;
using System.IO;
using JetBrains.Annotations;
using SammBot.Bot.Database.Models;

namespace SammBot.Bot.Database;

public class BotDatabase : DbContext
{
    [UsedImplicitly] public DbSet<Pronoun> Pronouns { get; set; } = default!;
    [UsedImplicitly] public DbSet<UserTag> UserTags { get; set; } = default!;
    [UsedImplicitly] public DbSet<UserWarning> UserWarnings { get; set; } = default!;
    [UsedImplicitly] public DbSet<GuildConfig> GuildConfigs { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder ModelBuilder)
    {
        ModelBuilder.Entity<GuildConfig>().Property(x => x.WarningLimit).HasDefaultValue(3);
        ModelBuilder.Entity<GuildConfig>().Property(x => x.WarningLimitAction).HasDefaultValue(WarnLimitAction.None);
            
        ModelBuilder.Entity<GuildConfig>().Property(x => x.EnableLogging).HasDefaultValue(false);
        ModelBuilder.Entity<GuildConfig>().Property(x => x.LogChannel).HasDefaultValue(0);
            
        ModelBuilder.Entity<GuildConfig>().Property(x => x.EnableWelcome).HasDefaultValue(false);
        ModelBuilder.Entity<GuildConfig>().Property(x => x.WelcomeChannel).HasDefaultValue(0);
        ModelBuilder.Entity<GuildConfig>().Property(x => x.WelcomeMessage).HasDefaultValue("%usermention%, welcome to %servername%! Remember to read the rules before chatting!");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder OptionsBuilder)
    {
        string databaseFile = Path.Combine(SettingsManager.Instance.BotDataDirectory, "bot.db");

        SqliteConnectionStringBuilder connectionStringBuilder = new SqliteConnectionStringBuilder() { DataSource = databaseFile };
        SqliteConnection connection = new SqliteConnection(connectionStringBuilder.ToString());

        OptionsBuilder.UseSqlite(connection);
    }
}