namespace NewsDashboard.Shared.Constants;

public static class AiKeywords
{
    public static readonly string[] Primary =
    [
        "claude", "claude code", "anthropic",
        "openai", "chatgpt", "codex", "gpt-4", "gpt-5",
        "gemini", "google ai", "deepmind",
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
        bool isGoogle = lower.Contains("gemini") || lower.Contains("deepmind") || lower.Contains("google ai");

        int count = (isAnthropic ? 1 : 0) + (isOpenAi ? 1 : 0) + (isGoogle ? 1 : 0);
        if (count >= 2) return "Multiple";
        if (isAnthropic) return "Anthropic";
        if (isOpenAi) return "OpenAI";
        if (isGoogle) return "Google";
        return "Other";
    }
}
