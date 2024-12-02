using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Data.Model;

namespace UrlShortenerService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<UrlMapping> UrlMappings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UrlMapping>()
            .HasIndex(u => u.ShortUrl) // Create an index on ShortUrl for faster lookups
            .IsUnique(); // Ensure ShortUrl is unique

        modelBuilder.Entity<UrlMapping>()
            .HasIndex(u => u.CustomId) // Create an index on CustomId for faster lookups
            .IsUnique(); // Ensure CustomId is unique if provided
    }
}
