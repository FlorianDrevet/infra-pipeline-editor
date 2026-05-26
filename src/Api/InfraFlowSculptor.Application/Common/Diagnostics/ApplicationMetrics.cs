using System.Diagnostics.Metrics;

namespace InfraFlowSculptor.Application.Common.Diagnostics;

/// <summary>
/// Centralized application-level business metrics exposed via OpenTelemetry.
/// </summary>
public sealed class ApplicationMetrics : IDisposable
{
    /// <summary>
    /// The meter name registered with the OpenTelemetry SDK.
    /// </summary>
    public const string MeterName = "InfraFlowSculptor";

    private readonly Meter _meter;
    private readonly Counter<long> _commandsExecuted;
    private readonly Counter<long> _queriesExecuted;
    private readonly Counter<long> _commandsFailed;
    private readonly Histogram<double> _commandDuration;
    private readonly Histogram<double> _queryDuration;
    private readonly Counter<long> _bicepGenerations;
    private readonly Counter<long> _pipelineGenerations;

    public ApplicationMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(MeterName);

        _commandsExecuted = _meter.CreateCounter<long>(
            "infraflow.commands.executed",
            description: "Total number of MediatR commands executed successfully");

        _queriesExecuted = _meter.CreateCounter<long>(
            "infraflow.queries.executed",
            description: "Total number of MediatR queries executed successfully");

        _commandsFailed = _meter.CreateCounter<long>(
            "infraflow.commands.failed",
            description: "Total number of MediatR commands that resulted in an error");

        _commandDuration = _meter.CreateHistogram<double>(
            "infraflow.commands.duration",
            unit: "ms",
            description: "Duration of MediatR command execution in milliseconds");

        _queryDuration = _meter.CreateHistogram<double>(
            "infraflow.queries.duration",
            unit: "ms",
            description: "Duration of MediatR query execution in milliseconds");

        _bicepGenerations = _meter.CreateCounter<long>(
            "infraflow.bicep.generations",
            description: "Total number of Bicep generation operations");

        _pipelineGenerations = _meter.CreateCounter<long>(
            "infraflow.pipeline.generations",
            description: "Total number of pipeline generation operations");
    }

    /// <summary>
    /// Records a successful command execution with its duration.
    /// </summary>
    public void RecordCommandExecuted(string commandName, double durationMs)
    {
        _commandsExecuted.Add(1, new KeyValuePair<string, object?>("command.name", commandName));
        _commandDuration.Record(durationMs, new KeyValuePair<string, object?>("command.name", commandName));
    }

    /// <summary>
    /// Records a successful query execution with its duration.
    /// </summary>
    public void RecordQueryExecuted(string queryName, double durationMs)
    {
        _queriesExecuted.Add(1, new KeyValuePair<string, object?>("query.name", queryName));
        _queryDuration.Record(durationMs, new KeyValuePair<string, object?>("query.name", queryName));
    }

    /// <summary>
    /// Records a failed command execution.
    /// </summary>
    public void RecordCommandFailed(string commandName)
    {
        _commandsFailed.Add(1, new KeyValuePair<string, object?>("command.name", commandName));
    }

    /// <summary>
    /// Records a Bicep generation operation.
    /// </summary>
    public void RecordBicepGeneration()
    {
        _bicepGenerations.Add(1);
    }

    /// <summary>
    /// Records a pipeline generation operation.
    /// </summary>
    public void RecordPipelineGeneration()
    {
        _pipelineGenerations.Add(1);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _meter.Dispose();
    }
}
