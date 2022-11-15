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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuildConfig>().Property(x => x.WarningLimit).HasDefaultValue(3);
            modelBuilder.Entity<GuildConfig>().Property(x => x.WarningLimitAction).HasDefaultValue(WarnLimitAction.None);
            
            modelBuilder.Entity<GuildConfig>().Property(x => x.EnableLogging).HasDefaultValue(false);
            modelBuilder.Entity<GuildConfig>().Property(x => x.LogChannel).HasDefaultValue(0);
            
            modelBuilder.Entity<GuildConfig>().Property(x => x.EnableWelcome).HasDefaultValue(false);
            modelBuilder.Entity<GuildConfig>().Property(x => x.WelcomeChannel).HasDefaultValue(0);
            modelBuilder.Entity<GuildConfig>().Property(x => x.WelcomeMessage).HasDefaultValue("{0}, welcome to {1}! Remember to read the rules before chatting!");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder OptionsBuilder)
        {
            string databaseFile = Path.Combine(SettingsManager.Instance.BotDataDirectory, "bot.db");

            SqliteConnectionStringBuilder connectionStringBuilder = new SqliteConnectionStringBuilder() { DataSource = databaseFile };
            SqliteConnection connection = new SqliteConnection(connectionStringBuilder.ToString());

            OptionsBuilder.UseSqlite(connection);
        }
    }
}
