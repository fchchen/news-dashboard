namespace NewsDashboard.Shared.Models;

public class TrendSnapshot
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int HackerNewsCount { get; set; }
    public int GitHubReleaseCount { get; set; }
    public int RssArticleCount { get; set; }
    public int AnthropicItemCount { get; set; }
    public int OpenAiItemCount { get; set; }
    public int GoogleItemCount { get; set; }
    public List<TrendingTopic> TrendingTopics { get; set; } = [];

    /// <summary>
    /// TTL in seconds. 30 days = 2592000 seconds.
    /// </summary>
    public int Ttl { get; set; } = 2_592_000;
}

public class TrendingTopic
{
    public string Topic { get; set; } = string.Empty;
    public int MentionCount { get; set; }
}
