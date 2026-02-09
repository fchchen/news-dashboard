using System.Text.Json;
using NewsDashboard.Shared.Constants;
using NewsDashboard.Shared.DTOs;
using NewsDashboard.Shared.Models;

namespace NewsDashboard.Api.Services;

public class GitHubReleasesService : IGitHubReleasesService
{
    private const string BaseUrl = "https://api.github.com";
    private static readonly string SourceName = SourceNames.GitHubRelease;

    private readonly HttpClient _httpClient;
    private readonly ICosmosDbService _cosmosDb;
    private readonly ILogger<GitHubReleasesService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GitHubReleasesService(HttpClient httpClient, ICosmosDbService cosmosDb, ILogger<GitHubReleasesService> logger)
    {
        _httpClient = httpClient;
        _cosmosDb = cosmosDb;
        _logger = logger;

        if (_httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("NewsDashboard/1.0");
            _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        }
    }

    public async Task<IEnumerable<NewsItem>> FetchAllReleasesAsync()
    {
        _logger.LogInformation("Fetching releases for all tracked repos");
        var allItems = new List<NewsItem>();

        foreach (var (owner, repo, company) in TrackedRepos.Repos)
        {
            try
            {
                var items = await FetchReleasesForRepoInternalAsync(owner, repo, company);
                allItems.AddRange(items);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch releases for {Owner}/{Repo}", owner, repo);
            }
        }

        if (allItems.Count > 0)
        {
            await _cosmosDb.UpsertManyNewsItemsAsync(allItems);
        }

        _logger.LogInformation("Fetched {Count} total releases", allItems.Count);
        return allItems;
    }

    public async Task<IEnumerable<NewsItem>> FetchReleasesForRepoAsync(string owner, string repo)
    {
        var repoInfo = TrackedRepos.Repos.FirstOrDefault(r => r.Owner == owner && r.Repo == repo);
        var company = repoInfo != default ? repoInfo.Company : "Other";
        return await FetchReleasesForRepoInternalAsync(owner, repo, company);
    }

    public async Task<IEnumerable<NewsItem>> GetCachedItemsAsync(int page, int pageSize)
    {
        return await _cosmosDb.GetNewsItemsAsync(page, pageSize, SourceName);
    }

    public async Task<int> GetCachedCountAsync()
    {
        return await _cosmosDb.GetNewsItemCountAsync(SourceName);
    }

    public async Task<IEnumerable<NewsItem>> GetCachedByRepoAsync(string owner, string repo, int page, int pageSize)
    {
        var repoFullName = $"{owner}/{repo}";
        var allItems = await _cosmosDb.GetNewsItemsAsync(1, 500, SourceName);
        var filtered = allItems.Where(i => i.Metadata.GetValueOrDefault("repoFullName") == repoFullName);
        return filtered.Skip((page - 1) * pageSize).Take(pageSize);
    }

    public async Task<int> GetCachedByRepoCountAsync(string owner, string repo)
    {
        var repoFullName = $"{owner}/{repo}";
        var allItems = await _cosmosDb.GetNewsItemsAsync(1, 500, SourceNames.GitHubRelease);
        return allItems.Count(i => i.Metadata.GetValueOrDefault("repoFullName") == repoFullName);
    }

    private async Task<List<NewsItem>> FetchReleasesForRepoInternalAsync(string owner, string repo, string company)
    {
        _logger.LogInformation("Fetching releases for {Owner}/{Repo}", owner, repo);

        var url = $"{BaseUrl}/repos/{owner}/{repo}/releases?per_page=10";
        var response = await _httpClient.GetStringAsync(url);
        var releases = JsonSerializer.Deserialize<List<GitHubReleaseApiItem>>(response, JsonOptions) ?? [];

        return releases.Select(r => MapToNewsItem(r, owner, repo, company)).ToList();
    }

    internal static NewsItem MapToNewsItem(GitHubReleaseApiItem release, string owner, string repo, string company)
    {
        var repoFullName = $"{owner}/{repo}";
        var version = release.TagName.TrimStart('v');
        var title = !string.IsNullOrEmpty(release.Name) ? release.Name : $"{repo} {release.TagName}";

        var tags = new List<string> { repo, "release", "cli" };
        tags.AddRange(AiKeywords.GetMatchingTags(title + " " + release.Body));

        return new NewsItem
        {
            ExternalId = $"{repoFullName}@{release.TagName}",
            Source = SourceNames.GitHubRelease,
            Title = title,
            Url = release.HtmlUrl,
            Description = TruncateDescription(release.Body),
            Score = 0,
            Author = release.Author?.Login ?? owner,
            Company = company,
            Tags = tags.Distinct().ToList(),
            PublishedAt = release.PublishedAt,
            FetchedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, string>
            {
                ["version"] = version,
                ["repoFullName"] = repoFullName,
                ["isPreRelease"] = release.Prerelease.ToString().ToLower()
            }
        };
    }

    private static string? TruncateDescription(string? body)
    {
        if (string.IsNullOrEmpty(body)) return null;
        return body.Length > 1000 ? body[..1000] + "..." : body;
    }
}
