using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using NewsDashboard.Api.Services;
using NewsDashboard.Shared.DTOs;
using NewsDashboard.Shared.Models;

namespace NewsDashboard.Api.Tests.Services;

public class HackerNewsServiceTests
{
    [Fact]
    public void MapToNewsItem_ShouldMapFieldsCorrectly()
    {
        // Arrange
        var hnItem = new HackerNewsApiItem(
            Id: 12345,
            Title: "Claude Code now supports MCP hooks",
            Url: "https://anthropic.com/news/claude-code-mcp",
            By: "testuser",
            Score: 542,
            Descendants: 187,
            Time: 1738000000,
            Type: "story"
        );

        // Act
        var result = HackerNewsService.MapToNewsItem(hnItem);

        // Assert
        result.ExternalId.Should().Be("hn-12345");
        result.Source.Should().Be("HackerNews");
        result.Title.Should().Be("Claude Code now supports MCP hooks");
        result.Url.Should().Be("https://anthropic.com/news/claude-code-mcp");
        result.Score.Should().Be(542);
        result.Author.Should().Be("testuser");
        result.Metadata["commentCount"].Should().Be("187");
        result.Metadata["hnUrl"].Should().Be("https://news.ycombinator.com/item?id=12345");
    }

    [Fact]
    public void MapToNewsItem_ShouldDetectAnthropicCompany()
    {
        var hnItem = new HackerNewsApiItem(12345, "Anthropic releases new Claude model", "https://anthropic.com", "user", 100, 50, 1738000000, "story");

        var result = HackerNewsService.MapToNewsItem(hnItem);

        result.Company.Should().Be("Anthropic");
    }

    [Fact]
    public void MapToNewsItem_ShouldDetectOpenAiCompany()
    {
        var hnItem = new HackerNewsApiItem(12345, "OpenAI launches GPT-5", "https://openai.com", "user", 100, 50, 1738000000, "story");

        var result = HackerNewsService.MapToNewsItem(hnItem);

        result.Company.Should().Be("OpenAI");
    }

    [Fact]
    public void MapToNewsItem_ShouldDetectBothCompanies()
    {
        var hnItem = new HackerNewsApiItem(12345, "Claude vs ChatGPT comparison", "https://example.com", "user", 100, 50, 1738000000, "story");

        var result = HackerNewsService.MapToNewsItem(hnItem);

        result.Company.Should().Be("Both");
    }

    [Fact]
    public void MapToNewsItem_ShouldExtractAiTags()
    {
        var hnItem = new HackerNewsApiItem(12345, "Claude Code agentic AI coding tool", "https://anthropic.com", "user", 100, 50, 1738000000, "story");

        var result = HackerNewsService.MapToNewsItem(hnItem);

        result.Tags.Should().Contain("claude");
        result.Tags.Should().Contain("claude code");
        result.Tags.Should().Contain("agentic");
        result.Tags.Should().Contain("ai coding");
        result.Tags.Should().Contain("anthropic");
    }

    [Fact]
    public void MapToNewsItem_ShouldUseHnUrlWhenNoUrl()
    {
        var hnItem = new HackerNewsApiItem(12345, "Ask HN: Best AI tools?", null, "user", 100, 50, 1738000000, "story");

        var result = HackerNewsService.MapToNewsItem(hnItem);

        result.Url.Should().Be("https://news.ycombinator.com/item?id=12345");
    }

    [Fact]
    public void MapToNewsItem_ShouldConvertUnixTimestamp()
    {
        var hnItem = new HackerNewsApiItem(12345, "Test", "https://example.com", "user", 100, 50, 1738000000, "story");

        var result = HackerNewsService.MapToNewsItem(hnItem);

        result.PublishedAt.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1738000000).UtcDateTime);
    }
}
