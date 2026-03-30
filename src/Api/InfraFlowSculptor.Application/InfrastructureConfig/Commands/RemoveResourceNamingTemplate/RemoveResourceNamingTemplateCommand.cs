using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveResourceNamingTemplate;

public record RemoveResourceNamingTemplateCommand(
    InfrastructureConfigId InfraConfigId,
    string ResourceType
) : ICommand<Deleted>;
