using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SammBotNET.Database
{
    public class PhrasesDB : DbContext
    {
        public virtual DbSet<Phrase> Phrase { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            SqliteConnectionStringBuilder cSb = new SqliteConnectionStringBuilder { DataSource = "phrases.db" };
            SqliteConnection connection = new SqliteConnection(cSb.ToString());
            optionsBuilder.UseSqlite(connection);
        }
    }
}
