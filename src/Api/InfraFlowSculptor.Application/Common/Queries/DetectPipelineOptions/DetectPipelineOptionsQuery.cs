using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Queries.DetectPipelineOptions;

/// <summary>
/// Query to auto-detect pipeline options from a compute resource's source repository.
/// </summary>
public record DetectPipelineOptionsQuery(
    AzureResourceId ResourceId
) : IQuery<DetectedPipelineOptionsResult>;
