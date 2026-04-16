using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;

/// <summary>
/// Represents a blob lifecycle management rule that automatically deletes blobs
/// in specified containers after a given number of days.
/// </summary>
public class BlobLifecycleRule : Entity<BlobLifecycleRuleId>
{
    /// <summary>Gets the parent Storage Account identifier.</summary>
    public AzureResourceId StorageAccountId { get; private set; } = null!;

    /// <summary>Gets the display name of this lifecycle rule.</summary>
    public string RuleName { get; private set; } = string.Empty;

    /// <summary>Gets the list of container name prefixes this rule applies to.</summary>
    public List<string> ContainerNames { get; private set; } = [];

    /// <summary>Gets the number of days after creation before blobs are deleted.</summary>
    public int TimeToLiveInDays { get; private set; }

    private BlobLifecycleRule(BlobLifecycleRuleId id) : base(id)
    {
    }

    private BlobLifecycleRule()
    {
    }

    /// <summary>Updates all properties of this lifecycle rule.</summary>
    public void Update(
        string ruleName,
        IReadOnlyCollection<string> containerNames,
        int timeToLiveInDays)
    {
        RuleName = ruleName;
        ContainerNames = [.. containerNames];
        TimeToLiveInDays = timeToLiveInDays;
    }

    /// <summary>Creates a new blob lifecycle rule.</summary>
    public static BlobLifecycleRule Create(
        AzureResourceId storageAccountId,
        string ruleName,
        IReadOnlyCollection<string> containerNames,
        int timeToLiveInDays)
    {
        return new BlobLifecycleRule(BlobLifecycleRuleId.CreateUnique())
        {
            StorageAccountId = storageAccountId,
            RuleName = ruleName,
            ContainerNames = [.. containerNames],
            TimeToLiveInDays = timeToLiveInDays
        };
    }
}
