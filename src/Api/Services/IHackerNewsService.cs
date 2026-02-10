using NewsDashboard.Shared.Models;

namespace NewsDashboard.Api.Services;

public interface IHackerNewsService
{
    Task<IEnumerable<NewsItem>> FetchAndFilterAsync(int maxStories = 500);
    Task<IEnumerable<NewsItem>> GetCachedItemsAsync(int page, int pageSize);
    Task<int> GetCachedCountAsync();
}
