using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.SqlServers.Requests;

/// <summary>Request body for creating a new SQL Server resource inside a Resource Group.</summary>
public class CreateSqlServerRequest : SqlServerRequestBase
{
    /// <summary>Unique identifier of the Resource Group that will own this SQL Server.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }

    /// <summary>Whether this resource already exists in Azure and is not managed by this project.</summary>

    public bool IsExisting { get; init; } = false;

}
