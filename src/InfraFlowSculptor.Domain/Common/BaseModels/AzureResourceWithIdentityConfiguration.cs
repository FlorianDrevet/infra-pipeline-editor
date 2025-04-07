using InfraFlowSculptor.BicepDirector.Enums;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;

namespace InfraFlowSculptor.Domain.Common.BaseModels;

public abstract class AzureResourceWithIdentityConfiguration<T>: AzureResourceInRgConfiguration<T> where T : notnull
{
    public bool IsSystemAssigned { get; protected set; }
    public Guid? UserAssignedIdentity { get; protected set; }
    
    private List<RightAssignation> _rightAssignations = new();
    public IReadOnlyList<RightAssignation> RightAssignations => _rightAssignations.AsReadOnly();
    
    protected AzureResourceWithIdentityConfiguration(
        T id,
        string name,
        Location location,
        DeploymentKind deploymentKind,
        ResourceGroup resourceGroup,
        bool isExisting,
        bool isSystemAssigned,
        Guid userAssignedIdentity,
        List<RightAssignation> rightAssignations) : base(id, name, location, deploymentKind, resourceGroup, isExisting)
    {
        IsSystemAssigned = isSystemAssigned;
        UserAssignedIdentity = userAssignedIdentity;
        _rightAssignations = rightAssignations ?? new List<RightAssignation>();
    }

    public AzureResourceWithIdentityConfiguration()
    {
        
    }
}