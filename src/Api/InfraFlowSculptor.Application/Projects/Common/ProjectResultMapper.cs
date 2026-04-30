using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>
/// Maps <see cref="Project"/> aggregates to <see cref="ProjectResult"/> instances.
/// </summary>
public static class ProjectResultMapper
{
    /// <summary>
    /// Converts a <see cref="Project"/> aggregate into an application result model.
    /// </summary>
    /// <param name="project">The aggregate to convert.</param>
    /// <returns>The mapped <see cref="ProjectResult"/>.</returns>
    public static ProjectResult ToProjectResult(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        return new ProjectResult(
            project.Id,
            project.Name,
            project.Description,
            project.Members.Select(MapMember).ToList(),
            project.EnvironmentDefinitions.Select(MapEnvironmentDefinition).ToList(),
            project.DefaultNamingTemplate?.Value,
            project.ResourceNamingTemplates.Select(MapResourceNamingTemplate).ToList(),
            project.ResourceAbbreviations.Select(MapResourceAbbreviation).ToList(),
            project.Tags.Select(MapTag).ToList(),
            project.AgentPoolName,
            Repositories: project.Repositories.Select(MapRepository).ToList(),
            LayoutPreset: project.LayoutPreset.Value.ToString());
    }

    private static ProjectMemberResult MapMember(ProjectMember member)
    {
        var user = member.User;

        return new ProjectMemberResult(
            member.Id,
            member.UserId,
            user != null ? user.EntraId.Value : Guid.Empty,
            member.Role.Value.ToString(),
            user != null ? user.Name.FirstName : string.Empty,
            user != null ? user.Name.LastName : string.Empty);
    }

    private static ProjectEnvironmentDefinitionResult MapEnvironmentDefinition(ProjectEnvironmentDefinition environmentDefinition)
        => new(
            environmentDefinition.Id,
            environmentDefinition.Name,
            environmentDefinition.ShortName.Value,
            environmentDefinition.Prefix.Value,
            environmentDefinition.Suffix.Value,
            environmentDefinition.Location.Value.ToString(),
            environmentDefinition.SubscriptionId.Value,
            environmentDefinition.Order.Value,
            environmentDefinition.RequiresApproval.Value,
            environmentDefinition.AzureResourceManagerConnection,
            environmentDefinition.Tags.Select(MapTag).ToList());

    private static ProjectResourceNamingTemplateResult MapResourceNamingTemplate(ProjectResourceNamingTemplate namingTemplate)
        => new(
            namingTemplate.Id,
            namingTemplate.ResourceType,
            namingTemplate.Template.Value);

    private static ProjectResourceAbbreviationResult MapResourceAbbreviation(ProjectResourceAbbreviation abbreviation)
        => new(
            abbreviation.Id,
            abbreviation.ResourceType,
            abbreviation.Abbreviation);

    private static ProjectRepositoryResult MapRepository(ProjectRepository repository)
        => new(
            repository.Id,
            repository.Alias.Value,
            repository.ProviderType?.Value.ToString(),
            repository.RepositoryUrl,
            repository.Owner,
            repository.RepositoryName,
            repository.DefaultBranch,
            repository.IsConfigured,
            repository.ContentKinds.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries));

    private static TagResult MapTag(Tag tag) => new(tag.Name, tag.Value);
}