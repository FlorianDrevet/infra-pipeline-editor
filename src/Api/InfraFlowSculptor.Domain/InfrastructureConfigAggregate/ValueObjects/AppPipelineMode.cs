namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

/// <summary>Defines how application pipelines are generated for compute resources in a configuration.</summary>
public enum AppPipelineMode
{
    /// <summary>Each compute resource gets its own isolated CI/release pipeline files.</summary>
    Isolated,

    /// <summary>All compute resources share a single combined CI/release pipeline with parallel jobs.</summary>
    Combined
}
