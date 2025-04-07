using InfraFlowSculptor.BicepDirector.Enums;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ManagedIdentityConfiguration.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;

namespace InfraFlowSculptor.Domain.ManagedIdentityConfiguration;

public sealed class ManagedIdentity : AzureResourceInRgConfiguration<ManagedIdentityId>
{
    private List<RightAssignation> _rightAssignations = new();
    public IReadOnlyList<RightAssignation> RightAssignations => _rightAssignations.AsReadOnly();
    
    private ManagedIdentity(
        ManagedIdentityId id,
        string name,
        Location location,
        DeploymentKind deploymentKind,
        ResourceGroup resourceGroup,
        bool isExisting,
        List<RightAssignation> rightAssignations)
        : base(id, name, location, deploymentKind, resourceGroup, isExisting)
    {
        _rightAssignations = rightAssignations;
    }

    public static ManagedIdentity Create(
        string name,
        Location location,
        DeploymentKind deploymentKind,
        ResourceGroup resourceGroup,
        bool isExisting,
        List<RightAssignation> rightAssignations)
    {
        return new ManagedIdentity(
            ManagedIdentityId.CreateUnique(),
            name,
            location,
            deploymentKind,
            resourceGroup,
            isExisting,
            rightAssignations);
    }

    public ManagedIdentity()
    {
    }
}