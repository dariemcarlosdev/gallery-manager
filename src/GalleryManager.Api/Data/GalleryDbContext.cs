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
            entity.Property(a => a.IdempotencyKey).HasMaxLength(64);
            entity.HasIndex(a => a.IdempotencyKey)
                .IsUnique()
                .HasFilter("\"IdempotencyKey\" IS NOT NULL");
        });

        modelBuilder.Entity<Exhibit>(entity =>
        {
            entity.ToTable("exhibits");
            entity.HasMany(e => e.Artworks)
                .WithOne()
                .HasForeignKey(a => a.ExhibitId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        base.OnModelCreating(modelBuilder);
    }
}
