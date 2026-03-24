using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.ApplicationInsights.Requests;

/// <summary>Request body for creating a new Application Insights resource inside a Resource Group.</summary>
public class CreateApplicationInsightsRequest : ApplicationInsightsRequestBase
{
    /// <summary>Unique identifier of the Resource Group that will own this Application Insights.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }
}
