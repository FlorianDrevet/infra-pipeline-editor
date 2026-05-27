using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

/// <summary>Represents the DNS validation state of a custom domain binding.</summary>
public sealed class DnsValidationStatus(DnsValidationStatus.DnsValidationStatusType value)
    : EnumValueObject<DnsValidationStatus.DnsValidationStatusType>(value)
{
    /// <summary>Available DNS validation states.</summary>
    public enum DnsValidationStatusType
    {
        /// <summary>DNS records have not yet been validated.</summary>
        Pending,

        /// <summary>DNS records have been validated and the domain is ready for binding.</summary>
        Validated,
    }

    /// <summary>DNS records have not yet been validated.</summary>
    public static readonly DnsValidationStatus Pending = new(DnsValidationStatusType.Pending);

    /// <summary>DNS records have been validated and the domain is ready for binding.</summary>
    public static readonly DnsValidationStatus Validated = new(DnsValidationStatusType.Validated);
}
