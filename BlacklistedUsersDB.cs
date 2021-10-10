using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SammBotNET.Database
{
    public class BlacklistedUsersDB : DbContext
    {
        public virtual DbSet<BlacklistedUser> BlacklistedUser { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            SqliteConnectionStringBuilder cSb = new SqliteConnectionStringBuilder { DataSource = "blacklist.db" };
            SqliteConnection connection = new SqliteConnection(cSb.ToString());
            optionsBuilder.UseSqlite(connection);
        }
    }
}
