namespace InfraFlowSculptor.Application.Imports.Common;

/// <summary>
/// Well-known category identifiers for <see cref="ImportPreviewGapResult"/>.
/// </summary>
public static class ImportPreviewGapCategory
{
    /// <summary>A resource type that is not yet supported by InfraFlowSculptor.</summary>
    public const string UnsupportedResource = "unsupported_resource";

    /// <summary>A resource property that cannot be mapped and is auto-managed by InfraFlowSculptor.</summary>
    public const string UnmappedProperty = "unmapped_property";
}
