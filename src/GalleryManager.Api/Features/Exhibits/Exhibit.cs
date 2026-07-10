using GalleryManager.Api.Features.Artworks;

namespace GalleryManager.Api.Features.Exhibits;

/// <summary>Domain entity for a gallery exhibit and the artworks assigned to it.</summary>
public class Exhibit
{
    /// <summary>Database-generated primary key.</summary>
    public int Id { get; set; }
    /// <summary>Display name of the exhibit.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Date the exhibit opens.</summary>
    public DateOnly StartDate { get; set; }
    /// <summary>Date the exhibit closes.</summary>
    public DateOnly EndDate { get; set; }
    /// <summary>Navigation collection of artworks assigned to this exhibit.</summary>
    public ICollection<Artwork> Artworks { get; set; } = new List<Artwork>();
}
