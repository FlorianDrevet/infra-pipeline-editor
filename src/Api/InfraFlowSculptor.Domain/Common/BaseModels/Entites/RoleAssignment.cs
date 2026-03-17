using BicepGenerator.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.BaseModels.Entites;

public sealed class RoleAssignment : Entity<RoleAssignmentId>
{
    public AzureResourceId SourceResourceId { get; private set; } = null!;
    public AzureResourceId TargetResourceId { get; private set; } = null!;
    public ManagedIdentityType ManagedIdentityType { get; private set; } = null!;
    public string RoleDefinitionId { get; private set; } = null!;

    private RoleAssignment() { }

    internal RoleAssignment(
        AzureResourceId sourceResourceId,
        AzureResourceId targetResourceId,
        ManagedIdentityType managedIdentityType,
        string roleDefinitionId)
        : base(RoleAssignmentId.CreateUnique())
    {
        SourceResourceId = sourceResourceId;
        TargetResourceId = targetResourceId;
        ManagedIdentityType = managedIdentityType;
        RoleDefinitionId = roleDefinitionId;
    }

    internal static RoleAssignment Create(
        AzureResourceId sourceResourceId,
        AzureResourceId targetResourceId,
        ManagedIdentityType managedIdentityType,
        string roleDefinitionId)
        => new(sourceResourceId, targetResourceId, managedIdentityType, roleDefinitionId);
}
