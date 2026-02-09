using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NewsDashboard.Shared.Constants;
using NewsDashboard.Shared.Models;

namespace NewsDashboard.Functions.Functions;

public class FetchRssFeedsFunction
{
    private readonly Container _container;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FetchRssFeedsFunction> _logger;

    public FetchRssFeedsFunction(Container container, IHttpClientFactory httpClientFactory, ILogger<FetchRssFeedsFunction> logger)
    {
        _container = container;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [Function(nameof(FetchRssFeeds))]
    public async Task FetchRssFeeds([TimerTrigger("0 */20 * * * *")] TimerInfo timerInfo)
    {
        _logger.LogInformation("FetchRssFeeds triggered at {Time}", DateTime.UtcNow);

        var httpClient = _httpClientFactory.CreateClient();
        var totalCount = 0;

        foreach (var (feedName, feedSource) in FeedSources.Feeds)
        {
            try
            {
                var stream = await httpClient.GetStreamAsync(feedSource.Url);
                using var reader = XmlReader.Create(stream);
                var feed = SyndicationFeed.Load(reader);

                foreach (var synItem in feed.Items)
                {
                    var title = synItem.Title?.Text ?? string.Empty;
                    var link = synItem.Links.FirstOrDefault()?.Uri?.ToString() ?? string.Empty;
                    var description = synItem.Summary?.Text;
                    var combinedText = $"{title} {description}";

                    if (feedSource.FilterRequired && !AiKeywords.MatchesAny(combinedText))
                        continue;

                    var company = feedSource.Company != "Various"
                        ? feedSource.Company
                        : AiKeywords.DetectCompany(combinedText);

                    var tags = AiKeywords.GetMatchingTags(combinedText);
                    tags.Add(feedName.ToLowerInvariant().Replace(" ", "-"));

                    var input = !string.IsNullOrEmpty(link) ? link : title;
                    var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input));
                    var stableId = Convert.ToHexString(hash)[..16].ToLower();

                    var item = new NewsItem
                    {
                        ExternalId = $"rss-{stableId}",
                        Source = SourceNames.RssFeed,
                        Title = title,
                        Url = link,
                        Description = description?.Length > 500 ? description[..500] + "..." : description,
                        Author = synItem.Authors.FirstOrDefault()?.Name,
                        Company = company,
                        Tags = tags.Distinct().ToList(),
                        PublishedAt = synItem.PublishDate != default
                            ? synItem.PublishDate.UtcDateTime
                            : DateTime.UtcNow,
                        FetchedAt = DateTime.UtcNow,
                        Metadata = new Dictionary<string, string>
                        {
                            ["feedName"] = feedName,
                            ["feedUrl"] = feedSource.Url
                        }
                    };

                    await _container.UpsertItemAsync(item, new PartitionKey(item.Source));
                    totalCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch RSS feed: {FeedName}", feedName);
            }
        }

        _logger.LogInformation("FetchRssFeeds completed. Upserted {Count} items", totalCount);
    }
}
