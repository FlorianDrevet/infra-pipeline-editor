namespace InfraFlowSculptor.Application.CustomDomains.Queries.GetDnsInstructions;

/// <summary>Result containing DNS instructions for a custom domain.</summary>
/// <param name="DomainName">The fully qualified domain name.</param>
/// <param name="DnsValidationStatus">Current DNS validation status.</param>
/// <param name="Steps">Ordered list of DNS configuration steps.</param>
public sealed record DnsInstructionsResult(
    string DomainName,
    string DnsValidationStatus,
    IReadOnlyList<DnsInstructionStep> Steps);
