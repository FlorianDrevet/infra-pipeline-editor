using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.SecureParameterMappings.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.SecureParameterMappings.Queries.GetSecureParameterMappings;

/// <summary>Handles the <see cref="GetSecureParameterMappingsQuery"/> request.</summary>
public sealed class GetSecureParameterMappingsQueryHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IProjectRepository projectRepository)
    : IQueryHandler<GetSecureParameterMappingsQuery, IReadOnlyList<SecureParameterMappingResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<IReadOnlyList<SecureParameterMappingResult>>> Handle(
        GetSecureParameterMappingsQuery request,
        CancellationToken cancellationToken)
    {
        var resource = await azureResourceRepository.GetByIdWithSecureParameterMappingsAsync(
            request.ResourceId, cancellationToken);

        if (resource is null)
            return Errors.SecureParameterMapping.ResourceNotFound(request.ResourceId);

        // Resolve VG group names if any mapping references a variable group
        var vgIds = resource.SecureParameterMappings
            .Where(m => m.VariableGroupId is not null)
            .Select(m => m.VariableGroupId!)
            .Distinct()
            .ToList();

        Dictionary<ProjectPipelineVariableGroupId, string> vgNameLookup = [];

        if (vgIds.Count > 0)
        {
            var resourceGroup = await resourceGroupRepository.GetByIdAsync(
                resource.ResourceGroupId, cancellationToken);

            if (resourceGroup is not null)
            {
                var authResult = await accessService.VerifyReadAccessAsync(
                    resourceGroup.InfraConfigId, cancellationToken);

                if (!authResult.IsError)
                {
                    var project = await projectRepository.GetByIdWithPipelineVariableGroupsAsync(
                        authResult.Value.ProjectId, cancellationToken);

                    if (project is not null)
                    {
                        vgNameLookup = project.PipelineVariableGroups
                            .Where(g => vgIds.Contains(g.Id))
                            .ToDictionary(g => g.Id, g => g.GroupName);
                    }
                }
            }
        }

        var result = resource.SecureParameterMappings
            .Select(m => new SecureParameterMappingResult(
                m.Id.Value,
                m.SecureParameterName,
                m.VariableGroupId?.Value,
                m.VariableGroupId is not null && vgNameLookup.TryGetValue(m.VariableGroupId, out var name)
                    ? name
                    : null,
                m.PipelineVariableName))
            .ToList();

        return result;
    }
}
