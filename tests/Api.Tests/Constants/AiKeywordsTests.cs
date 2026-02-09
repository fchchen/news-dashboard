using FluentAssertions;
using NewsDashboard.Shared.Constants;

namespace NewsDashboard.Api.Tests.Constants;

public class AiKeywordsTests
{
    [Theory]
    [InlineData("Claude Code is amazing", true)]
    [InlineData("Anthropic releases new model", true)]
    [InlineData("OpenAI GPT-5 announcement", true)]
    [InlineData("ChatGPT update released", true)]
    [InlineData("Agentic AI workflows", true)]
    [InlineData("New Cursor IDE features", true)]
    [InlineData("Aider CLI tool update", true)]
    [InlineData("MCP tool use integration", true)]
    [InlineData("LLM benchmarks comparison", true)]
    [InlineData("Today's weather forecast", false)]
    [InlineData("JavaScript framework comparison", false)]
    [InlineData("", false)]
    public void MatchesAny_ShouldFilterCorrectly(string text, bool expected)
    {
        AiKeywords.MatchesAny(text).Should().Be(expected);
    }

    [Fact]
    public void MatchesAny_ShouldBeCaseInsensitive()
    {
        AiKeywords.MatchesAny("CLAUDE CODE").Should().BeTrue();
        AiKeywords.MatchesAny("ANTHROPIC").Should().BeTrue();
        AiKeywords.MatchesAny("openAI").Should().BeTrue();
    }

    [Fact]
    public void GetMatchingTags_ShouldReturnAllMatchingKeywords()
    {
        var tags = AiKeywords.GetMatchingTags("Claude Code is an agentic AI coding tool by Anthropic");

        tags.Should().Contain("claude");
        tags.Should().Contain("claude code");
        tags.Should().Contain("agentic");
        tags.Should().Contain("ai coding");
        tags.Should().Contain("anthropic");
    }

    [Fact]
    public void GetMatchingTags_ShouldReturnEmptyForNonAiContent()
    {
        var tags = AiKeywords.GetMatchingTags("JavaScript framework comparison");
        tags.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Anthropic releases Claude 4", "Anthropic")]
    [InlineData("Claude Code update", "Anthropic")]
    [InlineData("OpenAI launches GPT-5", "OpenAI")]
    [InlineData("ChatGPT new features", "OpenAI")]
    [InlineData("Codex CLI improvements", "OpenAI")]
    [InlineData("Claude vs ChatGPT comparison", "Both")]
    [InlineData("Cursor IDE update", "Other")]
    [InlineData("Random tech news", "Other")]
    public void DetectCompany_ShouldIdentifyCorrectCompany(string text, string expectedCompany)
    {
        AiKeywords.DetectCompany(text).Should().Be(expectedCompany);
    }

    [Fact]
    public void DetectCompany_ShouldHandleNullAndEmpty()
    {
        AiKeywords.DetectCompany("").Should().Be("Other");
        AiKeywords.DetectCompany(null!).Should().Be("Other");
    }
}
