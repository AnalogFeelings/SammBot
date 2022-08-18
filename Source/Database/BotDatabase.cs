using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace SammBotNET.Database
{
	public class BotDatabase : DbContext
	{
		public DbSet<PeoneImage> PeoneImages { get; set; }
		public DbSet<Pronoun> Pronouns { get; set; }
		public DbSet<UserTag> UserTags { get; set; }
		public DbSet<UserWarning> UserWarnings { get; set; }
		public DbSet<GuildConfig> GuildConfigs { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder OptionsBuilder)
		{
			string DatabaseFile = Path.Combine(Settings.Instance.BotDataDirectory, "bot.db");

			SqliteConnectionStringBuilder ConnectionStringBuilder = new SqliteConnectionStringBuilder() { DataSource = DatabaseFile };
			SqliteConnection Connection = new SqliteConnection(ConnectionStringBuilder.ToString());

			OptionsBuilder.UseSqlite(Connection);
		}
	}
}
