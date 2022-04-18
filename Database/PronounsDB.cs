using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SammBotNET.Database
{
	public class PronounsDB : DbContext
	{
		public virtual DbSet<Pronoun> Pronouns { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder OptionsBuilder)
		{
			SqliteConnectionStringBuilder ConnectionStringBuilder = new SqliteConnectionStringBuilder() { DataSource = "pronouns.db" };
			SqliteConnection Connection = new SqliteConnection(ConnectionStringBuilder.ToString());

			OptionsBuilder.UseSqlite(Connection);
		}
	}
}
