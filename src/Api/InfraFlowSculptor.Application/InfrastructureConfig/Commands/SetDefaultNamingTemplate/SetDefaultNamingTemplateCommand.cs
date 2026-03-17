using ErrorOr;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetDefaultNamingTemplate;

/// <summary>
/// Sets or clears the default naming template for all resource types in an InfrastructureConfig.
/// Pass null for Template to clear the default.
/// </summary>
public record SetDefaultNamingTemplateCommand(
    InfrastructureConfigId InfraConfigId,
    string? Template
) : IRequest<ErrorOr<Updated>>;
