using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.SqlDatabases.Requests;

/// <summary>Request body for creating a new SQL Database resource inside a Resource Group.</summary>
public class CreateSqlDatabaseRequest : SqlDatabaseRequestBase
{
    /// <summary>Unique identifier of the Resource Group that will own this SQL Database.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }
}
