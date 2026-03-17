using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.StorageAccounts.Requests;

public class CreateStorageAccountRequest : StorageAccountRequestBase
{
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }
}
