using Microsoft.EntityFrameworkCore;

namespace EfCore.NestedSets.Tests
{
    public class AppDbContext : DbContext
    {
        public DbSet<Node> Nodes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // define the database to use
            optionsBuilder.UseSqlServer(DbSql.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Node>()
                .Ignore(b => b.Moving);
        }
    }

}