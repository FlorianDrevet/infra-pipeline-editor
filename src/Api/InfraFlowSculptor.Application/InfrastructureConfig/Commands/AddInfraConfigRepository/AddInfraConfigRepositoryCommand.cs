using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddInfraConfigRepository;

/// <summary>Adds a Git repository to an InfrastructureConfig (MultiRepo project layout only).</summary>
public sealed record AddInfraConfigRepositoryCommand(
    ProjectId ProjectId,
    InfrastructureConfigId ConfigId,
    string Alias,
    string ProviderType,
    string RepositoryUrl,
    string DefaultBranch,
    IReadOnlyList<string> ContentKinds) : IRequest<ErrorOr<InfraConfigRepositoryId>>;
