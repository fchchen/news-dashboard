using NewsDashboard.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace NewsDashboard.Api.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api")
            .WithTags("Dashboard");

        group.MapGet("/dashboard", GetDashboardSummary)
            .Produces<NewsDashboard.Shared.DTOs.DashboardSummaryResponse>();

        group.MapGet("/news", GetUnifiedFeed)
            .Produces<NewsDashboard.Shared.DTOs.PagedResponse<NewsDashboard.Shared.DTOs.NewsItemDto>>();

        group.MapGet("/news/trends", GetTrends)
            .Produces<NewsDashboard.Shared.DTOs.TrendsResponse>();

        group.MapPost("/refresh", RefreshAllSources)
            .Produces(StatusCodes.Status200OK);

        return app;
    }

    private static async Task<IResult> GetDashboardSummary(INewsDashboardService dashboardService)
    {
        var result = await dashboardService.GetDashboardSummaryAsync();
        return Results.Ok(result);
    }

    private static async Task<IResult> GetUnifiedFeed(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] string? source,
        [FromQuery] string? company,
        INewsDashboardService dashboardService)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize > 100 ? 100 : pageSize;

        var result = await dashboardService.GetUnifiedFeedAsync(page, pageSize, source, company);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetTrends(INewsDashboardService dashboardService)
    {
        var result = await dashboardService.GetTrendsAsync();
        return Results.Ok(result);
    }

    private static async Task<IResult> RefreshAllSources(
        INewsDashboardService dashboardService,
        IHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            return Results.Forbid();
        }

        await dashboardService.RefreshAllSourcesAsync();
        return Results.Ok(new { message = "All sources refreshed" });
    }
}
