using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.ResourceGroups.Common;

public record ResourceGroupResult(ResourceGroupId Id, InfrastructureConfigId InfraConfigId, Location Location, Name Name, AzureResource[] Resources);