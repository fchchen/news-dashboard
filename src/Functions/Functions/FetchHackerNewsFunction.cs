using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NewsDashboard.Shared.Constants;
using NewsDashboard.Shared.DTOs;
using NewsDashboard.Shared.Models;

namespace NewsDashboard.Functions.Functions;

public class FetchHackerNewsFunction(Container container, IHttpClientFactory httpClientFactory, ILogger<FetchHackerNewsFunction> logger)
{
    private const string BaseUrl = "https://hacker-news.firebaseio.com/v0";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Function(nameof(FetchHackerNews))]
    public async Task FetchHackerNews([TimerTrigger("0 */15 * * * *")] TimerInfo timerInfo)
    {
        logger.LogInformation("FetchHackerNews triggered at {Time}", DateTime.UtcNow);

        var httpClient = httpClientFactory.CreateClient();
        var topStoriesJson = await httpClient.GetStringAsync($"{BaseUrl}/topstories.json");
        var topIds = JsonSerializer.Deserialize<List<int>>(topStoriesJson) ?? [];

        var storyTasks = topIds.Take(100).Select(async id =>
        {
            try
            {
                var json = await httpClient.GetStringAsync($"{BaseUrl}/item/{id}.json");
                return JsonSerializer.Deserialize<HackerNewsApiItem>(json, JsonOptions);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to fetch HN story {Id}", id);
                return null;
            }
        });

        var stories = await Task.WhenAll(storyTasks);

        var filtered = stories
            .Where(s => s is not null && AiKeywords.MatchesAny($"{s.Title} {s.Url}"))
            .ToList();

        logger.LogInformation("Found {Count} AI-related HN stories", filtered.Count);

        foreach (var story in filtered)
        {
            var combinedText = $"{story!.Title} {story.Url}";
            var item = new NewsItem
            {
                ExternalId = $"hn-{story.Id}",
                Source = SourceNames.HackerNews,
                Title = story.Title ?? string.Empty,
                Url = story.Url ?? $"https://news.ycombinator.com/item?id={story.Id}",
                Score = story.Score,
                Author = story.By,
                Company = AiKeywords.DetectCompany(combinedText),
                Tags = AiKeywords.GetMatchingTags(combinedText),
                PublishedAt = DateTimeOffset.FromUnixTimeSeconds(story.Time).UtcDateTime,
                FetchedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>
                {
                    ["commentCount"] = story.Descendants.ToString(),
                    ["hnUrl"] = $"https://news.ycombinator.com/item?id={story.Id}"
                }
            };

            await container.UpsertItemAsync(item, new PartitionKey(item.Source));
        }

        logger.LogInformation("FetchHackerNews completed. Upserted {Count} items", filtered.Count);
    }
}
