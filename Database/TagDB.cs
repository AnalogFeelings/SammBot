using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SammBotNET.Database
{
    public class TagDB : DbContext
    {
        public virtual DbSet<UserTag> UserTag { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            SqliteConnectionStringBuilder cSb = new() { DataSource = "usertags.db" };
            SqliteConnection connection = new(cSb.ToString());
            optionsBuilder.UseSqlite(connection);
        }
    }
}
