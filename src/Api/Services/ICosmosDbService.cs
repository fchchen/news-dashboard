using NewsDashboard.Shared.Models;

namespace NewsDashboard.Api.Services;

public interface ICosmosDbService
{
    Task<NewsItem?> GetNewsItemAsync(string id, string source);
    Task<NewsItem?> GetByExternalIdAsync(string externalId, string source);
    Task<IEnumerable<NewsItem>> GetNewsItemsAsync(int page, int pageSize, string? source = null, string? company = null);
    Task<int> GetNewsItemCountAsync(string? source = null, string? company = null);
    Task<NewsItem> UpsertNewsItemAsync(NewsItem item);
    Task UpsertManyNewsItemsAsync(IEnumerable<NewsItem> items);
    Task<TrendSnapshot?> GetLatestSnapshotAsync();
    Task<TrendSnapshot> UpsertSnapshotAsync(TrendSnapshot snapshot);
}
