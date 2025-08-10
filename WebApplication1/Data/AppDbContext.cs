using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<UrlMapping> UrlMappings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // short code must be unique
            modelBuilder.Entity<UrlMapping>()
                .HasIndex(u => u.ShortCode)
                .IsUnique();

            // OPTIONAL: enforce one short link per OriginalUrl (1:1)
            // If you keep this, make sure there are no duplicate OriginalUrl rows
            modelBuilder.Entity<UrlMapping>()
                .HasIndex(u => u.OriginalUrl)
                .IsUnique();
        }
    }
}
