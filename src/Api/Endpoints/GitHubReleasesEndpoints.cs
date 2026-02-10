using NewsDashboard.Api.Services;
using NewsDashboard.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace NewsDashboard.Api.Endpoints;

public static class GitHubReleasesEndpoints
{
    public static IEndpointRouteBuilder MapGitHubReleasesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/github/releases")
            .WithTags("GitHub Releases");

        group.MapGet("/", GetAllReleases)
            .Produces<PagedResponse<NewsItemDto>>();

        group.MapGet("/{owner}/{repo}", GetRepoReleases)
            .Produces<PagedResponse<NewsItemDto>>();

        return app;
    }

    private static async Task<IResult> GetAllReleases(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        IGitHubReleasesService releasesService)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize > 1000 ? 1000 : pageSize;

        var items = await releasesService.GetCachedItemsAsync(page, pageSize);
        var totalCount = await releasesService.GetCachedCountAsync();

        var dtos = items.Select(i => new NewsItemDto(
            i.Id, i.ExternalId, i.Source, i.Title, i.Url, i.Description,
            i.Score, i.Author, i.Company, i.Tags, i.PublishedAt, i.Metadata));

        return Results.Ok(new PagedResponse<NewsItemDto>(dtos, totalCount, page, pageSize));
    }

    private static async Task<IResult> GetRepoReleases(
        string owner,
        string repo,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        IGitHubReleasesService releasesService)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize > 1000 ? 1000 : pageSize;

        var items = await releasesService.GetCachedByRepoAsync(owner, repo, page, pageSize);
        var totalCount = await releasesService.GetCachedByRepoCountAsync(owner, repo);

        var dtos = items.Select(i => new NewsItemDto(
            i.Id, i.ExternalId, i.Source, i.Title, i.Url, i.Description,
            i.Score, i.Author, i.Company, i.Tags, i.PublishedAt, i.Metadata));

        return Results.Ok(new PagedResponse<NewsItemDto>(dtos, totalCount, page, pageSize));
    }
}
