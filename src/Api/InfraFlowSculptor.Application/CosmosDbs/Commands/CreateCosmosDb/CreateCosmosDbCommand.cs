using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.CosmosDbs.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using ErrorOr;

namespace InfraFlowSculptor.Application.CosmosDbs.Commands.CreateCosmosDb;

/// <summary>Command to create a new Cosmos DB account resource inside a Resource Group.</summary>
/// <param name="ResourceGroupId">The parent resource group identifier.</param>
/// <param name="Name">The display name.</param>
/// <param name="Location">The Azure region.</param>
/// <param name="EnvironmentSettings">Optional per-environment configuration overrides.</param>
public record CreateCosmosDbCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyList<CosmosDbEnvironmentConfigData>? EnvironmentSettings = null,
    bool IsExisting = false
) : ICommand<CosmosDbResult>;
