using NewsDashboard.Api.Services;
using NewsDashboard.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace NewsDashboard.Api.Endpoints;

public static class RssFeedEndpoints
{
    public static IEndpointRouteBuilder MapRssFeedEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/rss")
            .WithTags("RSS Feeds");

        group.MapGet("/", GetRssItems)
            .Produces<PagedResponse<NewsItemDto>>();

        group.MapGet("/sources", GetSources)
            .Produces<IEnumerable<RssFeedSourceDto>>();

        return app;
    }

    private static async Task<IResult> GetRssItems(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] string? source,
        IRssFeedService rssFeedService)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize > 100 ? 100 : pageSize;

        var items = await rssFeedService.GetCachedItemsAsync(page, pageSize, source);
        var totalCount = await rssFeedService.GetCachedCountAsync(source);

        var dtos = items.Select(i => new NewsItemDto(
            i.Id, i.ExternalId, i.Source, i.Title, i.Url, i.Description,
            i.Score, i.Author, i.Company, i.Tags, i.PublishedAt, i.Metadata));

        return Results.Ok(new PagedResponse<NewsItemDto>(dtos, totalCount, page, pageSize));
    }

    private static IResult GetSources(IRssFeedService rssFeedService)
    {
        return Results.Ok(rssFeedService.GetAvailableSources());
    }
}
