using InfraFlowSculptor.Application.RoleAssignments.Commands.AddRoleAssignment;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Contracts.RoleAssignments.Requests;
using InfraFlowSculptor.Contracts.RoleAssignments.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

public class RoleAssignmentMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<(Guid SourceResourceId, AddRoleAssignmentRequest Request), AddRoleAssignmentCommand>()
            .MapWith(src => new AddRoleAssignmentCommand(
                new AzureResourceId(src.SourceResourceId),
                new AzureResourceId(src.Request.TargetResourceId),
                src.Request.ManagedIdentityType,
                src.Request.RoleDefinitionId));

        config.NewConfig<ManagedIdentityType, string>()
            .MapWith(src => src.Value.ToString());

        config.NewConfig<RoleAssignmentId, Guid>()
            .MapWith(src => src.Value);

        config.NewConfig<RoleAssignmentResult, RoleAssignmentResponse>()
            .MapWith(src => new RoleAssignmentResponse(
                src.Id.Value,
                src.SourceResourceId.Value,
                src.TargetResourceId.Value,
                src.ManagedIdentityType.Value.ToString(),
                src.RoleDefinitionId));
    }
}
