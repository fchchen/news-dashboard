using System.Text.Json;
using NewsDashboard.Shared.Constants;
using NewsDashboard.Shared.DTOs;
using NewsDashboard.Shared.Models;

namespace NewsDashboard.Api.Services;

public class HackerNewsService(HttpClient httpClient, ICosmosDbService cosmosDb, ILogger<HackerNewsService> logger) : IHackerNewsService
{
    private const string BaseUrl = "https://hacker-news.firebaseio.com/v0";
    private static readonly string SourceName = SourceNames.HackerNews;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IEnumerable<NewsItem>> FetchAndFilterAsync(int maxStories = 500)
    {
        logger.LogInformation("Fetching top stories from Hacker News");

        var topStoryIds = await FetchTopStoryIdsAsync(maxStories);
        logger.LogInformation("Fetched {Count} top story IDs", topStoryIds.Count);

        var fetchTasks = topStoryIds.Select(FetchStoryAsync);
        var stories = await Task.WhenAll(fetchTasks);

        var filtered = stories
            .Where(s => s is not null)
            .Where(s => AiKeywords.MatchesAny(s!.Title ?? "") || AiKeywords.MatchesAny(s!.Url ?? ""))
            .Select(s => MapToNewsItem(s!))
            .ToList();

        logger.LogInformation("Filtered to {Count} AI-related stories", filtered.Count);

        if (filtered.Count > 0)
        {
            await cosmosDb.UpsertManyNewsItemsAsync(filtered);
        }

        return filtered;
    }

    public async Task<IEnumerable<NewsItem>> GetCachedItemsAsync(int page, int pageSize)
    {
        return await cosmosDb.GetNewsItemsAsync(page, pageSize, SourceName);
    }

    public async Task<int> GetCachedCountAsync()
    {
        return await cosmosDb.GetNewsItemCountAsync(SourceName);
    }

    private async Task<List<int>> FetchTopStoryIdsAsync(int max)
    {
        var response = await httpClient.GetStringAsync($"{BaseUrl}/topstories.json");
        var ids = JsonSerializer.Deserialize<List<int>>(response) ?? [];
        return ids.Take(max).ToList();
    }

    private async Task<HackerNewsApiItem?> FetchStoryAsync(int id)
    {
        try
        {
            var response = await httpClient.GetStringAsync($"{BaseUrl}/item/{id}.json");
            return JsonSerializer.Deserialize<HackerNewsApiItem>(response, JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch HN story {Id}", id);
            return null;
        }
    }

    internal static NewsItem MapToNewsItem(HackerNewsApiItem item)
    {
        var combinedText = $"{item.Title} {item.Url}";
        var tags = AiKeywords.GetMatchingTags(combinedText);
        var company = AiKeywords.DetectCompany(combinedText);

        return new NewsItem
        {
            ExternalId = $"hn-{item.Id}",
            Source = SourceName,
            Title = item.Title ?? string.Empty,
            Url = item.Url ?? $"https://news.ycombinator.com/item?id={item.Id}",
            Score = item.Score,
            Author = item.By,
            Company = company,
            Tags = tags,
            PublishedAt = DateTimeOffset.FromUnixTimeSeconds(item.Time).UtcDateTime,
            FetchedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, string>
            {
                ["commentCount"] = item.Descendants.ToString(),
                ["hnUrl"] = $"https://news.ycombinator.com/item?id={item.Id}"
            }
        };
    }
}
