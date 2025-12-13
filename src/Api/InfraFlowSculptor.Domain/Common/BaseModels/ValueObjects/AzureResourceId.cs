using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

public class AzureResourceId: Id<AzureResourceId>
{
    public AzureResourceId(Guid value) : base(value)
    {
    }
}