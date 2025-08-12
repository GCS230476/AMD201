using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class AppDbContext : DbContext
    {
        // ✅ DI needs this
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<UrlMapping> UrlMappings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UrlMapping>(e =>
            {
                e.Property(x => x.OriginalUrl)
                    .HasMaxLength(2048)
                    .IsRequired();

                e.Property(x => x.ShortCode)
                    .HasMaxLength(30)
                    .IsRequired();

                e.Property(x => x.CreatedAt)
                    .IsRequired();

                // Indexes
                e.HasIndex(x => x.ShortCode).IsUnique();
                e.HasIndex(x => x.OriginalUrl).IsUnique(); // 1:1 mapping
            });
        }

    }
}
