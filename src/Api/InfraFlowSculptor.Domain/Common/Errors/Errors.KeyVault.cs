using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    public static class KeyVault
    {
        public static Error NotFoundError(AzureResourceId id) => Error.NotFound(
            code: "KeyVault.NotFound",
            description: $"A keyVault with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );
    }
}