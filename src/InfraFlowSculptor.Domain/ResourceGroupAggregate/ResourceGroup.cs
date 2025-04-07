using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ResourceGroupAggregate;

public sealed class ResourceGroup : AzureResourceConfiguration<ResourceGroupId>
{
    public string? Prefix { get; private set; } = null!;
    public string? Suffix { get; private set; } = null!;

    private ResourceGroup(ResourceGroupId id, string name, Location location, string? prefix, string? suffix) 
        : base(id, name, location)
    {
        Prefix = prefix;
        Suffix = suffix;
    }

    public static ResourceGroup Create(string name, Location location, string? prefix, string? suffix)
    {
        return new ResourceGroup(
            ResourceGroupId.CreateUnique(),
            name,
            location,
            prefix, 
            suffix);
    }

    public ResourceGroup()
    {
    }
}