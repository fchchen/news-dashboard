using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NewsDashboard.Api.Services;
using NewsDashboard.Shared.Constants;
using NewsDashboard.Shared.Models;

namespace NewsDashboard.Api.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:CosmosDb", "");

        builder.ConfigureServices(services =>
        {
            // Remove the background service so it doesn't fetch during tests
            var descriptor = services.SingleOrDefault(
                d => d.ImplementationType == typeof(NewsFetchBackgroundService));
            if (descriptor != null)
                services.Remove(descriptor);
        });
    }

    public async Task SeedDataAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ICosmosDbService>();

        var items = new List<NewsItem>
        {
            new()
            {
                ExternalId = "hn-1001", Source = SourceNames.HackerNews,
                Title = "Claude Code ships new feature", Url = "https://example.com/1",
                Score = 250, Author = "testuser", Company = "Anthropic",
                Tags = ["claude", "anthropic"], PublishedAt = DateTime.UtcNow.AddHours(-1),
                Metadata = new() { ["commentCount"] = "42", ["hnUrl"] = "https://news.ycombinator.com/item?id=1001" }
            },
            new()
            {
                ExternalId = "hn-1002", Source = SourceNames.HackerNews,
                Title = "GPT-5 benchmark results", Url = "https://example.com/2",
                Score = 180, Author = "aidev", Company = "OpenAI",
                Tags = ["openai", "gpt"], PublishedAt = DateTime.UtcNow.AddHours(-2),
                Metadata = new() { ["commentCount"] = "31", ["hnUrl"] = "https://news.ycombinator.com/item?id=1002" }
            },
            new()
            {
                ExternalId = "gh-claude-code@v1.0", Source = SourceNames.GitHubRelease,
                Title = "Claude Code v1.0.0", Url = "https://github.com/anthropics/claude-code/releases/v1.0",
                Score = 0, Author = "anthropics", Company = "Anthropic",
                Tags = ["claude-code", "release"], PublishedAt = DateTime.UtcNow.AddHours(-3),
                Metadata = new() { ["version"] = "1.0.0", ["repoFullName"] = "anthropics/claude-code", ["isPreRelease"] = "false" }
            },
            new()
            {
                ExternalId = "rss-abc123", Source = SourceNames.RssFeed,
                Title = "Gemini 2.5 Pro announced", Url = "https://blog.google/ai/gemini-2-5",
                Score = 0, Author = "Google AI Blog", Company = "Google",
                Tags = ["gemini", "google"], PublishedAt = DateTime.UtcNow.AddHours(-4),
                Metadata = new() { ["feedName"] = "Google AI Blog", ["feedUrl"] = "https://blog.google/technology/ai/rss/" }
            },
            new()
            {
                ExternalId = "rss-def456", Source = SourceNames.RssFeed,
                Title = "Anthropic safety research update", Url = "https://anthropic.com/blog/safety",
                Score = 0, Author = "Anthropic Blog", Company = "Anthropic",
                Tags = ["anthropic", "safety"], PublishedAt = DateTime.UtcNow.AddHours(-5),
                Metadata = new() { ["feedName"] = "Anthropic Blog", ["feedUrl"] = "https://www.anthropic.com/rss.xml" }
            }
        };

        await db.UpsertManyNewsItemsAsync(items);
    }
}
