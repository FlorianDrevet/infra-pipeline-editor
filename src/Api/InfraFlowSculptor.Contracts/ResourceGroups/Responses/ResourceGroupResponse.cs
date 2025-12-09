namespace InfraFlowSculptor.Contracts.ResourceGroups.Responses;

public record ResourceGroupResponse(
    Guid Id,
    Guid InfraConfigId,
    string Name,
    string Location
);
