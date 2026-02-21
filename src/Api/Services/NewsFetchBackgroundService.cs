namespace NewsDashboard.Api.Services;

public class NewsFetchBackgroundService(IServiceProvider serviceProvider, ILogger<NewsFetchBackgroundService> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("News fetch background service starting. Interval: {Interval}", _interval);

        // Initial fetch after 30 second delay (let app start up)
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Starting scheduled news fetch");
                using var scope = serviceProvider.CreateScope();
                var dashboardService = scope.ServiceProvider.GetRequiredService<INewsDashboardService>();
                await dashboardService.RefreshAllSourcesAsync();
                logger.LogInformation("Scheduled news fetch completed");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during scheduled news fetch");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
