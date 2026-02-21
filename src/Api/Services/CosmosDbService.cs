using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using NewsDashboard.Shared.Constants;
using NewsDashboard.Shared.Models;

namespace NewsDashboard.Api.Services;

public class CosmosDbService(CosmosClient cosmosClient, ILogger<CosmosDbService> logger) : ICosmosDbService
{
    private static readonly SemaphoreSlim UpsertThrottle = new(10, 10);

    private readonly Container _newsContainer = cosmosClient.GetContainer(CosmosConstants.Database, CosmosConstants.NewsItemsContainer);
    private readonly Container _snapshotsContainer = cosmosClient.GetContainer(CosmosConstants.Database, CosmosConstants.SnapshotsContainer);

    public async Task<NewsItem?> GetNewsItemAsync(string id, string source)
    {
        try
        {
            var response = await _newsContainer.ReadItemAsync<NewsItem>(id, new PartitionKey(source));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<NewsItem?> GetByExternalIdAsync(string externalId, string source)
    {
        var queryOptions = new QueryRequestOptions { PartitionKey = new PartitionKey(source) };
        var query = _newsContainer.GetItemLinqQueryable<NewsItem>(requestOptions: queryOptions)
            .Where(n => n.ExternalId == externalId)
            .Take(1)
            .ToFeedIterator();

        if (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            return response.FirstOrDefault();
        }

        return null;
    }

    public async Task<IEnumerable<NewsItem>> GetNewsItemsAsync(int page, int pageSize, string? source = null, string? company = null)
    {
        QueryRequestOptions? options = null;
        if (!string.IsNullOrEmpty(source))
        {
            options = new QueryRequestOptions { PartitionKey = new PartitionKey(source) };
        }

        var queryable = _newsContainer.GetItemLinqQueryable<NewsItem>(requestOptions: options);
        IQueryable<NewsItem> filtered = queryable;

        if (!string.IsNullOrEmpty(source))
            filtered = filtered.Where(n => n.Source == source);

        if (!string.IsNullOrEmpty(company))
            filtered = filtered.Where(n => n.Company == company);

        var query = filtered
            .OrderByDescending(n => n.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToFeedIterator();

        var results = new List<NewsItem>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    public async Task<int> GetNewsItemCountAsync(string? source = null, string? company = null)
    {
        QueryRequestOptions? options = null;
        if (!string.IsNullOrEmpty(source))
        {
            options = new QueryRequestOptions { PartitionKey = new PartitionKey(source) };
        }

        var queryable = _newsContainer.GetItemLinqQueryable<NewsItem>(requestOptions: options);
        IQueryable<NewsItem> filtered = queryable;

        if (!string.IsNullOrEmpty(source))
            filtered = filtered.Where(n => n.Source == source);

        if (!string.IsNullOrEmpty(company))
            filtered = filtered.Where(n => n.Company == company);

        return await filtered.CountAsync();
    }

    public async Task<NewsItem> UpsertNewsItemAsync(NewsItem item)
    {
        var response = await _newsContainer.UpsertItemAsync(item, new PartitionKey(item.Source));
        return response.Resource;
    }

    public async Task UpsertManyNewsItemsAsync(IEnumerable<NewsItem> items)
    {
        var tasks = items.Select(async item =>
        {
            await UpsertThrottle.WaitAsync();
            try
            {
                await UpsertNewsItemAsync(item);
            }
            finally
            {
                UpsertThrottle.Release();
            }
        });
        await Task.WhenAll(tasks);
        logger.LogInformation("Batch upserted news items");
    }

    public async Task<TrendSnapshot?> GetLatestSnapshotAsync()
    {
        var query = _snapshotsContainer.GetItemLinqQueryable<TrendSnapshot>()
            .OrderByDescending(s => s.Timestamp)
            .Take(1)
            .ToFeedIterator();

        if (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            return response.FirstOrDefault();
        }

        return null;
    }

    public async Task<TrendSnapshot> UpsertSnapshotAsync(TrendSnapshot snapshot)
    {
        var response = await _snapshotsContainer.UpsertItemAsync(snapshot, new PartitionKey(snapshot.Id));
        logger.LogInformation("Upserted trend snapshot {Id}", snapshot.Id);
        return response.Resource;
    }
}
