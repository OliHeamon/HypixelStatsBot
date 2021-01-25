using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HypixelStatsBot.Database
{
    public class PlayerStorage : DbContext
    {
        public DbSet<AccountData> AccountData { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            SqliteConnectionStringBuilder connectionStringBuilder = new SqliteConnectionStringBuilder 
            { 
                DataSource = "players.db" 
            };

            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);

            optionsBuilder.UseSqlite(connection);
        }
    }
}
