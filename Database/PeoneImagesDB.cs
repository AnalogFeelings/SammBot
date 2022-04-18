using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SammBotNET.Database
{
	public class PeoneImagesDB : DbContext
	{
		public virtual DbSet<PeoneImage> PeoneImage { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			SqliteConnectionStringBuilder cSb = new SqliteConnectionStringBuilder() { DataSource = "peone.db" };
			SqliteConnection connection = new SqliteConnection(cSb.ToString());
			optionsBuilder.UseSqlite(connection);
		}
	}
}
