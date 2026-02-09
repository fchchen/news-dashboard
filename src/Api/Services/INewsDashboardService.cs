using NewsDashboard.Shared.DTOs;

namespace NewsDashboard.Api.Services;

public interface INewsDashboardService
{
    Task<DashboardSummaryResponse> GetDashboardSummaryAsync();
    Task<TrendsResponse> GetTrendsAsync();
    Task<PagedResponse<NewsItemDto>> GetUnifiedFeedAsync(int page, int pageSize, string? source = null, string? company = null);
    Task RefreshAllSourcesAsync();
}
