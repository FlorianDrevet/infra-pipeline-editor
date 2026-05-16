namespace InfraFlowSculptor.Contracts.CustomDomains.Responses;

/// <summary>Response DTO for a single DNS configuration step.</summary>
/// <param name="Order">Step sequence number.</param>
/// <param name="Title">Short title for the step.</param>
/// <param name="Description">Detailed description of what the user must do.</param>
/// <param name="RecordType">DNS record type (e.g. "CNAME", "TXT"), or null if not applicable.</param>
/// <param name="RecordName">DNS record name to create, or null if not applicable.</param>
/// <param name="RecordValue">DNS record value to set, or null if not applicable.</param>
public record DnsInstructionStepResponse(
    int Order,
    string Title,
    string Description,
    string? RecordType,
    string? RecordName,
    string? RecordValue);
