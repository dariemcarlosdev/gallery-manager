using GalleryManager.Api.Data;
using GalleryManager.Api.Features.Artworks;
using GalleryManager.Api.Features.Exhibits;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (port is not null)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddDbContext<GalleryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("GalleryDb")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string AngularDevCors = "AngularDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevCors, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",
                builder.Configuration["Frontend:VercelUrl"] ?? "https://placeholder.vercel.app")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseCors(AngularDevCors);
app.UseHttpsRedirection();

// Vertical slice endpoint registration - each feature owns its own MapEndpoint.
GetArtworks.MapEndpoint(app);
CreateArtwork.MapEndpoint(app);
UpdateArtworkStatus.MapEndpoint(app);

GetExhibits.MapEndpoint(app);
AssignArtworkToExhibit.MapEndpoint(app);
GetExhibitRevenue.MapEndpoint(app);

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithTags("Health");

app.Run();
