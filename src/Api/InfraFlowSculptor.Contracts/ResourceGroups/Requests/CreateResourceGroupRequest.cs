namespace InfraFlowSculptor.Contracts.ResourceGroups.Requests;

public record CreateResourceGroupRequest(
    Guid InfraConfigId,
    string Name,
    string Location
);
