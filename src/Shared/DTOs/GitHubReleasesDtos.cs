using System.Text.Json.Serialization;

namespace NewsDashboard.Shared.DTOs;

public record GitHubReleaseApiItem(
    int Id,
    [property: JsonPropertyName("tag_name")] string TagName,
    string Name,
    string Body,
    [property: JsonPropertyName("html_url")] string HtmlUrl,
    [property: JsonPropertyName("published_at")] DateTime PublishedAt,
    [property: JsonPropertyName("prerelease")] bool Prerelease,
    GitHubAuthor? Author
);

public record GitHubAuthor(
    string Login
);
