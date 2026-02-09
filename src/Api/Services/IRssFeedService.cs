using NewsDashboard.Shared.DTOs;
using NewsDashboard.Shared.Models;

namespace NewsDashboard.Api.Services;

public interface IRssFeedService
{
    Task<IEnumerable<NewsItem>> FetchAllFeedsAsync();
    Task<IEnumerable<NewsItem>> GetCachedItemsAsync(int page, int pageSize, string? source = null);
    Task<int> GetCachedCountAsync(string? source = null);
    IEnumerable<RssFeedSourceDto> GetAvailableSources();
}
