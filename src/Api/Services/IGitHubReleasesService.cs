using NewsDashboard.Shared.Models;

namespace NewsDashboard.Api.Services;

public interface IGitHubReleasesService
{
    Task<IEnumerable<NewsItem>> FetchAllReleasesAsync();
    Task<IEnumerable<NewsItem>> FetchReleasesForRepoAsync(string owner, string repo);
    Task<IEnumerable<NewsItem>> GetCachedItemsAsync(int page, int pageSize);
    Task<int> GetCachedCountAsync();
    Task<IEnumerable<NewsItem>> GetCachedByRepoAsync(string owner, string repo, int page, int pageSize);
    Task<int> GetCachedByRepoCountAsync(string owner, string repo);
}
