using System.Threading.RateLimiting;
using Asp.Versioning;
using GalleryManager.Api.Data;
using GalleryManager.Api.Features.Artworks;
using GalleryManager.Api.Features.Exhibits;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Bind to the host-provided PORT when present (e.g. Render/containers); otherwise use launch defaults.
var port = Environment.GetEnvironmentVariable("PORT");
if (port is not null)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// EF Core context backed by PostgreSQL (Npgsql), connection string "GalleryDb".
builder.Services.AddDbContext<GalleryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("GalleryDb" +
    "")));

// Standardized RFC 7807 problem-details responses for errors.
builder.Services.AddProblemDetails();

// URL-segment API versioning (/api/v1/...); assumes v1.0 when unspecified and advertises supported versions.
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// Fixed-window rate limiter: 100 requests/minute, replying 429 with a Retry-After header on rejection.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await Results.Problem(
            detail: "Too many requests. Please try again later.",
            statusCode: StatusCodes.Status429TooManyRequests,
            title: "Rate limit exceeded")
            .ExecuteAsync(context.HttpContext);
    };
});

// Output caching: no-cache by default; "Short" policy caches 30s, varying by all query params.
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.NoCache());
    options.AddPolicy("Short", builder => builder.Expire(TimeSpan.FromSeconds(30)).SetVaryByQuery("*"));
});

// Minimal-API endpoint discovery + Swagger/OpenAPI generation (UI enabled in Development only).
builder.Services.AddEndpointsApiExplorer();
// Vertical slices reuse nested type names (e.g. Response) across features;
// qualify schemaIds by declaring type to avoid Swashbuckle collisions.
builder.Services.AddSwaggerGen(c => c.CustomSchemaIds(t => t.FullName?.Replace("+", ".")));

// CORS policy allowing the Angular front-end (local dev ports + the configured Vercel URL).
const string AngularDevCors = "AngularDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevCors, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",
                "http://localhost:4301",
                "http://localhost:4302",
                builder.Configuration["Frontend:VercelUrl"] ?? "https://placeholder.vercel.app")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Swagger UI only in Development to avoid exposing the API surface in production.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Honor X-Forwarded-For/Proto so scheme and client IP are correct behind a reverse proxy.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Middleware pipeline order: CORS, HTTPS redirect, rate limiting, then output caching.
app.UseCors(AngularDevCors);
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseOutputCache();

// Version set + /api/v{version} route group; all feature endpoints hang off v1 with rate limiting applied.
var versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .Build();

var v1 = app.MapGroup("/api/v{version:apiVersion}")
    .WithApiVersionSet(versionSet)
    .MapToApiVersion(new ApiVersion(1, 0))
    .RequireRateLimiting("fixed");

// Register each vertical-slice feature endpoint onto the v1 group.
GetArtworks.MapEndpoint(v1);
CreateArtwork.MapEndpoint(v1);
UpdateArtworkStatus.MapEndpoint(v1);

GetExhibits.MapEndpoint(v1);
AssignArtworkToExhibit.MapEndpoint(v1);
GetExhibitRevenue.MapEndpoint(v1);

// Unversioned liveness probe for platform health checks.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithTags("Health");

app.Run();
