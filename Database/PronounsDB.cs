using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SammBotNET.Database
{
    public class PronounsDB : DbContext
    {
        public virtual DbSet<Pronoun> Pronouns { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            SqliteConnectionStringBuilder cSb = new() { DataSource = "pronouns.db" };
            SqliteConnection connection = new(cSb.ToString());
            optionsBuilder.UseSqlite(connection);
        }
    }
}
