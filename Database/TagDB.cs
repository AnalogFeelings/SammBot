using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SammBotNET.Database
{
    public class TagDB : DbContext
    {
        public virtual DbSet<UserTag> UserTag { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            SqliteConnectionStringBuilder cSb = new SqliteConnectionStringBuilder() { DataSource = "usertags.db" };
            SqliteConnection connection = new SqliteConnection(cSb.ToString());
            optionsBuilder.UseSqlite(connection);
        }
    }
}
