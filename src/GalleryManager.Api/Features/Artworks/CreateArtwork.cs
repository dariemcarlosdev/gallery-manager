using FluentValidation;
using GalleryManager.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GalleryManager.Api.Features.Artworks;

public static class CreateArtwork
{
    public record Request(string Title, string Artist, string Medium, decimal Price);

    public record Response(int Id, string Title, string Artist, string Medium, decimal Price, string Status);

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
        app.MapPost("/artworks", async (
            Request request,
            GalleryDbContext db,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var validator = new Validator();
            var validation = await validator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var idempotencyKey = httpContext.Request.Headers["Idempotency-Key"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                var existing = await db.Artworks
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.IdempotencyKey == idempotencyKey, ct);

                if (existing is not null)
                {
                    var existingResponse = new Response(
                        existing.Id, existing.Title, existing.Artist,
                        existing.Medium, existing.Price, existing.Status.ToString());
                    return Results.Ok(existingResponse);
                }
            }

            var artwork = new Artwork
            {
                Title = request.Title,
                Artist = request.Artist,
                Medium = request.Medium,
                Price = request.Price,
                Status = ArtworkStatus.Available,
                IdempotencyKey = idempotencyKey
            };

            db.Artworks.Add(artwork);
            await db.SaveChangesAsync(ct);

            var response = new Response(
                artwork.Id, artwork.Title, artwork.Artist,
                artwork.Medium, artwork.Price, artwork.Status.ToString());
            return Results.Created($"/api/v1/artworks/{artwork.Id}", response);
        })
        .WithName("CreateArtwork")
        .WithTags("Artworks")
        .Produces<Response>(StatusCodes.Status201Created)
        .Produces<Response>(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status429TooManyRequests);
    }
}
