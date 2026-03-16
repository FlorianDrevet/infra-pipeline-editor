namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

public record EnvironmentDefinitionResponse(
    string Id,
    string Name,
    string Prefix,
    string Suffix,
    string Location,
    Guid TenantId,
    Guid SubscriptionId,
    int Order,
    bool RequiresApproval,
    IReadOnlyList<TagResponse> Tags);

public record TagResponse(string Name, string Value);
