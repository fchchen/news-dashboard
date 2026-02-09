using NewsDashboard.Shared.Constants;
using NewsDashboard.Shared.DTOs;
using NewsDashboard.Shared.Models;

namespace NewsDashboard.Api.Services;

public class NewsDashboardService : INewsDashboardService
{
    private readonly IHackerNewsService _hackerNews;
    private readonly IGitHubReleasesService _gitHubReleases;
    private readonly IRssFeedService _rssFeed;
    private readonly ICosmosDbService _cosmosDb;
    private readonly ILogger<NewsDashboardService> _logger;

    public NewsDashboardService(
        IHackerNewsService hackerNews,
        IGitHubReleasesService gitHubReleases,
        IRssFeedService rssFeed,
        ICosmosDbService cosmosDb,
        ILogger<NewsDashboardService> logger)
    {
        _hackerNews = hackerNews;
        _gitHubReleases = gitHubReleases;
        _rssFeed = rssFeed;
        _cosmosDb = cosmosDb;
        _logger = logger;
    }

    public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync()
    {
        var hnCount = await _cosmosDb.GetNewsItemCountAsync(SourceNames.HackerNews);
        var ghCount = await _cosmosDb.GetNewsItemCountAsync(SourceNames.GitHubRelease);
        var rssCount = await _cosmosDb.GetNewsItemCountAsync(SourceNames.RssFeed);
        var anthropicCount = await _cosmosDb.GetNewsItemCountAsync(company: "Anthropic");
        var openAiCount = await _cosmosDb.GetNewsItemCountAsync(company: "OpenAI");

        var stats = new DashboardStats(hnCount, ghCount, rssCount, hnCount + ghCount + rssCount);

        var companies = new List<CompanyBreakdown>
        {
            new("Anthropic", anthropicCount),
            new("OpenAI", openAiCount)
        };

        // Get hot HN items (highest score)
        var hotItems = (await _cosmosDb.GetNewsItemsAsync(1, 5, SourceNames.HackerNews))
            .OrderByDescending(i => i.Score)
            .Select(MapToDto)
            .ToList();

        // Get latest releases
        var latestReleases = (await _cosmosDb.GetNewsItemsAsync(1, 4, SourceNames.GitHubRelease))
            .Select(MapToDto)
            .ToList();

        // Get latest blog posts
        var latestBlogs = (await _cosmosDb.GetNewsItemsAsync(1, 4, SourceNames.RssFeed))
            .Select(MapToDto)
            .ToList();

        // Compute trending topics
        var allRecent = await _cosmosDb.GetNewsItemsAsync(1, 50);
        var trendingTopics = ComputeTrendingTopics(allRecent);

        var lastSnapshot = await _cosmosDb.GetLatestSnapshotAsync();

        return new DashboardSummaryResponse(
            stats,
            companies,
            trendingTopics,
            hotItems,
            latestReleases,
            latestBlogs,
            lastSnapshot?.Timestamp
        );
    }

    public async Task<TrendsResponse> GetTrendsAsync()
    {
        var allRecent = await _cosmosDb.GetNewsItemsAsync(1, 100);
        var itemsList = allRecent.ToList();

        var topics = ComputeTrendingTopics(itemsList);

        var companyDistribution = itemsList
            .GroupBy(i => i.Company)
            .Select(g => new CompanyBreakdown(g.Key, g.Count()))
            .OrderByDescending(c => c.ItemCount)
            .ToList();

        var sourceDistribution = itemsList
            .GroupBy(i => i.Source)
            .Select(g => new SourceBreakdown(g.Key, g.Count()))
            .OrderByDescending(s => s.Count)
            .ToList();

        return new TrendsResponse(topics, companyDistribution, sourceDistribution);
    }

    public async Task<PagedResponse<NewsItemDto>> GetUnifiedFeedAsync(int page, int pageSize, string? source = null, string? company = null)
    {
        var items = await _cosmosDb.GetNewsItemsAsync(page, pageSize, source, company);
        var totalCount = await _cosmosDb.GetNewsItemCountAsync(source, company);

        return new PagedResponse<NewsItemDto>(
            items.Select(MapToDto),
            totalCount,
            page,
            pageSize
        );
    }

    public async Task RefreshAllSourcesAsync()
    {
        _logger.LogInformation("Refreshing all news sources");

        var tasks = new List<Task>
        {
            _hackerNews.FetchAndFilterAsync(),
            _gitHubReleases.FetchAllReleasesAsync(),
            _rssFeed.FetchAllFeedsAsync()
        };

        await Task.WhenAll(tasks);

        // Update snapshot
        var hnCount = await _cosmosDb.GetNewsItemCountAsync(SourceNames.HackerNews);
        var ghCount = await _cosmosDb.GetNewsItemCountAsync(SourceNames.GitHubRelease);
        var rssCount = await _cosmosDb.GetNewsItemCountAsync(SourceNames.RssFeed);
        var anthropicCount = await _cosmosDb.GetNewsItemCountAsync(company: "Anthropic");
        var openAiCount = await _cosmosDb.GetNewsItemCountAsync(company: "OpenAI");

        var allRecent = await _cosmosDb.GetNewsItemsAsync(1, 100);
        var trendingTopics = ComputeTrendingTopics(allRecent)
            .Select(t => new TrendingTopic { Topic = t.Topic, MentionCount = t.MentionCount })
            .ToList();

        var snapshot = new TrendSnapshot
        {
            HackerNewsCount = hnCount,
            GitHubReleaseCount = ghCount,
            RssArticleCount = rssCount,
            AnthropicItemCount = anthropicCount,
            OpenAiItemCount = openAiCount,
            TrendingTopics = trendingTopics
        };

        await _cosmosDb.UpsertSnapshotAsync(snapshot);
        _logger.LogInformation("All sources refreshed successfully");
    }

    private static List<TrendingTopicDto> ComputeTrendingTopics(IEnumerable<NewsItem> items)
    {
        return items
            .SelectMany(i => i.Tags)
            .GroupBy(t => t)
            .Select(g => new TrendingTopicDto(g.Key, g.Count()))
            .OrderByDescending(t => t.MentionCount)
            .Take(10)
            .ToList();
    }

    private static NewsItemDto MapToDto(NewsItem item)
    {
        return new NewsItemDto(
            item.Id,
            item.ExternalId,
            item.Source,
            item.Title,
            item.Url,
            item.Description,
            item.Score,
            item.Author,
            item.Company,
            item.Tags,
            item.PublishedAt,
            item.Metadata
        );
    }
}
