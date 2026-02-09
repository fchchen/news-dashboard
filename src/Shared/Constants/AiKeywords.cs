namespace NewsDashboard.Shared.Constants;

public static class AiKeywords
{
    public static readonly string[] Primary =
    [
        "claude", "claude code", "anthropic",
        "openai", "chatgpt", "codex", "gpt-4", "gpt-5",
        "agentic", "ai agent", "ai coding",
        "cursor", "windsurf", "copilot", "cline", "aider"
    ];

    public static readonly string[] Secondary =
    [
        "llm", "large language model", "model release",
        "mcp", "tool use", "function calling",
        "ai cli", "ai terminal", "code generation"
    ];

    public static readonly string[] All = [.. Primary, .. Secondary];

    public static bool MatchesAny(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        var lower = text.ToLowerInvariant();
        return Array.Exists(All, keyword => lower.Contains(keyword));
    }

    public static List<string> GetMatchingTags(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        var lower = text.ToLowerInvariant();
        return All.Where(keyword => lower.Contains(keyword)).ToList();
    }

    public static string DetectCompany(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "Other";
        var lower = text.ToLowerInvariant();

        bool isAnthropic = lower.Contains("anthropic") || lower.Contains("claude");
        bool isOpenAi = lower.Contains("openai") || lower.Contains("chatgpt") ||
                        lower.Contains("codex") || lower.Contains("gpt-4") || lower.Contains("gpt-5");

        if (isAnthropic && !isOpenAi) return "Anthropic";
        if (isOpenAi && !isAnthropic) return "OpenAI";
        if (isAnthropic && isOpenAi) return "Both";
        return "Other";
    }
}
