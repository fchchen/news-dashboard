namespace NewsDashboard.Shared.Models;

public class NewsItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ExternalId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Score { get; set; }
    public string? Author { get; set; }
    public string Company { get; set; } = "Other";
    public List<string> Tags { get; set; } = [];
    public DateTime PublishedAt { get; set; }
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// TTL in seconds. 14 days = 1209600 seconds.
    /// </summary>
    public int Ttl { get; set; } = 1_209_600;
}
