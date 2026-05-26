using Microsoft.Extensions.Logging;

namespace InfraFlowSculptor.Infrastructure.Logging;

/// <summary>
/// High-performance log messages for persistence operations using source generation.
/// </summary>
internal static partial class PersistenceLogMessages
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Unit of work committed {ChangeCount} changes for request {RequestName}")]
    public static partial void UnitOfWorkCommitted(
        this ILogger logger,
        int changeCount,
        string requestName);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Unit of work commit took {ElapsedMs:F1}ms for request {RequestName} — exceeds threshold")]
    public static partial void UnitOfWorkSlowCommit(
        this ILogger logger,
        double elapsedMs,
        string requestName);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Error,
        Message = "Database migration failed for context {ContextName}")]
    public static partial void MigrationFailed(
        this ILogger logger,
        Exception exception,
        string contextName);
}
