using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInfraConfigLayoutMode;

/// <summary>Sets or clears the per-configuration <see cref="ConfigLayoutMode"/>. Only meaningful when the parent project layout is MultiRepo.</summary>
/// <param name="ProjectId">Parent project id (used for authorization).</param>
/// <param name="ConfigId">Target infrastructure configuration id.</param>
/// <param name="Mode">"AllInOne" or "SplitInfraCode" or null/empty to clear.</param>
public sealed record SetInfraConfigLayoutModeCommand(
    ProjectId ProjectId,
    InfrastructureConfigId ConfigId,
    string? Mode) : IRequest<ErrorOr<Updated>>;
