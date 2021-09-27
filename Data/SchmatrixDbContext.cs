using Microsoft.EntityFrameworkCore;
using Status.Models;

namespace Status.Data
{
    public class SchmatrixDbContext : DbContext
    {
        public SchmatrixDbContext(DbContextOptions<SchmatrixDbContext> options) : base(options)
        {
        }

        public DbSet<MuebWithIp> MuebWithIps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MuebWithIp>().HasNoKey().ToTable("mueb_with_ip", t => t.ExcludeFromMigrations());
        }
    }
}