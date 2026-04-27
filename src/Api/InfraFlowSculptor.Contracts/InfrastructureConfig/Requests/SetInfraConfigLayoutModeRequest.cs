namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Sets or clears the per-configuration layout mode (only meaningful when project is in MultiRepo).</summary>
public sealed class SetInfraConfigLayoutModeRequest
{
    /// <summary>"AllInOne", "SplitInfraCode" or null/empty to clear.</summary>
    public string? Mode { get; init; }
}
