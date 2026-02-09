namespace NewsDashboard.Api.Services;

public class NewsFetchBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NewsFetchBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);

    public NewsFetchBackgroundService(IServiceProvider serviceProvider, ILogger<NewsFetchBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("News fetch background service starting. Interval: {Interval}", _interval);

        // Initial fetch after 30 second delay (let app start up)
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting scheduled news fetch");
                using var scope = _serviceProvider.CreateScope();
                var dashboardService = scope.ServiceProvider.GetRequiredService<INewsDashboardService>();
                await dashboardService.RefreshAllSourcesAsync();
                _logger.LogInformation("Scheduled news fetch completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled news fetch");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
