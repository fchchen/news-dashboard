using NewsDashboard.Shared.Constants;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Add Cosmos DB
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("CosmosDb")
        ?? throw new InvalidOperationException("ConnectionStrings:CosmosDb is required");
    var allowInsecure = configuration.GetValue<bool>("CosmosDbAllowInsecure");

    var options = new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    };

    if (allowInsecure)
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

    return new CosmosClient(connectionString, options);
});

// Register containers
builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<CosmosClient>();
    return client.GetContainer(CosmosConstants.Database, CosmosConstants.NewsItemsContainer);
});

// Register HttpClient
builder.Services.AddHttpClient();

builder.Services.AddApplicationInsightsTelemetryWorkerService();

builder.Build().Run();
