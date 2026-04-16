using InfraFlowSculptor.Application.RoleAssignments.Commands.AddRoleAssignment;
using InfraFlowSculptor.Application.RoleAssignments.Commands.AssignIdentityToResource;
using InfraFlowSculptor.Application.RoleAssignments.Commands.UpdateRoleAssignmentIdentity;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Contracts.RoleAssignments.Requests;
using InfraFlowSculptor.Contracts.RoleAssignments.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for role assignment request/response types.</summary>
public sealed class RoleAssignmentMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<(Guid SourceResourceId, AddRoleAssignmentRequest Request), AddRoleAssignmentCommand>()
            .MapWith(src => new AddRoleAssignmentCommand(
                new AzureResourceId(src.SourceResourceId),
                new AzureResourceId(src.Request.TargetResourceId),
                src.Request.ManagedIdentityType,
                src.Request.RoleDefinitionId,
                src.Request.UserAssignedIdentityId.HasValue
                    ? new AzureResourceId(src.Request.UserAssignedIdentityId.Value)
                    : null));

        config.NewConfig<(Guid SourceResourceId, Guid RoleAssignmentId, UpdateRoleAssignmentIdentityRequest Request), UpdateRoleAssignmentIdentityCommand>()
            .MapWith(src => new UpdateRoleAssignmentIdentityCommand(
                new AzureResourceId(src.SourceResourceId),
                new RoleAssignmentId(src.RoleAssignmentId),
                src.Request.ManagedIdentityType,
                src.Request.UserAssignedIdentityId.HasValue
                    ? new AzureResourceId(src.Request.UserAssignedIdentityId.Value)
                    : null));

        config.NewConfig<(Guid ResourceId, AssignIdentityToResourceRequest Request), AssignIdentityToResourceCommand>()
            .MapWith(src => new AssignIdentityToResourceCommand(
                new AzureResourceId(src.ResourceId),
                new AzureResourceId(src.Request.UserAssignedIdentityId)));

        config.NewConfig<ManagedIdentityType, string>()
            .MapWith(src => src.Value.ToString());

        config.NewConfig<RoleAssignmentId, Guid>()
            .MapWith(src => src.Value);

        config.NewConfig<RoleAssignmentId, string>()
            .MapWith(src => src.Value.ToString());

        config.NewConfig<RoleAssignmentResult, RoleAssignmentResponse>()
            .MapWith(src => new RoleAssignmentResponse(
                src.Id.Value.ToString(),
                src.SourceResourceId.Value.ToString(),
                src.TargetResourceId.Value.ToString(),
                src.ManagedIdentityType.Value.ToString(),
                src.RoleDefinitionId,
                src.UserAssignedIdentityId != null ? src.UserAssignedIdentityId.Value.ToString() : null));

        config.NewConfig<AzureRoleDefinitionResult, AzureRoleDefinitionResponse>()
            .MapWith(src => new AzureRoleDefinitionResponse(
                src.Id,
                src.Name,
                src.Description,
                src.DocumentationUrl));

        config.NewConfig<IdentityRoleAssignmentResult, IdentityRoleAssignmentResponse>()
            .MapWith(src => new IdentityRoleAssignmentResponse(
                src.Id.Value.ToString(),
                src.SourceResourceId.Value.ToString(),
                src.SourceResourceName,
                src.SourceResourceType,
                src.TargetResourceId.Value.ToString(),
                src.TargetResourceName,
                src.TargetResourceType,
                src.RoleDefinitionId,
                src.RoleName));

        config.NewConfig<RoleAssignmentImpactItem, RoleAssignmentImpactItemResponse>()
            .MapWith(src => new RoleAssignmentImpactItemResponse(
                src.AffectedResourceId.ToString(),
                src.AffectedResourceName,
                src.AffectedResourceType,
                src.TargetResourceId.ToString(),
                src.TargetResourceName,
                src.TargetResourceType,
                src.ImpactType,
                src.Description,
                src.Severity,
                src.AffectedSettingsCount));

        config.NewConfig<RoleAssignmentImpactResult, RoleAssignmentImpactResponse>()
            .MapWith(src => new RoleAssignmentImpactResponse(
                src.HasImpact,
                src.Impacts.Select(i => new RoleAssignmentImpactItemResponse(
                    i.AffectedResourceId.ToString(),
                    i.AffectedResourceName,
                    i.AffectedResourceType,
                    i.TargetResourceId.ToString(),
                    i.TargetResourceName,
                    i.TargetResourceType,
                    i.ImpactType,
                    i.Description,
                    i.Severity,
                    i.AffectedSettingsCount)).ToList()));
    }
}
