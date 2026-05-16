namespace InfraFlowSculptor.Contracts.CustomDomains.Responses;

/// <summary>Response DTO containing DNS instructions for a custom domain.</summary>
/// <param name="DomainName">The fully qualified domain name.</param>
/// <param name="DnsValidationStatus">Current DNS validation status.</param>
/// <param name="Steps">Ordered list of DNS configuration steps.</param>
public record DnsInstructionsResponse(
    string DomainName,
    string DnsValidationStatus,
    IReadOnlyList<DnsInstructionStepResponse> Steps);
