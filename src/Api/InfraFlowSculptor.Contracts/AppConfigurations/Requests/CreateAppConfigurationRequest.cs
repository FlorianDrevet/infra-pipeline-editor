using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.AppConfigurations.Requests;

/// <summary>Request body for creating a new App Configuration resource inside a Resource Group.</summary>
public class CreateAppConfigurationRequest : AppConfigurationRequestBase
{
    /// <summary>Unique identifier of the Resource Group that will own this App Configuration.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }
}
