using InfraFlowSculptor.Domain.ResourceGroupAggregate;

namespace InfraFlowSculptor.Domain.Common.BaseModels;

public class AzureResource
{
    public bool Existing { get; protected set; }
    public ResourceGroup ResourceGroup { get; protected set; } = null!;
    public string? Location { get; protected set; } = null!;
}