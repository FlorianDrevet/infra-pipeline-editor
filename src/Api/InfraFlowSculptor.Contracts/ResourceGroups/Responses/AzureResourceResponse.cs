namespace InfraFlowSculptor.Contracts.ResourceGroups.Responses;

public record AzureResourceResponse(
    Guid Id,
    string ResourceType,
    string Name,
    string Location);
