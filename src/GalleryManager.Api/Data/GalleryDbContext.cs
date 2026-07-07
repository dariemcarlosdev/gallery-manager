using GalleryManager.Api.Features.Artworks;
using GalleryManager.Api.Features.Exhibits;
using Microsoft.EntityFrameworkCore;

namespace GalleryManager.Api.Data;

public class GalleryDbContext(DbContextOptions<GalleryDbContext> options) : DbContext(options)
{
    public DbSet<Artwork> Artworks => Set<Artwork>();
    public DbSet<Exhibit> Exhibits => Set<Exhibit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Artwork>(entity =>
        {
            entity.ToTable("artworks");
            entity.Property(a => a.Status).HasConversion<string>();
            entity.Property(a => a.Price).HasColumnType("numeric(10,2)");
        });

        modelBuilder.Entity<Exhibit>(entity =>
        {
            entity.ToTable("exhibits");
        });

        base.OnModelCreating(modelBuilder);
    }
}
