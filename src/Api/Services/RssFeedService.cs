using System.ServiceModel.Syndication;
using System.Xml;
using NewsDashboard.Shared.Constants;
using NewsDashboard.Shared.DTOs;
using NewsDashboard.Shared.Models;

namespace NewsDashboard.Api.Services;

public class RssFeedService : IRssFeedService
{
    private readonly HttpClient _httpClient;
    private readonly ICosmosDbService _cosmosDb;
    private readonly ILogger<RssFeedService> _logger;

    public RssFeedService(HttpClient httpClient, ICosmosDbService cosmosDb, ILogger<RssFeedService> logger)
    {
        _httpClient = httpClient;
        _cosmosDb = cosmosDb;
        _logger = logger;
    }

    public async Task<IEnumerable<NewsItem>> FetchAllFeedsAsync()
    {
        _logger.LogInformation("Fetching all RSS feeds");
        var allItems = new List<NewsItem>();

        foreach (var (feedName, feedSource) in FeedSources.Feeds)
        {
            try
            {
                var items = await FetchFeedAsync(feedName, feedSource);
                allItems.AddRange(items);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch RSS feed: {FeedName}", feedName);
            }
        }

        if (allItems.Count > 0)
        {
            await _cosmosDb.UpsertManyNewsItemsAsync(allItems);
        }

        _logger.LogInformation("Fetched {Count} total RSS items", allItems.Count);
        return allItems;
    }

    public async Task<IEnumerable<NewsItem>> GetCachedItemsAsync(int page, int pageSize, string? feedName = null)
    {
        // When feedName filter is provided, fetch a larger window and filter in-memory
        // (feedName is stored in metadata, not a Cosmos-queryable field)
        if (!string.IsNullOrEmpty(feedName))
        {
            var allItems = await _cosmosDb.GetNewsItemsAsync(1, 500, SourceNames.RssFeed);
            var filtered = allItems.Where(i => i.Metadata.GetValueOrDefault("feedName") == feedName);
            return filtered.Skip((page - 1) * pageSize).Take(pageSize);
        }

        return await _cosmosDb.GetNewsItemsAsync(page, pageSize, SourceNames.RssFeed);
    }

    public async Task<int> GetCachedCountAsync(string? feedName = null)
    {
        if (!string.IsNullOrEmpty(feedName))
        {
            var allItems = await _cosmosDb.GetNewsItemsAsync(1, 500, SourceNames.RssFeed);
            return allItems.Count(i => i.Metadata.GetValueOrDefault("feedName") == feedName);
        }

        return await _cosmosDb.GetNewsItemCountAsync(SourceNames.RssFeed);
    }

    public IEnumerable<RssFeedSourceDto> GetAvailableSources()
    {
        return FeedSources.Feeds.Select(f => new RssFeedSourceDto(f.Key, f.Value.Url, f.Value.Company, f.Value.FilterRequired));
    }

    private async Task<List<NewsItem>> FetchFeedAsync(string feedName, FeedSource feedSource)
    {
        _logger.LogInformation("Fetching RSS feed: {FeedName}", feedName);

        var stream = await _httpClient.GetStreamAsync(feedSource.Url);
        using var reader = XmlReader.Create(stream);
        var feed = SyndicationFeed.Load(reader);

        var items = feed.Items
            .Select(item => MapToNewsItem(item, feedName, feedSource))
            .Where(item =>
            {
                if (!feedSource.FilterRequired) return true;
                return AiKeywords.MatchesAny(item.Title) || AiKeywords.MatchesAny(item.Description ?? "");
            })
            .ToList();

        _logger.LogInformation("Fetched {Count} items from {FeedName}", items.Count, feedName);
        return items;
    }

    internal static NewsItem MapToNewsItem(SyndicationItem item, string feedName, FeedSource feedSource)
    {
        var title = item.Title?.Text ?? string.Empty;
        var link = item.Links.FirstOrDefault()?.Uri?.ToString() ?? string.Empty;
        var description = item.Summary?.Text;
        var combinedText = $"{title} {description}";

        var company = feedSource.Company != "Various"
            ? feedSource.Company
            : AiKeywords.DetectCompany(combinedText);

        var tags = AiKeywords.GetMatchingTags(combinedText);
        tags.Add(feedName.ToLowerInvariant().Replace(" ", "-"));

        return new NewsItem
        {
            ExternalId = $"rss-{GenerateStableId(link, title)}",
            Source = SourceNames.RssFeed,
            Title = title,
            Url = link,
            Description = TruncateDescription(description),
            Score = 0,
            Author = item.Authors.FirstOrDefault()?.Name,
            Company = company,
            Tags = tags.Distinct().ToList(),
            PublishedAt = item.PublishDate != default
                ? item.PublishDate.UtcDateTime
                : DateTime.UtcNow,
            FetchedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, string>
            {
                ["feedName"] = feedName,
                ["feedUrl"] = feedSource.Url
            }
        };
    }

    internal static string GenerateStableId(string url, string title)
    {
        var input = !string.IsNullOrEmpty(url) ? url : title;
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash)[..16].ToLower();
    }

    private static string? TruncateDescription(string? text)
    {
        if (string.IsNullOrEmpty(text)) return null;
        var stripped = System.Text.RegularExpressions.Regex.Replace(text, "<[^>]+>", " ").Trim();
        return stripped.Length > 500 ? stripped[..500] + "..." : stripped;
    }
}
