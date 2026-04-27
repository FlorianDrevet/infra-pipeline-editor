namespace InfraFlowSculptor.Contracts.CustomDomains.Responses;

/// <summary>Response DTO for a custom domain binding.</summary>
/// <param name="Id">Custom domain binding identifier.</param>
/// <param name="ResourceId">Parent Azure resource identifier.</param>
/// <param name="EnvironmentName">Deployment environment name.</param>
/// <param name="DomainName">Fully qualified domain name.</param>
/// <param name="BindingType">SSL binding type ("SniEnabled" or "Disabled").</param>
public record CustomDomainResponse(
    string Id,
    string ResourceId,
    string EnvironmentName,
    string DomainName,
    string BindingType);
