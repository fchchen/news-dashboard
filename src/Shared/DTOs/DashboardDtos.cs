namespace NewsDashboard.Shared.DTOs;

public record DashboardSummaryResponse(
    DashboardStats Stats,
    List<CompanyBreakdown> Companies,
    List<TrendingTopicDto> TrendingTopics,
    List<NewsItemDto> HotItems,
    List<NewsItemDto> LatestReleases,
    List<NewsItemDto> LatestBlogPosts,
    DateTime? LastFetchedAt
);

public record DashboardStats(
    int HackerNewsCount,
    int GitHubReleaseCount,
    int RssArticleCount,
    int TotalCount
);

public record CompanyBreakdown(
    string Company,
    int ItemCount
);

public record TrendingTopicDto(
    string Topic,
    int MentionCount
);

public record NewsItemDto(
    string Id,
    string ExternalId,
    string Source,
    string Title,
    string Url,
    string? Description,
    int Score,
    string? Author,
    string Company,
    List<string> Tags,
    DateTime PublishedAt,
    Dictionary<string, string> Metadata
);

public record PagedResponse<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record TrendsResponse(
    List<TrendingTopicDto> Topics,
    List<CompanyBreakdown> CompanyDistribution,
    List<SourceBreakdown> SourceDistribution
);

public record SourceBreakdown(
    string Source,
    int Count
);
