using System.Collections.Concurrent;
using NewsDashboard.Shared.Models;

namespace NewsDashboard.Api.Services;

public class InMemoryDbService(ILogger<InMemoryDbService> logger) : ICosmosDbService
{
    private readonly ConcurrentDictionary<string, NewsItem> _newsItems = new();
    private readonly object _snapshotLock = new();
    private TrendSnapshot? _latestSnapshot;

    public Task<NewsItem?> GetNewsItemAsync(string id, string source)
    {
        _newsItems.TryGetValue(id, out var item);
        return Task.FromResult(item);
    }

    public Task<NewsItem?> GetByExternalIdAsync(string externalId, string source)
    {
        var item = _newsItems.Values
            .FirstOrDefault(n => n.ExternalId == externalId && n.Source == source);
        return Task.FromResult(item);
    }

    public Task<IEnumerable<NewsItem>> GetNewsItemsAsync(int page, int pageSize, string? source = null, string? company = null)
    {
        var query = _newsItems.Values.AsEnumerable();

        if (!string.IsNullOrEmpty(source))
            query = query.Where(n => n.Source == source);

        if (!string.IsNullOrEmpty(company))
            query = query.Where(n => n.Company == company);

        var results = query
            .OrderByDescending(n => n.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return Task.FromResult(results);
    }

    public Task<int> GetNewsItemCountAsync(string? source = null, string? company = null)
    {
        var query = _newsItems.Values.AsEnumerable();

        if (!string.IsNullOrEmpty(source))
            query = query.Where(n => n.Source == source);

        if (!string.IsNullOrEmpty(company))
            query = query.Where(n => n.Company == company);

        return Task.FromResult(query.Count());
    }

    public Task<NewsItem> UpsertNewsItemAsync(NewsItem item)
    {
        // Deduplicate by ExternalId + Source
        var existing = _newsItems.Values
            .FirstOrDefault(n => n.ExternalId == item.ExternalId && n.Source == item.Source);

        if (existing != null)
        {
            item.Id = existing.Id;
            _newsItems[existing.Id] = item;
        }
        else
        {
            _newsItems[item.Id] = item;
        }

        return Task.FromResult(item);
    }

    public Task UpsertManyNewsItemsAsync(IEnumerable<NewsItem> items)
    {
        foreach (var item in items)
        {
            var existing = _newsItems.Values
                .FirstOrDefault(n => n.ExternalId == item.ExternalId && n.Source == item.Source);

            if (existing != null)
            {
                item.Id = existing.Id;
                _newsItems[existing.Id] = item;
            }
            else
            {
                _newsItems[item.Id] = item;
            }
        }

        logger.LogInformation("Batch upserted {Count} news items (in-memory)", items.Count());
        return Task.CompletedTask;
    }

    public Task<TrendSnapshot?> GetLatestSnapshotAsync()
    {
        lock (_snapshotLock)
        {
            return Task.FromResult(_latestSnapshot);
        }
    }

    public Task<TrendSnapshot> UpsertSnapshotAsync(TrendSnapshot snapshot)
    {
        lock (_snapshotLock)
        {
            _latestSnapshot = snapshot;
        }

        logger.LogInformation("Upserted trend snapshot {Id} (in-memory)", snapshot.Id);
        return Task.FromResult(snapshot);
    }
}
