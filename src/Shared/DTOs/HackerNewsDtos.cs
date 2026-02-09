namespace NewsDashboard.Shared.DTOs;

/// <summary>
/// HN Firebase API item response.
/// </summary>
public record HackerNewsApiItem(
    int Id,
    string? Title,
    string? Url,
    string? By,
    int Score,
    int Descendants,
    long Time,
    string? Type
);
