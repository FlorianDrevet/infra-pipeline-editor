using InfraFlowSculptor.Application.Projects;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectResources;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.ReadRepositories;

/// <summary>
/// EF Core read repository for lightweight project resource list projections.
/// </summary>
public sealed class ProjectResourceReadRepository(ProjectDbContext context) : IProjectResourceReadRepository
{
    /// <inheritdoc />
    public Task<List<ProjectResourceResult>> GetByProjectIdAsync(
        ProjectId projectId,
        CancellationToken cancellationToken = default)
        => GetResourceProjectionsAsync(projectId, cancellationToken);

    private async Task<List<ProjectResourceResult>> GetResourceProjectionsAsync(
        ProjectId projectId,
        CancellationToken cancellationToken)
    {
        var projections = await context.AzureResources
            .AsNoTracking()
            .Where(resource => resource.ResourceGroup!.InfraConfig.ProjectId == projectId)
            .Select(resource => new ProjectResourceProjection(
                resource.Id.Value,
                resource.Name.Value,
                resource.ResourceType,
                resource.ResourceGroup!.Name.Value,
                resource.ResourceGroup.InfraConfigId.Value,
                resource.ResourceGroup.InfraConfig.Name.Value))
            .ToListAsync(cancellationToken);

        return projections
            .Select(projection => new ProjectResourceResult(
                projection.ResourceId,
                projection.ResourceName,
                projection.ResourceType.Value,
                projection.ResourceGroupName,
                projection.ConfigId,
                projection.ConfigName))
            .ToList();
    }

    private sealed record ProjectResourceProjection(
        Guid ResourceId,
        string ResourceName,
        ResourceTypeName ResourceType,
        string ResourceGroupName,
        Guid ConfigId,
        string ConfigName);
}