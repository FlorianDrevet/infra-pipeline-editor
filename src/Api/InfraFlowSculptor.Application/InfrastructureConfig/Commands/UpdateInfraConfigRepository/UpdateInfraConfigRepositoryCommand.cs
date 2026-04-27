using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.UpdateInfraConfigRepository;

/// <summary>Updates an existing InfraConfigRepository.</summary>
public sealed record UpdateInfraConfigRepositoryCommand(
    ProjectId ProjectId,
    InfrastructureConfigId ConfigId,
    InfraConfigRepositoryId RepositoryId,
    string ProviderType,
    string RepositoryUrl,
    string DefaultBranch,
    IReadOnlyList<string> ContentKinds) : IRequest<ErrorOr<Updated>>;
