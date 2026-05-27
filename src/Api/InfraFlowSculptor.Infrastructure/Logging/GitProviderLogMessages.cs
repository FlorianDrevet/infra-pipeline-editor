using Microsoft.Extensions.Logging;

namespace InfraFlowSculptor.Infrastructure.Logging;

/// <summary>
/// High-performance log messages for Git provider operations using source generation.
/// </summary>
internal static partial class GitProviderLogMessages
{
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "Git push to {Provider}/{Repository} branch {Branch} completed in {ElapsedMs:F1}ms")]
    public static partial void GitPushCompleted(
        this ILogger logger,
        string provider,
        string repository,
        string branch,
        double elapsedMs);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Error,
        Message = "Git push to {Provider}/{Repository} branch {Branch} failed")]
    public static partial void GitPushFailed(
        this ILogger logger,
        Exception exception,
        string provider,
        string repository,
        string branch);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Information,
        Message = "Fetching tree from {Provider}/{Repository} at ref {Ref}")]
    public static partial void FetchingTree(
        this ILogger logger,
        string provider,
        string repository,
        string @ref);
}
