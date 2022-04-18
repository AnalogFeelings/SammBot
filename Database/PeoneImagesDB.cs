using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SammBotNET.Database
{
	public class PeoneImagesDB : DbContext
	{
		public virtual DbSet<PeoneImage> PeoneImage { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder OptionsBuilder)
		{
			SqliteConnectionStringBuilder ConnectionStringBuilder = new SqliteConnectionStringBuilder() { DataSource = "peone.db" };
			SqliteConnection Connection = new SqliteConnection(ConnectionStringBuilder.ToString());

			OptionsBuilder.UseSqlite(Connection);
		}
	}
}
