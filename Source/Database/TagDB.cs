using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SammBotNET.Database
{
	public class TagDB : DbContext
	{
		public virtual DbSet<UserTag> UserTag { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder OptionsBuilder)
		{
			SqliteConnectionStringBuilder ConnectionStringBuilder = new SqliteConnectionStringBuilder() { DataSource = "usertags.db" };
			SqliteConnection Connection = new SqliteConnection(ConnectionStringBuilder.ToString());

			OptionsBuilder.UseSqlite(Connection);
		}
	}
}
