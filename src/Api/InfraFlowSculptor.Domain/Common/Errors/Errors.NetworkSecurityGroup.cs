using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors related to the Network Security Group aggregate.</summary>
    public static class NetworkSecurityGroup
    {
        /// <summary>Returned when a Network Security Group with the specified identifier does not exist.</summary>
        public static Error NotFoundError(AzureResourceId id) => Error.NotFound(
            code: "NetworkSecurityGroup.NotFound",
            description: $"A network security group with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );
    }
}
