namespace InfraFlowSculptor.Api.Options;

/// <summary>
/// Represents the API CORS configuration.
/// </summary>
public sealed class ApiCorsOptions
{
    /// <summary>Gets the configuration section name.</summary>
    public const string SectionName = "Cors";

    /// <summary>Gets or sets the allowed browser origins for the API.</summary>
    public string[] AllowedOrigins { get; set; } = [];
}
