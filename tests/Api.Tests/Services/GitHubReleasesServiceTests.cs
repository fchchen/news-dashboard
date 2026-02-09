using FluentAssertions;
using NewsDashboard.Api.Services;
using NewsDashboard.Shared.Constants;
using NewsDashboard.Shared.DTOs;

namespace NewsDashboard.Api.Tests.Services;

public class GitHubReleasesServiceTests
{
    private static GitHubReleaseApiItem CreateRelease(
        string tagName = "v1.5.0",
        string name = "Claude Code v1.5.0",
        string body = "## What's New\n- MCP hook support",
        string htmlUrl = "https://github.com/anthropics/claude-code/releases/tag/v1.5.0",
        DateTime? publishedAt = null,
        bool prerelease = false,
        GitHubAuthor? author = null)
    {
        return new GitHubReleaseApiItem(
            Id: 1,
            TagName: tagName,
            Name: name,
            Body: body,
            HtmlUrl: htmlUrl,
            PublishedAt: publishedAt ?? new DateTime(2026, 2, 9, 10, 0, 0, DateTimeKind.Utc),
            Prerelease: prerelease,
            Author: author ?? new GitHubAuthor("anthropics")
        );
    }

    [Fact]
    public void MapToNewsItem_ShouldMapFieldsCorrectly()
    {
        var release = CreateRelease();

        var result = GitHubReleasesService.MapToNewsItem(release, "anthropics", "claude-code", "Anthropic");

        result.ExternalId.Should().Be("anthropics/claude-code@v1.5.0");
        result.Source.Should().Be(SourceNames.GitHubRelease);
        result.Title.Should().Be("Claude Code v1.5.0");
        result.Url.Should().Be("https://github.com/anthropics/claude-code/releases/tag/v1.5.0");
        result.Description.Should().Contain("MCP hook support");
        result.Author.Should().Be("anthropics");
        result.Company.Should().Be("Anthropic");
    }

    [Fact]
    public void MapToNewsItem_ShouldExtractVersionFromTag()
    {
        var release = CreateRelease(tagName: "v2.3.1");

        var result = GitHubReleasesService.MapToNewsItem(release, "anthropics", "claude-code", "Anthropic");

        result.Metadata["version"].Should().Be("2.3.1");
    }

    [Fact]
    public void MapToNewsItem_ShouldSetRepoFullNameInMetadata()
    {
        var release = CreateRelease();

        var result = GitHubReleasesService.MapToNewsItem(release, "openai", "codex", "OpenAI");

        result.Metadata["repoFullName"].Should().Be("openai/codex");
    }

    [Fact]
    public void MapToNewsItem_ShouldTrackPreReleaseStatus()
    {
        var release = CreateRelease(prerelease: true);

        var result = GitHubReleasesService.MapToNewsItem(release, "anthropics", "claude-code", "Anthropic");

        result.Metadata["isPreRelease"].Should().Be("true");
    }

    [Fact]
    public void MapToNewsItem_ShouldFallbackToRepoNameWhenNoTitle()
    {
        var release = CreateRelease(name: "");

        var result = GitHubReleasesService.MapToNewsItem(release, "anthropics", "claude-code", "Anthropic");

        result.Title.Should().Be("claude-code v1.5.0");
    }

    [Fact]
    public void MapToNewsItem_ShouldIncludeRepoTag()
    {
        var release = CreateRelease();

        var result = GitHubReleasesService.MapToNewsItem(release, "anthropics", "claude-code", "Anthropic");

        result.Tags.Should().Contain("claude-code");
        result.Tags.Should().Contain("release");
    }

    [Fact]
    public void MapToNewsItem_ShouldTruncateLongDescription()
    {
        var longBody = new string('x', 2000);
        var release = CreateRelease(body: longBody);

        var result = GitHubReleasesService.MapToNewsItem(release, "anthropics", "claude-code", "Anthropic");

        result.Description!.Length.Should().BeLessOrEqualTo(1003); // 1000 + "..."
    }

    [Fact]
    public void MapToNewsItem_ShouldHandleNullBody()
    {
        var release = CreateRelease(body: null!);

        var result = GitHubReleasesService.MapToNewsItem(release, "anthropics", "claude-code", "Anthropic");

        result.Description.Should().BeNull();
    }

    [Fact]
    public void MapToNewsItem_ShouldUseOwnerWhenAuthorIsNull()
    {
        var release = new GitHubReleaseApiItem(
            Id: 1, TagName: "v1.0.0", Name: "Test", Body: "body",
            HtmlUrl: "https://example.com", PublishedAt: DateTime.UtcNow,
            Prerelease: false, Author: null);

        var result = GitHubReleasesService.MapToNewsItem(release, "openai", "codex", "OpenAI");

        result.Author.Should().Be("openai");
    }

    [Fact]
    public void MapToNewsItem_ShouldSetPublishedAt()
    {
        var publishDate = new DateTime(2026, 1, 15, 8, 30, 0, DateTimeKind.Utc);
        var release = CreateRelease(publishedAt: publishDate);

        var result = GitHubReleasesService.MapToNewsItem(release, "anthropics", "claude-code", "Anthropic");

        result.PublishedAt.Should().Be(publishDate);
    }

    [Fact]
    public void MapToNewsItem_ShouldExtractAiTagsFromBody()
    {
        var release = CreateRelease(body: "Added MCP tool use and agentic workflow support");

        var result = GitHubReleasesService.MapToNewsItem(release, "anthropics", "claude-code", "Anthropic");

        result.Tags.Should().Contain("mcp");
        result.Tags.Should().Contain("tool use");
        result.Tags.Should().Contain("agentic");
    }
}
