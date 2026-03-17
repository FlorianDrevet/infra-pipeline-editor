using ErrorOr;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveResourceNamingTemplate;

public record RemoveResourceNamingTemplateCommand(
    InfrastructureConfigId InfraConfigId,
    string ResourceType
) : IRequest<ErrorOr<Deleted>>;
