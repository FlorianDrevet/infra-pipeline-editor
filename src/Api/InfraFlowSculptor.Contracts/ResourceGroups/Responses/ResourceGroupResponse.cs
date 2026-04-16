namespace InfraFlowSculptor.Contracts.ResourceGroups.Responses;

/// <summary>Represents an Azure Resource Group that belongs to an Infrastructure Configuration.</summary>
/// <param name="Id">Unique identifier of the Resource Group.</param>
/// <param name="InfraConfigId">Identifier of the parent Infrastructure Configuration.</param>
/// <param name="Name">Display name of the Resource Group.</param>
/// <param name="Location">Azure region where the Resource Group is deployed.</param>
public record ResourceGroupResponse(
    string Id,
    string InfraConfigId,
    string Name,
    string Location
);
