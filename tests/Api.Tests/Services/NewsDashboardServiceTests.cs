using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using NewsDashboard.Api.Services;
using NewsDashboard.Shared.Models;

namespace NewsDashboard.Api.Tests.Services;

public class NewsDashboardServiceTests
{
    private readonly Mock<IHackerNewsService> _hnMock = new();
    private readonly Mock<IGitHubReleasesService> _ghMock = new();
    private readonly Mock<IRssFeedService> _rssMock = new();
    private readonly Mock<ICosmosDbService> _cosmosMock = new();
    private readonly Mock<ILogger<NewsDashboardService>> _loggerMock = new();
    private readonly NewsDashboardService _sut;

    public NewsDashboardServiceTests()
    {
        _sut = new NewsDashboardService(_hnMock.Object, _ghMock.Object, _rssMock.Object, _cosmosMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_ShouldReturnCorrectStats()
    {
        // Arrange
        _cosmosMock.Setup(c => c.GetNewsItemCountAsync("HackerNews", null)).ReturnsAsync(24);
        _cosmosMock.Setup(c => c.GetNewsItemCountAsync("GitHubRelease", null)).ReturnsAsync(8);
        _cosmosMock.Setup(c => c.GetNewsItemCountAsync("RssFeed", null)).ReturnsAsync(31);
        _cosmosMock.Setup(c => c.GetNewsItemCountAsync(null, "Anthropic")).ReturnsAsync(18);
        _cosmosMock.Setup(c => c.GetNewsItemCountAsync(null, "OpenAI")).ReturnsAsync(12);
        _cosmosMock.Setup(c => c.GetNewsItemCountAsync(null, "Google")).ReturnsAsync(7);
        _cosmosMock.Setup(c => c.GetNewsItemsAsync(1, 5, "HackerNews", null)).ReturnsAsync([]);
        _cosmosMock.Setup(c => c.GetNewsItemsAsync(1, 4, "GitHubRelease", null)).ReturnsAsync([]);
        _cosmosMock.Setup(c => c.GetNewsItemsAsync(1, 4, "RssFeed", null)).ReturnsAsync([]);
        _cosmosMock.Setup(c => c.GetNewsItemsAsync(1, 50, null, null)).ReturnsAsync([]);
        _cosmosMock.Setup(c => c.GetLatestSnapshotAsync()).ReturnsAsync((TrendSnapshot?)null);

        // Act
        var result = await _sut.GetDashboardSummaryAsync();

        // Assert
        result.Stats.HackerNewsCount.Should().Be(24);
        result.Stats.GitHubReleaseCount.Should().Be(8);
        result.Stats.RssArticleCount.Should().Be(31);
        result.Stats.TotalCount.Should().Be(63);
        result.Companies.Should().HaveCount(3);
        result.Companies[0].Company.Should().Be("Anthropic");
        result.Companies[0].ItemCount.Should().Be(18);
        result.Companies[1].Company.Should().Be("OpenAI");
        result.Companies[1].ItemCount.Should().Be(12);
        result.Companies[2].Company.Should().Be("Google");
        result.Companies[2].ItemCount.Should().Be(7);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_ShouldComputeTrendingTopics()
    {
        // Arrange
        var items = new List<NewsItem>
        {
            new() { Tags = ["claude", "agentic", "mcp"], Source = "HackerNews" },
            new() { Tags = ["claude", "claude code"], Source = "HackerNews" },
            new() { Tags = ["openai", "gpt-5"], Source = "RssFeed" },
            new() { Tags = ["agentic", "ai agent"], Source = "RssFeed" },
        };

        _cosmosMock.Setup(c => c.GetNewsItemCountAsync(It.IsAny<string?>(), It.IsAny<string?>())).ReturnsAsync(0);
        _cosmosMock.Setup(c => c.GetNewsItemsAsync(1, 5, "HackerNews", null)).ReturnsAsync([]);
        _cosmosMock.Setup(c => c.GetNewsItemsAsync(1, 4, "GitHubRelease", null)).ReturnsAsync([]);
        _cosmosMock.Setup(c => c.GetNewsItemsAsync(1, 4, "RssFeed", null)).ReturnsAsync([]);
        _cosmosMock.Setup(c => c.GetNewsItemsAsync(1, 50, null, null)).ReturnsAsync(items);
        _cosmosMock.Setup(c => c.GetLatestSnapshotAsync()).ReturnsAsync((TrendSnapshot?)null);

        // Act
        var result = await _sut.GetDashboardSummaryAsync();

        // Assert
        result.TrendingTopics.Should().NotBeEmpty();
        result.TrendingTopics.First().MentionCount.Should().Be(2);
        result.TrendingTopics.Should().Contain(t => t.Topic == "claude" && t.MentionCount == 2);
        result.TrendingTopics.Should().Contain(t => t.Topic == "agentic" && t.MentionCount == 2);
    }

    [Fact]
    public async Task RefreshAllSourcesAsync_ShouldCallAllServices()
    {
        // Arrange
        _hnMock.Setup(h => h.FetchAndFilterAsync(100)).ReturnsAsync([]);
        _ghMock.Setup(g => g.FetchAllReleasesAsync()).ReturnsAsync([]);
        _rssMock.Setup(r => r.FetchAllFeedsAsync()).ReturnsAsync([]);
        _cosmosMock.Setup(c => c.GetNewsItemCountAsync(It.IsAny<string?>(), It.IsAny<string?>())).ReturnsAsync(0);
        _cosmosMock.Setup(c => c.GetNewsItemsAsync(1, 100, null, null)).ReturnsAsync([]);
        _cosmosMock.Setup(c => c.UpsertSnapshotAsync(It.IsAny<TrendSnapshot>()))
            .ReturnsAsync(new TrendSnapshot());

        // Act
        await _sut.RefreshAllSourcesAsync();

        // Assert
        _hnMock.Verify(h => h.FetchAndFilterAsync(100), Times.Once);
        _ghMock.Verify(g => g.FetchAllReleasesAsync(), Times.Once);
        _rssMock.Verify(r => r.FetchAllFeedsAsync(), Times.Once);
        _cosmosMock.Verify(c => c.UpsertSnapshotAsync(It.IsAny<TrendSnapshot>()), Times.Once);
    }
}
