namespace GalleryManager.Api.Features.Artworks;

public enum ArtworkStatus
{
    Available,
    OnLoan,
    Sold
}

public class Artwork
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Medium { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ArtworkStatus Status { get; set; } = ArtworkStatus.Available;
    public int? ExhibitId { get; set; }
    public string? IdempotencyKey { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
