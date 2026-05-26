using System.Diagnostics;
using InfraFlowSculptor.Application.Common.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InfraFlowSculptor.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that logs the start, completion, and failures of MediatR requests,
/// and records execution metrics for business observability.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ApplicationMetrics metrics) :
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var isCommand = requestName.Contains("Command", StringComparison.Ordinal);

        logger.LogInformation("Handling {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();

        try
        {
#pragma warning disable CA2016 // RequestHandlerDelegate has no CancellationToken parameter
            var response = await next();
#pragma warning restore CA2016

            stopwatch.Stop();
            var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;

            logger.LogInformation(
                "Handled {RequestName} in {ElapsedMs:F1}ms",
                requestName,
                elapsedMs);

            if (isCommand)
            {
                metrics.RecordCommandExecuted(requestName, elapsedMs);
            }
            else
            {
                metrics.RecordQueryExecuted(requestName, elapsedMs);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "Request {RequestName} failed after {ElapsedMs:F1}ms",
                requestName,
                stopwatch.Elapsed.TotalMilliseconds);

            if (isCommand)
            {
                metrics.RecordCommandFailed(requestName);
            }

            throw;
        }
    }
}
