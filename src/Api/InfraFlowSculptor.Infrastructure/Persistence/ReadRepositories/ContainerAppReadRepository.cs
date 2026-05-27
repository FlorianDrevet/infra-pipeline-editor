using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.ContainerApps;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Contracts.Common.Requests;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.OwnedEntities;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.ReadRepositories;

/// <summary>
/// EF Core read repository for lightweight Container App detail projections.
/// </summary>
public sealed class ContainerAppReadRepository(ProjectDbContext context) : IContainerAppReadRepository
{
    /// <inheritdoc />
    public async Task<ContainerAppDetailReadResult?> GetByIdAsync(
        AzureResourceId id,
        CancellationToken cancellationToken = default)
    {
        var projection = await context.Set<ContainerApp>()
            .AsNoTracking()
            .Where(containerApp => containerApp.Id == id)
            .Select(containerApp => new ContainerAppDetailProjection(
                containerApp.Id,
                containerApp.ResourceGroupId,
                containerApp.ResourceGroup!.InfraConfigId,
                containerApp.Name,
                containerApp.Location,
                containerApp.ContainerAppEnvironmentId.Value,
                containerApp.ContainerRegistryId == null ? null : containerApp.ContainerRegistryId.Value,
                containerApp.AcrAuthMode == null ? null : containerApp.AcrAuthMode.Value.ToString(),
                containerApp.AcrPullIdentityId == null ? null : containerApp.AcrPullIdentityId.Value,
                containerApp.DockerImageName,
                containerApp.DockerImageValidated,
                containerApp.DockerfilePath,
                containerApp.ApplicationName,
                containerApp.SourceCodePath,
                containerApp.PipelineStepOptions,
                containerApp.IsExisting))
            .FirstOrDefaultAsync(cancellationToken);

        if (projection is null)
            return null;

        var environmentSettings = await context.ContainerAppEnvironmentSettings
            .AsNoTracking()
            .Where(settings => settings.ContainerAppId == id)
            .Select(settings => new ContainerAppEnvironmentConfigData(
                settings.EnvironmentName,
                settings.CpuCores,
                settings.MemoryGi,
                settings.MinReplicas,
                settings.MaxReplicas,
                settings.IngressEnabled,
                settings.IngressTargetPort,
                settings.IngressExternal,
                settings.TransportMethod,
                settings.ReadinessProbePath,
                settings.ReadinessProbePort,
                settings.LivenessProbePath,
                settings.LivenessProbePort,
                settings.StartupProbePath,
                settings.StartupProbePort,
                settings.ContainerRegistryServiceConnection))
            .ToListAsync(cancellationToken);

        var result = new ContainerAppResult(
            projection.Id,
            projection.ResourceGroupId,
            projection.Name,
            projection.Location,
            projection.ContainerAppEnvironmentId,
            projection.ContainerRegistryId,
            projection.AcrAuthMode,
            projection.AcrPullIdentityId,
            projection.DockerImageName,
            projection.DockerImageValidated,
            projection.DockerfilePath,
            projection.ApplicationName,
            projection.SourceCodePath,
            MapPipelineStepOptions(projection.PipelineStepOptions),
            environmentSettings,
            projection.IsExisting);

        return new ContainerAppDetailReadResult(result, projection.InfraConfigId);
    }

    private static PipelineStepOptionsDto? MapPipelineStepOptions(AppPipelineStepOptions? options)
        => options is null ? null : PipelineStepOptionsDataMapper.ToDto(options);

    private sealed record ContainerAppDetailProjection(
        AzureResourceId Id,
        ResourceGroupId ResourceGroupId,
        InfrastructureConfigId InfraConfigId,
        Name Name,
        Location Location,
        Guid ContainerAppEnvironmentId,
        Guid? ContainerRegistryId,
        string? AcrAuthMode,
        Guid? AcrPullIdentityId,
        string? DockerImageName,
        bool DockerImageValidated,
        string? DockerfilePath,
        string? ApplicationName,
        string? SourceCodePath,
        AppPipelineStepOptions PipelineStepOptions,
        bool IsExisting);
}