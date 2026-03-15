using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    public static class StorageAccount
    {
        public static Error NotFoundError(AzureResourceId id) => Error.NotFound(
            code: "StorageAccount.NotFound",
            description: $"A storage account with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );

        public static Error BlobContainerNotFoundError(BlobContainerId id) => Error.NotFound(
            code: "StorageAccount.BlobContainerNotFound",
            description: $"A blob container with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );

        public static Error QueueNotFoundError(StorageQueueId id) => Error.NotFound(
            code: "StorageAccount.QueueNotFound",
            description: $"A queue with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );

        public static Error TableNotFoundError(StorageTableId id) => Error.NotFound(
            code: "StorageAccount.TableNotFound",
            description: $"A table with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );
    }
}
