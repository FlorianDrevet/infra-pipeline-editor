using System.Text.RegularExpressions;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Services;

/// <summary>
/// Default implementation of <see cref="IResourceNameResolver"/> that applies the
/// project/config naming templates and substitutes the supported placeholders.
/// </summary>
public sealed class ResourceNameResolver(
    IProjectRepository projectRepository,
    IInfrastructureConfigRepository infraConfigRepository) : IResourceNameResolver
{
    private static readonly Regex PlaceholderRegex = new(
        @"\{([^}]+)\}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(250));

    /// <inheritdoc />
    public async Task<ErrorOr<IReadOnlyList<ResolvedResourceName>>> ResolveAsync(
        ProjectId projectId,
        InfrastructureConfigId? configId,
        string resourceType,
        string rawName,
        CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetByIdWithAllAsync(projectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(projectId);

        Domain.InfrastructureConfigAggregate.InfrastructureConfig? infraConfig = null;
        if (configId is not null)
        {
            infraConfig = await infraConfigRepository.GetByIdWithNamingTemplatesAsync(configId, cancellationToken);
            if (infraConfig is null)
                return Errors.InfrastructureConfig.NotFoundError(configId);
        }

        if (project.EnvironmentDefinitions.Count == 0)
        {
            IReadOnlyList<ResolvedResourceName> empty = Array.Empty<ResolvedResourceName>();
            return ErrorOrFactory.From(empty);
        }

        var template = ResolveTemplate(project, infraConfig, resourceType);
        var abbreviation = ResourceAbbreviationCatalog.GetAbbreviation(resourceType);

        var results = new List<ResolvedResourceName>(project.EnvironmentDefinitions.Count);
        foreach (var env in project.EnvironmentDefinitions)
        {
            var generated = ApplyTemplate(template, env, resourceType, abbreviation, rawName);
            results.Add(new ResolvedResourceName(
                env.Name.Value,
                env.ShortName.Value,
                env.SubscriptionId.Value.ToString(),
                generated,
                template));
        }

        IReadOnlyList<ResolvedResourceName> resolved = results;
        return ErrorOrFactory.From(resolved);
    }

    /// <summary>
    /// Picks the template to apply following the documented precedence rules.
    /// </summary>
    private static string ResolveTemplate(
        Project project,
        Domain.InfrastructureConfigAggregate.InfrastructureConfig? infraConfig,
        string resourceType)
    {
        var useConfigOverride = infraConfig is not null && !infraConfig.UseProjectNamingConventions;

        if (useConfigOverride)
        {
            var configResource = infraConfig!.ResourceNamingTemplates
                .FirstOrDefault(t => string.Equals(t.ResourceType, resourceType, StringComparison.OrdinalIgnoreCase));
            if (configResource is not null)
                return configResource.Template.Value;
        }

        var projectResource = project.ResourceNamingTemplates
            .FirstOrDefault(t => string.Equals(t.ResourceType, resourceType, StringComparison.OrdinalIgnoreCase));
        if (projectResource is not null)
            return projectResource.Template.Value;

        if (useConfigOverride && infraConfig!.DefaultNamingTemplate is not null)
            return infraConfig.DefaultNamingTemplate.Value;

        if (project.DefaultNamingTemplate is not null)
            return project.DefaultNamingTemplate.Value;

        return "{name}";
    }

    /// <summary>
    /// Substitutes all supported placeholders in <paramref name="template"/>.
    /// </summary>
    private static string ApplyTemplate(
        string template,
        ProjectEnvironmentDefinition env,
        string resourceType,
        string resourceAbbreviation,
        string rawName)
    {
        return PlaceholderRegex.Replace(template, match =>
        {
            var key = match.Groups[1].Value.Trim();
            return key.ToLowerInvariant() switch
            {
                "name" => rawName,
                "prefix" => env.Prefix.Value,
                "suffix" => env.Suffix.Value,
                "env" => env.Name.Value,
                "envshort" => env.ShortName.Value,
                "location" => env.Location.Value.ToString(),
                "resourcetype" => resourceType,
                "resourceabbr" => resourceAbbreviation,
                _ => match.Value
            };
        });
    }
}
