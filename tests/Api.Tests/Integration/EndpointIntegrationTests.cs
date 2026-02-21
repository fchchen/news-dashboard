using System.Net;
using System.Text.Json;
using FluentAssertions;

namespace NewsDashboard.Api.Tests.Integration;

public class EndpointIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EndpointIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync() => await _factory.SeedDataAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("status").GetString().Should().Be("healthy");
        doc.RootElement.TryGetProperty("timestamp", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Dashboard_ReturnsSeededData()
    {
        var response = await _client.GetAsync("/api/dashboard");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        // Stats reflect seeded items: 2 HN, 1 GH, 2 RSS = 5 total
        var stats = root.GetProperty("stats");
        stats.GetProperty("hackerNewsCount").GetInt32().Should().Be(2);
        stats.GetProperty("gitHubReleaseCount").GetInt32().Should().Be(1);
        stats.GetProperty("rssArticleCount").GetInt32().Should().Be(2);
        stats.GetProperty("totalCount").GetInt32().Should().Be(5);

        // Company breakdown: 3 Anthropic, 1 OpenAI, 1 Google
        var companies = root.GetProperty("companies");
        companies.GetArrayLength().Should().Be(3);

        // Hot items should include the highest-score HN story
        var hotItems = root.GetProperty("hotItems");
        hotItems.GetArrayLength().Should().BeGreaterThan(0);
        hotItems[0].GetProperty("title").GetString().Should().Be("Claude Code ships new feature");
    }

    [Fact]
    public async Task News_ReturnsAllSeededItems()
    {
        var response = await _client.GetAsync("/api/news?page=1&pageSize=20");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.GetProperty("totalCount").GetInt32().Should().Be(5);
        root.GetProperty("items").GetArrayLength().Should().Be(5);
        root.GetProperty("page").GetInt32().Should().Be(1);
        root.GetProperty("pageSize").GetInt32().Should().Be(20);
    }

    [Fact]
    public async Task News_WithPagination_ReturnsCorrectSlice()
    {
        var response = await _client.GetAsync("/api/news?page=1&pageSize=2");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().Be(5);
        doc.RootElement.GetProperty("items").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task News_WithInvalidPagination_ClampsValues()
    {
        var response = await _client.GetAsync("/api/news?page=0&pageSize=0");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("page").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(20);
    }

    [Fact]
    public async Task News_FilterByCompany_ReturnsMatchingItems()
    {
        var response = await _client.GetAsync("/api/news?page=1&pageSize=20&company=Anthropic");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().Be(3);
    }

    [Fact]
    public async Task HackerNews_ReturnsOnlyHnItems()
    {
        var response = await _client.GetAsync("/api/hackernews?page=1&pageSize=10");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().Be(2);
        doc.RootElement.GetProperty("items").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task GitHubReleases_ReturnsOnlyReleaseItems()
    {
        var response = await _client.GetAsync("/api/github/releases?page=1&pageSize=10");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("items")[0].GetProperty("title").GetString()
            .Should().Be("Claude Code v1.0.0");
    }

    [Fact]
    public async Task RssFeeds_ReturnsOnlyRssItems()
    {
        var response = await _client.GetAsync("/api/rss?page=1&pageSize=10");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().Be(2);
        doc.RootElement.GetProperty("items").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task RssSources_ReturnsListOfSources()
    {
        var response = await _client.GetAsync("/api/rss/sources");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        doc.RootElement.GetArrayLength().Should().BeGreaterThan(0);
    }
}
