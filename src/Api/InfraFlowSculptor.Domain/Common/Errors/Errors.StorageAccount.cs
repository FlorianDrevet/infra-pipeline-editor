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

        /// <summary>Returned when a blob container with the same name already exists in the storage account.</summary>
        public static Error DuplicateBlobContainerName(string name) => Error.Conflict(
            code: "StorageAccount.DuplicateBlobContainerName",
            description: $"A blob container with the name '{name}' already exists in this storage account."
        );

        /// <summary>Returned when a queue with the same name already exists in the storage account.</summary>
        public static Error DuplicateQueueName(string name) => Error.Conflict(
            code: "StorageAccount.DuplicateQueueName",
            description: $"A queue with the name '{name}' already exists in this storage account."
        );

        /// <summary>Returned when a table with the same name already exists in the storage account.</summary>
        public static Error DuplicateTableName(string name) => Error.Conflict(
            code: "StorageAccount.DuplicateTableName",
            description: $"A table with the name '{name}' already exists in this storage account."
        );
    }
}
