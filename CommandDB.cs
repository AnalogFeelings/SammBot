using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SammBotNET.Database
{
    public class CommandDB : DbContext
    {
        public virtual DbSet<CustomCommand> CustomCommand { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            SqliteConnectionStringBuilder cSb = new SqliteConnectionStringBuilder { DataSource = "customcmds.db" };
            SqliteConnection connection = new SqliteConnection(cSb.ToString());
            optionsBuilder.UseSqlite(connection);
        }
    }
}
