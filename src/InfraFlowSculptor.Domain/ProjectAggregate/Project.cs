using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using Environment = InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects.Environment;

namespace InfraFlowSculptor.Domain.ProjectAggregate;

public sealed class Project : AggregateRoot<ProjectId>
{
    public string Name { get; private set; } = null!;
    public ResourceGroup PrincipalResourceGroup { get; private set; } = null!;

    private Project(ProjectId id)
        : base(id)
    {

    }

    public static Project Create(string name, ResourceGroup principalResourceGroup)
    {
        return new Project(ProjectId.CreateUnique())
        {
            Name = name,
            PrincipalResourceGroup = principalResourceGroup
        };
    }

    public Project()
    {
    }
}