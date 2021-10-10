using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SammBotNET.Database
{
    public class EmotionalSupportDB : DbContext
    {
        public virtual DbSet<EmotionalSupport> EmotionalSupport { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            SqliteConnectionStringBuilder cSb = new SqliteConnectionStringBuilder { DataSource = "emotional.db" };
            SqliteConnection connection = new SqliteConnection(cSb.ToString());
            optionsBuilder.UseSqlite(connection);
        }
    }
}
