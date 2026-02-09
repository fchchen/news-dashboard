using System.ServiceModel.Syndication;
using FluentAssertions;
using NewsDashboard.Api.Services;
using NewsDashboard.Shared.Constants;

namespace NewsDashboard.Api.Tests.Services;

public class RssFeedServiceTests
{
    private static SyndicationItem CreateSyndicationItem(
        string title = "Anthropic launches Claude 4.5",
        string link = "https://anthropic.com/news/claude-4-5",
        string summary = "New model with improved reasoning capabilities",
        DateTimeOffset? publishDate = null)
    {
        var item = new SyndicationItem
        {
            Title = new TextSyndicationContent(title),
            Summary = new TextSyndicationContent(summary),
            PublishDate = publishDate ?? new DateTimeOffset(2026, 2, 9, 14, 0, 0, TimeSpan.Zero)
        };
        item.Links.Add(new SyndicationLink(new Uri(link)));
        return item;
    }

    [Fact]
    public void MapToNewsItem_ShouldMapFieldsCorrectly()
    {
        var synItem = CreateSyndicationItem();
        var feedSource = new FeedSource("https://anthropic.com/news/rss", "Anthropic", false);

        var result = RssFeedService.MapToNewsItem(synItem, "Anthropic Blog", feedSource);

        result.Source.Should().Be(SourceNames.RssFeed);
        result.Title.Should().Be("Anthropic launches Claude 4.5");
        result.Url.Should().Be("https://anthropic.com/news/claude-4-5");
        result.Company.Should().Be("Anthropic");
        result.Metadata["feedName"].Should().Be("Anthropic Blog");
        result.Metadata["feedUrl"].Should().Be("https://anthropic.com/news/rss");
    }

    [Fact]
    public void MapToNewsItem_ShouldGenerateStableExternalId()
    {
        var synItem = CreateSyndicationItem();
        var feedSource = new FeedSource("https://anthropic.com/news/rss", "Anthropic", false);

        var result1 = RssFeedService.MapToNewsItem(synItem, "Anthropic Blog", feedSource);
        var result2 = RssFeedService.MapToNewsItem(synItem, "Anthropic Blog", feedSource);

        result1.ExternalId.Should().StartWith("rss-");
        result1.ExternalId.Should().Be(result2.ExternalId);
    }

    [Fact]
    public void MapToNewsItem_ShouldDetectCompanyForVariousSources()
    {
        var synItem = CreateSyndicationItem(title: "OpenAI releases new Codex CLI features");
        var feedSource = new FeedSource("https://techcrunch.com/feed/", "Various", true);

        var result = RssFeedService.MapToNewsItem(synItem, "TechCrunch AI", feedSource);

        result.Company.Should().Be("OpenAI");
    }

    [Fact]
    public void MapToNewsItem_ShouldUseFixedCompanyForNonVariousSources()
    {
        var synItem = CreateSyndicationItem(title: "Some random title");
        var feedSource = new FeedSource("https://anthropic.com/news/rss", "Anthropic", false);

        var result = RssFeedService.MapToNewsItem(synItem, "Anthropic Blog", feedSource);

        result.Company.Should().Be("Anthropic");
    }

    [Fact]
    public void MapToNewsItem_ShouldAddFeedNameAsTag()
    {
        var synItem = CreateSyndicationItem();
        var feedSource = new FeedSource("https://anthropic.com/news/rss", "Anthropic", false);

        var result = RssFeedService.MapToNewsItem(synItem, "Anthropic Blog", feedSource);

        result.Tags.Should().Contain("anthropic-blog");
    }

    [Fact]
    public void MapToNewsItem_ShouldExtractAiTags()
    {
        var synItem = CreateSyndicationItem(
            title: "Claude Code gets MCP support",
            summary: "Agentic AI workflows now supported");
        var feedSource = new FeedSource("https://anthropic.com/news/rss", "Anthropic", false);

        var result = RssFeedService.MapToNewsItem(synItem, "Anthropic Blog", feedSource);

        result.Tags.Should().Contain("claude code");
        result.Tags.Should().Contain("mcp");
        result.Tags.Should().Contain("agentic");
    }

    [Fact]
    public void MapToNewsItem_ShouldTruncateHtmlDescription()
    {
        var htmlContent = $"<p>{new string('x', 600)}</p>";
        var synItem = CreateSyndicationItem(summary: htmlContent);
        var feedSource = new FeedSource("https://anthropic.com/news/rss", "Anthropic", false);

        var result = RssFeedService.MapToNewsItem(synItem, "Anthropic Blog", feedSource);

        result.Description!.Should().NotContain("<p>");
        result.Description!.Length.Should().BeLessOrEqualTo(503); // 500 + "..."
    }

    [Fact]
    public void MapToNewsItem_ShouldHandleNullDescription()
    {
        var synItem = new SyndicationItem
        {
            Title = new TextSyndicationContent("Test title"),
            PublishDate = DateTimeOffset.UtcNow
        };
        synItem.Links.Add(new SyndicationLink(new Uri("https://example.com")));
        var feedSource = new FeedSource("https://example.com/rss", "Other", true);

        var result = RssFeedService.MapToNewsItem(synItem, "Test Feed", feedSource);

        result.Description.Should().BeNull();
    }

    [Fact]
    public void MapToNewsItem_ShouldUseCurrentTimeWhenNoPublishDate()
    {
        var synItem = new SyndicationItem
        {
            Title = new TextSyndicationContent("Test title"),
            Summary = new TextSyndicationContent("Test summary")
        };
        synItem.Links.Add(new SyndicationLink(new Uri("https://example.com")));
        var feedSource = new FeedSource("https://example.com/rss", "Other", true);

        var before = DateTime.UtcNow;
        var result = RssFeedService.MapToNewsItem(synItem, "Test Feed", feedSource);

        result.PublishedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void GenerateStableId_ShouldReturnConsistentHash()
    {
        var id1 = RssFeedService.GenerateStableId("https://example.com/article", "Test Title");
        var id2 = RssFeedService.GenerateStableId("https://example.com/article", "Test Title");

        id1.Should().Be(id2);
        id1.Should().HaveLength(16);
    }

    [Fact]
    public void GenerateStableId_ShouldPreferUrlOverTitle()
    {
        var idWithUrl = RssFeedService.GenerateStableId("https://example.com/article", "Title");
        var idWithDifferentTitle = RssFeedService.GenerateStableId("https://example.com/article", "Different Title");

        idWithUrl.Should().Be(idWithDifferentTitle);
    }

    [Fact]
    public void GenerateStableId_ShouldFallbackToTitleWhenUrlEmpty()
    {
        var id = RssFeedService.GenerateStableId("", "My Article Title");

        id.Should().NotBeEmpty();
        id.Should().HaveLength(16);
    }
}
