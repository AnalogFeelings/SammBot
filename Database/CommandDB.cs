using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SammBotNET.Database
{
    public class CommandDB : DbContext
    {
        public virtual DbSet<CustomCommand> CustomCommand { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            SqliteConnectionStringBuilder cSb = new() { DataSource = "customcmds.db" };
            SqliteConnection connection = new(cSb.ToString());
            optionsBuilder.UseSqlite(connection);
        }
    }
}
