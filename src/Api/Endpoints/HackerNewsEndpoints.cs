using NewsDashboard.Api.Services;
using NewsDashboard.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace NewsDashboard.Api.Endpoints;

public static class HackerNewsEndpoints
{
    public static IEndpointRouteBuilder MapHackerNewsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/hackernews")
            .WithTags("Hacker News");

        group.MapGet("/", GetHackerNewsItems)
            .Produces<PagedResponse<NewsItemDto>>();

        return app;
    }

    private static async Task<IResult> GetHackerNewsItems(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        IHackerNewsService hackerNewsService)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize > 1000 ? 1000 : pageSize;

        var items = await hackerNewsService.GetCachedItemsAsync(page, pageSize);
        var totalCount = await hackerNewsService.GetCachedCountAsync();

        var dtos = items.Select(i => new NewsItemDto(
            i.Id, i.ExternalId, i.Source, i.Title, i.Url, i.Description,
            i.Score, i.Author, i.Company, i.Tags, i.PublishedAt, i.Metadata));

        return Results.Ok(new PagedResponse<NewsItemDto>(dtos, totalCount, page, pageSize));
    }
}
