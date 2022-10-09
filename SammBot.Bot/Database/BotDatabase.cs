using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SammBot.Bot.Core;
using System.IO;

namespace SammBot.Bot.Database
{
    public class BotDatabase : DbContext
    {
        public DbSet<Pronoun> Pronouns { get; set; }
        public DbSet<UserTag> UserTags { get; set; }
        public DbSet<UserWarning> UserWarnings { get; set; }
        public DbSet<GuildConfig> GuildConfigs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder OptionsBuilder)
        {
            string databaseFile = Path.Combine(Settings.Instance.BotDataDirectory, "bot.db");

            SqliteConnectionStringBuilder connectionStringBuilder = new SqliteConnectionStringBuilder() { DataSource = databaseFile };
            SqliteConnection connection = new SqliteConnection(connectionStringBuilder.ToString());

            OptionsBuilder.UseSqlite(connection);
        }
    }
}
