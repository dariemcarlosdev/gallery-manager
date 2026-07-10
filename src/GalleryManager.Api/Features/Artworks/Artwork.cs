namespace GalleryManager.Api.Features.Artworks;

/// <summary>Lifecycle state of an artwork within the gallery.</summary>
public enum ArtworkStatus
{
    /// <summary>In the gallery's possession and not committed elsewhere.</summary>
    Available,
    /// <summary>Temporarily loaned out, typically to an exhibit.</summary>
    OnLoan,
    /// <summary>Sold to a buyer; counts toward exhibit revenue.</summary>
    Sold
}

/// <summary>Domain entity representing a single artwork and its persisted state.</summary>
public class Artwork
{
    /// <summary>Database-generated primary key.</summary>
    public int Id { get; set; }
    /// <summary>Display title of the artwork.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Name of the artist who created the work.</summary>
    public string Artist { get; set; } = string.Empty;
    /// <summary>Material or technique (e.g. "Oil on canvas").</summary>
    public string Medium { get; set; } = string.Empty;
    /// <summary>Listed price in the gallery's base currency.</summary>
    public decimal Price { get; set; }
    /// <summary>Current lifecycle state; defaults to <see cref="ArtworkStatus.Available"/>.</summary>
    public ArtworkStatus Status { get; set; } = ArtworkStatus.Available;
    /// <summary>Optional foreign key to the exhibit this artwork is assigned to.</summary>
    public int? ExhibitId { get; set; }
    /// <summary>Client-supplied key used to deduplicate create requests; unique when set.</summary>
    public string? IdempotencyKey { get; set; }
    /// <summary>UTC timestamp set when the record is created.</summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
