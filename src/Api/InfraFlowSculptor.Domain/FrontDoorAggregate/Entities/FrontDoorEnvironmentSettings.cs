using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.FrontDoorAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.FrontDoorAggregate.Entities;

/// <summary>Per-environment settings for a Front Door profile.</summary>
public sealed class FrontDoorEnvironmentSettings : Entity<FrontDoorEnvironmentSettingsId>
{
    /// <summary>Gets the parent Front Door identifier.</summary>
    public AzureResourceId FrontDoorId { get; private set; } = null!;

    /// <summary>Gets the environment name.</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets the pricing tier for this environment.</summary>
    public FrontDoorSku Sku { get; private set; } = null!;

    private FrontDoorEnvironmentSettings() { }

    /// <summary>Creates new environment settings.</summary>
    internal static FrontDoorEnvironmentSettings Create(AzureResourceId frontDoorId, string environmentName, FrontDoorSku sku)
    {
        return new FrontDoorEnvironmentSettings
        {
            Id = FrontDoorEnvironmentSettingsId.CreateUnique(),
            FrontDoorId = frontDoorId,
            EnvironmentName = environmentName,
            Sku = sku
        };
    }

    /// <summary>Updates the environment settings.</summary>
    public void Update(FrontDoorSku sku)
    {
        Sku = sku;
    }
}
