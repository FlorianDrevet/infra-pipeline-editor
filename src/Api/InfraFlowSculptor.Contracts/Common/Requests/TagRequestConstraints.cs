namespace InfraFlowSculptor.Contracts.Common.Requests;

/// <summary>
/// Defines the Azure tag limits enforced by request contracts.
/// </summary>
public static class TagRequestConstraints
{
    /// <summary>Maximum number of tags allowed on a tagged request.</summary>
    public const int MaxTagCount = 15;

    /// <summary>Maximum number of characters allowed for a tag name.</summary>
    public const int MaxNameLength = 512;

    /// <summary>Maximum number of characters allowed for a tag value.</summary>
    public const int MaxValueLength = 256;
}
