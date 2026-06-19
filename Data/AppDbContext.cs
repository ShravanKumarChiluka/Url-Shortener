using UrlShortener.Models;
using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Data
{
    public class AppDbContext :DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User>Users { get; set; }
        public DbSet<ShortUrl> ShortUrls { get; set; }
        public DbSet<Click> Clicks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShortUrl>()
                .HasIndex(s => s.ShortCode)
                .IsUnique();

            modelBuilder.Entity<ShortUrl>()
                .HasIndex(s => s.CustomAlias)
                .IsUnique();
        }
    }
}
