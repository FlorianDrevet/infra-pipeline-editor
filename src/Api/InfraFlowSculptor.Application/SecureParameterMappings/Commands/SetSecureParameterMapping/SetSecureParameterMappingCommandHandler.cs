using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.SecureParameterMappings.Commands.SetSecureParameterMapping;

/// <summary>Handles the <see cref="SetSecureParameterMappingCommand"/> request.</summary>
public sealed class SetSecureParameterMappingCommandHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IProjectRepository projectRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<SetSecureParameterMappingCommand, Updated>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Updated>> Handle(
        SetSecureParameterMappingCommand request,
        CancellationToken cancellationToken)
    {
        var resource = await azureResourceRepository.GetByIdWithSecureParameterMappingsAsync(
            request.ResourceId, cancellationToken);

        if (resource is null)
            return Errors.SecureParameterMapping.ResourceNotFound(request.ResourceId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(
            resource.ResourceGroupId, cancellationToken);

        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(resource.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(
            resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        // Verify the variable group exists on the project when provided
        if (request.VariableGroupId is not null)
        {
            var infraConfig = authResult.Value;
            var project = await projectRepository.GetByIdWithPipelineVariableGroupsAsync(
                infraConfig.ProjectId, cancellationToken);

            if (project is null)
                return Errors.Project.NotFoundError(infraConfig.ProjectId);

            var variableGroupExists = project.PipelineVariableGroups
                .Any(g => g.Id == request.VariableGroupId);

            if (!variableGroupExists)
                return Errors.SecureParameterMapping.VariableGroupNotFound(request.VariableGroupId);
        }

        var result = resource.SetSecureParameterMapping(
            request.SecureParameterName,
            request.VariableGroupId,
            request.PipelineVariableName);

        if (result.IsError)
            return result.Errors;

        await azureResourceRepository.UpdateAsync(resource, cancellationToken);

        return Result.Updated;
    }
}
