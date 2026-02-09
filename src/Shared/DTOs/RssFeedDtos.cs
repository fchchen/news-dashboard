namespace NewsDashboard.Shared.DTOs;

public record RssFeedSourceDto(
    string Name,
    string Url,
    string Company,
    bool FilterRequired
);
