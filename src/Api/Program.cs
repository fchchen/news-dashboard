using NewsDashboard.Api.Endpoints;
using NewsDashboard.Api.Middleware;
using NewsDashboard.Api.Services;
using NewsDashboard.Shared.Constants;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "News Dashboard API", Version = "v1" });
});

// Configure Cosmos DB
var cosmosConnectionString = builder.Configuration.GetConnectionString("CosmosDb");
var allowInsecureCosmos = builder.Configuration.GetValue<bool>("CosmosDbAllowInsecure");

if (!string.IsNullOrEmpty(cosmosConnectionString))
{
    builder.Services.AddSingleton(sp =>
    {
        var options = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            }
        };

        if (allowInsecureCosmos)
        {
            options.HttpClientFactory = () =>
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                return new HttpClient(handler);
            };
        }

        return new CosmosClient(cosmosConnectionString, options);
    });
}

// Register HttpClient for external API calls
builder.Services.AddHttpClient<IHackerNewsService, HackerNewsService>();
builder.Services.AddHttpClient<IGitHubReleasesService, GitHubReleasesService>();
builder.Services.AddHttpClient<IRssFeedService, RssFeedService>();

// Register Services
builder.Services.AddScoped<ICosmosDbService, CosmosDbService>();
builder.Services.AddScoped<INewsDashboardService, NewsDashboardService>();

// Background service for periodic fetching
builder.Services.AddHostedService<NewsFetchBackgroundService>();

// Configure CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Initialize Cosmos DB containers in background
if (!string.IsNullOrEmpty(cosmosConnectionString))
{
    _ = Task.Run(async () =>
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var cosmosClient = scope.ServiceProvider.GetRequiredService<CosmosClient>();
            var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(CosmosConstants.Database);

            await database.Database.CreateContainerIfNotExistsAsync(
                new ContainerProperties(CosmosConstants.NewsItemsContainer, "/source")
                {
                    DefaultTimeToLive = 1_209_600 // 14 days
                });

            await database.Database.CreateContainerIfNotExistsAsync(
                new ContainerProperties(CosmosConstants.SnapshotsContainer, "/id")
                {
                    DefaultTimeToLive = 2_592_000 // 30 days
                });

            app.Logger.LogInformation("Cosmos DB initialized successfully");
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Failed to initialize Cosmos DB. Will retry on first request.");
        }
    });
}

// Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Always enable Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAngular");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Map endpoints
app.MapDashboardEndpoints();
app.MapHackerNewsEndpoints();
app.MapGitHubReleasesEndpoints();
app.MapRssFeedEndpoints();

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithTags("Health");

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
