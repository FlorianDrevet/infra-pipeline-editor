using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.SecureParameterMappings.Commands.SetSecureParameterMapping;

/// <summary>Sets or clears a secure parameter mapping on an Azure resource.</summary>
/// <param name="ResourceId">Identifier of the Azure resource.</param>
/// <param name="SecureParameterName">Name of the secure Bicep parameter.</param>
/// <param name="VariableGroupId">Optional pipeline variable group identifier. <c>null</c> to clear.</param>
/// <param name="PipelineVariableName">Optional pipeline variable name. <c>null</c> to clear.</param>
public record SetSecureParameterMappingCommand(
    AzureResourceId ResourceId,
    string SecureParameterName,
    ProjectPipelineVariableGroupId? VariableGroupId,
    string? PipelineVariableName) : ICommand<Updated>;
