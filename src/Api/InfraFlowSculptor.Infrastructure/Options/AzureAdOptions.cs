namespace InfraFlowSculptor.Infrastructure.Options;

public sealed class AzureAdOptions
{
    public static string SectionName = "AzureAd";

    public required string TenantId { get; set; }
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required IEnumerable<string> Scopes { get; set; }
}