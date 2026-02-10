namespace NewsDashboard.Shared.Constants;

public static class TrackedRepos
{
    public static readonly (string Owner, string Repo, string Company)[] Repos =
    [
        ("anthropics", "claude-code", "Anthropic"),
        ("openai", "codex", "OpenAI"),
        ("getcursor", "cursor", "Other"),
        ("continuedev", "continue", "Other"),
        ("paul-gauthier", "aider", "Other"),
        ("cline", "cline", "Other")
    ];
}

public static class FeedSources
{
    public static readonly Dictionary<string, FeedSource> Feeds = new()
    {
        ["Anthropic Blog"] = new("https://raw.githubusercontent.com/Olshansk/rss-feeds/main/feeds/feed_anthropic_news.xml", "Anthropic", false),
        ["OpenAI Blog"] = new("https://openai.com/news/rss.xml", "OpenAI", false),
        ["TechCrunch AI"] = new("https://techcrunch.com/category/artificial-intelligence/feed/", "Various", true),
        ["The Verge AI"] = new("https://www.theverge.com/rss/ai-artificial-intelligence/index.xml", "Various", true),
        ["Ars Technica AI"] = new("https://feeds.arstechnica.com/arstechnica/index", "Various", true),
        ["VentureBeat AI"] = new("https://venturebeat.com/category/ai/feed/", "Various", true)
    };
}

public record FeedSource(string Url, string Company, bool FilterRequired);
