using InfraFlowSculptor.Mcp.Drafts;
using InfraFlowSculptor.Mcp.Imports;

namespace InfraFlowSculptor.Mcp.Common;

/// <summary>
/// Background service that periodically evicts stale drafts and import previews
/// from in-memory singleton stores to prevent unbounded memory growth.
/// </summary>
public sealed class InMemoryCleanupService(
    IProjectDraftService draftService,
    IImportPreviewService previewService,
    ILogger<InMemoryCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(30);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(CleanupInterval, stoppingToken);

            var draftsEvicted = draftService.EvictExpired(MaxAge);
            var previewsEvicted = previewService.EvictExpired(MaxAge);

            if (draftsEvicted > 0 || previewsEvicted > 0)
            {
                logger.LogInformation(
                    "In-memory cleanup: evicted {DraftCount} drafts and {PreviewCount} previews older than {MaxAge}.",
                    draftsEvicted, previewsEvicted, MaxAge);
            }
        }
    }
}
