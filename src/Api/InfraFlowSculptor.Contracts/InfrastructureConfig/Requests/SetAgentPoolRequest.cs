namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Request body for setting or clearing the self-hosted agent pool for pipeline generation.</summary>
public class SetAgentPoolRequest
{
    /// <summary>
    /// The name of the self-hosted agent pool. When null or empty, pipelines use the Microsoft-hosted pool.
    /// </summary>
    public string? AgentPoolName { get; init; }
}
