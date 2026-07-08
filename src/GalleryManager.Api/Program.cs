using System.Threading.RateLimiting;
using Asp.Versioning;
using GalleryManager.Api.Data;
using GalleryManager.Api.Features.Artworks;
using GalleryManager.Api.Features.Exhibits;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (port is not null)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddDbContext<GalleryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("GalleryDb")));

builder.Services.AddProblemDetails();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

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

builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.NoCache());
    options.AddPolicy("Short", builder => builder.Expire(TimeSpan.FromSeconds(30)).SetVaryByQuery("*"));
});

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
                "http://localhost:4301",
                "http://localhost:4302",
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
app.UseRateLimiter();
app.UseOutputCache();

var versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .Build();

var v1 = app.MapGroup("/api/v{version:apiVersion}")
    .WithApiVersionSet(versionSet)
    .MapToApiVersion(new ApiVersion(1, 0))
    .RequireRateLimiting("fixed");

GetArtworks.MapEndpoint(v1);
CreateArtwork.MapEndpoint(v1);
UpdateArtworkStatus.MapEndpoint(v1);

GetExhibits.MapEndpoint(v1);
AssignArtworkToExhibit.MapEndpoint(v1);
GetExhibitRevenue.MapEndpoint(v1);

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithTags("Health");

app.Run();
