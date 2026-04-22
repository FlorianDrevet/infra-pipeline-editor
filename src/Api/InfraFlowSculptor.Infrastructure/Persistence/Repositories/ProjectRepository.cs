using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectPipelineVariableGroups;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using InfraFlowSculptor.Infrastructure.Persistence.Repositories;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IProjectRepository"/>.</summary>
public sealed class ProjectRepository(ProjectDbContext context)
    : BaseRepository<Project, ProjectDbContext>(context), IProjectRepository
{
    /// <inheritdoc />
    public async Task<Project?> GetByIdWithMembersAsync(
        ProjectId id, CancellationToken cancellationToken = default)
        => await Context.Projects
            .Include(p => p.Members)
                .ThenInclude(m => m.User!)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<Project?> GetByIdWithAllAsync(
        ProjectId id, CancellationToken cancellationToken = default)
        => await Context.Projects
            .Include(p => p.Members)
                .ThenInclude(m => m.User!)
            .Include(p => p.EnvironmentDefinitions)
            .Include(p => p.ResourceNamingTemplates)
            .Include(p => p.ResourceAbbreviations)
            .Include(p => p.GitRepositoryConfiguration)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<Project?> GetByIdWithPipelineVariableGroupsAsync(
        ProjectId id, CancellationToken cancellationToken = default)
        => await Context.Projects
            .Include(p => p.PipelineVariableGroups)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<List<Project>> GetAllForUserAsync(
        UserId userId, CancellationToken cancellationToken = default)
        => await Context.Projects
            .AsNoTracking()
            .Include(p => p.Members)
                .ThenInclude(m => m.User!)
            .Where(p => p.Members.Any(m => m.UserId == userId))
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<List<ProjectId>> GetProjectIdsForUserAsync(
        UserId userId, CancellationToken cancellationToken = default)
        => await Context.Projects
            .AsNoTracking()
            .Where(p => p.Members.Any(m => m.UserId == userId))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<List<ProjectSummary>> GetProjectSummariesForUserAsync(
        UserId userId, CancellationToken cancellationToken = default)
        => await Context.Projects
            .AsNoTracking()
            .Where(p => p.Members.Any(m => m.UserId == userId))
            .Select(p => new ProjectSummary(p.Id.Value, p.Name.Value, p.Description))
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<Dictionary<Guid, List<PipelineVariableUsageResult>>> GetPipelineVariableUsagesAsync(
        IReadOnlyCollection<ProjectPipelineVariableGroupId> variableGroupIds,
        CancellationToken cancellationToken = default)
    {
        if (variableGroupIds.Count == 0)
            return [];

        // AppSettings usages
        var appSettingUsages = await Context.AppSettings
            .AsNoTracking()
            .Where(a => a.VariableGroupId != null && variableGroupIds.Contains(a.VariableGroupId))
            .Select(a => new
            {
                GroupId = a.VariableGroupId!.Value,
                a.PipelineVariableName,
                SettingName = a.Name,
                a.ResourceId,
            })
            .ToListAsync(cancellationToken);

        // SecureParameterMappings usages
        var secureParamUsages = await Context.SecureParameterMappings
            .AsNoTracking()
            .Where(s => s.VariableGroupId != null && variableGroupIds.Contains(s.VariableGroupId))
            .Select(s => new
            {
                GroupId = s.VariableGroupId!.Value,
                s.PipelineVariableName,
                SettingName = s.SecureParameterName,
                s.ResourceId,
            })
            .ToListAsync(cancellationToken);

        // AppConfigurationKeys usages
        var appConfigKeyUsages = await Context.AppConfigurationKeys
            .AsNoTracking()
            .Where(k => k.VariableGroupId != null && variableGroupIds.Contains(k.VariableGroupId))
            .Select(k => new
            {
                GroupId = k.VariableGroupId!.Value,
                k.PipelineVariableName,
                SettingName = k.Key,
                ResourceId = k.AppConfigurationId,
            })
            .ToListAsync(cancellationToken);

        // Collect all unique resource IDs
        var allResourceIds = appSettingUsages.Select(u => u.ResourceId)
            .Concat(secureParamUsages.Select(u => u.ResourceId))
            .Concat(appConfigKeyUsages.Select(u => u.ResourceId))
            .Distinct()
            .ToList();

        // Batch-load resource info with ResourceGroup → InfrastructureConfig
        var resourceLookup = await Context.AzureResources
            .AsNoTracking()
            .Include(r => r.ResourceGroup)
                .ThenInclude(rg => rg.InfraConfig)
            .Where(r => allResourceIds.Contains(r.Id))
            .ToDictionaryAsync(
                r => r.Id.Value,
                r => new
                {
                    ResourceName = r.Name.Value,
                    r.ResourceType,
                    ConfigName = r.ResourceGroup.InfraConfig.Name.Value,
                },
                cancellationToken);

        // Build results grouped by variable group ID
        var result = new Dictionary<Guid, List<PipelineVariableUsageResult>>();

        void AddUsages<T>(IEnumerable<T> usages, Func<T, Guid> getGroupId, Func<T, string?> getVarName,
            Func<T, string> getSettingName, Func<T, Domain.Common.BaseModels.ValueObjects.AzureResourceId> getResourceId)
        {
            foreach (var usage in usages)
            {
                var varName = getVarName(usage);
                if (string.IsNullOrEmpty(varName))
                    continue;

                var groupId = getGroupId(usage);
                var resourceId = getResourceId(usage);

                if (!resourceLookup.TryGetValue(resourceId.Value, out var info))
                    continue;

                if (!result.TryGetValue(groupId, out var list))
                {
                    list = [];
                    result[groupId] = list;
                }

                list.Add(new PipelineVariableUsageResult(
                    varName,
                    getSettingName(usage),
                    info.ResourceName,
                    info.ResourceType,
                    info.ConfigName));
            }
        }

        AddUsages(appSettingUsages, u => u.GroupId, u => u.PipelineVariableName, u => u.SettingName,
            u => u.ResourceId);
        AddUsages(secureParamUsages, u => u.GroupId, u => u.PipelineVariableName, u => u.SettingName,
            u => u.ResourceId);
        AddUsages(appConfigKeyUsages, u => u.GroupId, u => u.PipelineVariableName, u => u.SettingName,
            u => u.ResourceId);

        return result;
    }
}
