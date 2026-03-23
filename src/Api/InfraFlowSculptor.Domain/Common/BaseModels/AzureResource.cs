using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.BaseModels;

public class AzureResource : AggregateRoot<AzureResourceId>
{
    public required ResourceGroupId ResourceGroupId { get; set; }
    public ResourceGroup ResourceGroup { get; set; } = null!;

    public required Name Name { get; set; }
    public required Location Location { get; set; }

    /// <summary>
    /// When set, overrides any naming template and uses this value as the resolved resource name.
    /// Set this at resource creation time when the user explicitly provides a full name.
    /// </summary>
    public string? CustomNameOverride { get; set; }
    
    private readonly List<AzureResource> _dependsOn = new List<AzureResource>();
    public IReadOnlyList<AzureResource> DependsOn => _dependsOn.AsReadOnly();
    
    protected virtual IReadOnlyCollection<ParameterUsage> AllowedParameterUsages { get; }

    private readonly List<ResourceParameterUsage> _parameterUsages = new();
    public IReadOnlyCollection<ResourceParameterUsage> ParameterUsages => _parameterUsages;
    
    private readonly List<InputOutputLink> _inputs = new();
    public IReadOnlyCollection<InputOutputLink> Inputs => _inputs;

    private readonly List<InputOutputLink> _outputs = new();
    public IReadOnlyCollection<InputOutputLink> Outputs => _outputs;

    private readonly List<RoleAssignment> _roleAssignments = new();
    public IReadOnlyCollection<RoleAssignment> RoleAssignments => _roleAssignments.AsReadOnly();

    public void AddDependency(AzureResource resource)
    {
        if (resource.Id == Id)
            throw new InvalidOperationException("Une ressource ne peut pas dépendre d'elle-même.");

        if (_dependsOn.Any(r => r.Id == resource.Id))
            return;

        _dependsOn.Add(resource);
    }

    public void AddRoleAssignment(
        AzureResourceId targetResourceId,
        ManagedIdentityType managedIdentityType,
        string roleDefinitionId)
    {
        if (targetResourceId == Id)
            throw new InvalidOperationException("A resource cannot assign a role to itself.");

        if (_roleAssignments.Any(r =>
                r.TargetResourceId == targetResourceId &&
                r.RoleDefinitionId == roleDefinitionId &&
                r.ManagedIdentityType.Value == managedIdentityType.Value))
            return;

        _roleAssignments.Add(RoleAssignment.Create(Id, targetResourceId, managedIdentityType, roleDefinitionId));
    }

    public void RemoveRoleAssignment(RoleAssignmentId roleAssignmentId)
    {
        var assignment = _roleAssignments.FirstOrDefault(r => r.Id == roleAssignmentId);
        if (assignment is not null)
            _roleAssignments.Remove(assignment);
    }

    protected void AddParameterUsage(
        ParameterDefinition parameter,
        ParameterUsage usage)
    {
        if (!AllowedParameterUsages.Contains(usage))
            throw new InvalidOperationException(
                $"{GetType().Name} does not support parameter usage '{usage}'");

        _parameterUsages.Add(
            new ResourceParameterUsage(Id, parameter.Id, usage));
    }

    protected AzureResource()
    {
    }
}