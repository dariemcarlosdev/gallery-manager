using FluentValidation;
using GalleryManager.Api.Data;

namespace GalleryManager.Api.Features.Artworks;

public static class CreateArtwork
{
    public record Request(string Title, string Artist, string Medium, decimal Price);

    public record Response(int Id, string Title, string Status);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Artist).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Medium).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/artworks", async (
            Request request,
            GalleryDbContext db,
            CancellationToken ct) =>
        {
            var validator = new Validator();
            var validation = await validator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var artwork = new Artwork
            {
                Title = request.Title,
                Artist = request.Artist,
                Medium = request.Medium,
                Price = request.Price,
                Status = ArtworkStatus.Available
            };

            db.Artworks.Add(artwork);
            await db.SaveChangesAsync(ct);

            var response = new Response(artwork.Id, artwork.Title, artwork.Status.ToString());
            return Results.Created($"/api/artworks/{artwork.Id}", response);
        })
        .WithName("CreateArtwork")
        .WithTags("Artworks")
        .Produces<Response>(StatusCodes.Status201Created)
        .ProducesValidationProblem();
    }
}
