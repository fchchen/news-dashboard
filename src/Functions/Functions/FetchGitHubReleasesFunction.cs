using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NewsDashboard.Shared.Constants;
using NewsDashboard.Shared.DTOs;
using NewsDashboard.Shared.Models;

namespace NewsDashboard.Functions.Functions;

public class FetchGitHubReleasesFunction(Container container, IHttpClientFactory httpClientFactory, ILogger<FetchGitHubReleasesFunction> logger)
{
    private const string BaseUrl = "https://api.github.com";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Function(nameof(FetchGitHubReleases))]
    public async Task FetchGitHubReleases([TimerTrigger("0 */30 * * * *")] TimerInfo timerInfo)
    {
        logger.LogInformation("FetchGitHubReleases triggered at {Time}", DateTime.UtcNow);

        var httpClient = httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("NewsDashboard/1.0");
        httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");

        var totalCount = 0;

        foreach (var (owner, repo, company) in TrackedRepos.Repos)
        {
            try
            {
                var url = $"{BaseUrl}/repos/{owner}/{repo}/releases?per_page=5";
                var json = await httpClient.GetStringAsync(url);
                var releases = JsonSerializer.Deserialize<List<GitHubReleaseApiItem>>(json, JsonOptions) ?? [];

                foreach (var release in releases)
                {
                    var repoFullName = $"{owner}/{repo}";
                    var version = release.TagName.TrimStart('v');
                    var title = !string.IsNullOrEmpty(release.Name) ? release.Name : $"{repo} {release.TagName}";
                    var tags = new List<string> { repo, "release", "cli" };
                    tags.AddRange(AiKeywords.GetMatchingTags(title + " " + release.Body));

                    var item = new NewsItem
                    {
                        ExternalId = $"{repoFullName}@{release.TagName}",
                        Source = SourceNames.GitHubRelease,
                        Title = title,
                        Url = release.HtmlUrl,
                        Description = release.Body?.Length > 1000 ? release.Body[..1000] + "..." : release.Body,
                        Author = release.Author?.Login ?? owner,
                        Company = company,
                        Tags = tags.Distinct().ToList(),
                        PublishedAt = release.PublishedAt,
                        FetchedAt = DateTime.UtcNow,
                        Metadata = new Dictionary<string, string>
                        {
                            ["version"] = version,
                            ["repoFullName"] = repoFullName,
                            ["isPreRelease"] = release.Prerelease.ToString().ToLower()
                        }
                    };

                    await container.UpsertItemAsync(item, new PartitionKey(item.Source));
                    totalCount++;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to fetch releases for {Owner}/{Repo}", owner, repo);
            }
        }

        logger.LogInformation("FetchGitHubReleases completed. Upserted {Count} items", totalCount);
    }
}
