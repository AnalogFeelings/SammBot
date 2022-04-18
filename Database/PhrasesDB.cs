using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SammBotNET.Database
{
	public class PhrasesDB : DbContext
	{
		public virtual DbSet<Phrase> Phrase { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder OptionsBuilder)
		{
			SqliteConnectionStringBuilder ConnectionStringBuilder = new SqliteConnectionStringBuilder() { DataSource = "phrases.db" };
			SqliteConnection Connection = new SqliteConnection(ConnectionStringBuilder.ToString());

			OptionsBuilder.UseSqlite(Connection);
		}
	}
}
