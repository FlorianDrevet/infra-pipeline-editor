using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.ResourceGroups.Common;

public record AzureResourceResult(
    AzureResourceId Id,
    string ResourceType,
    Name Name,
    Location Location);
