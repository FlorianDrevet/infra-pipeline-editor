namespace InfraFlowSculptor.Api.Options;

public class ScalarOAuthOptions
{
    public const string SectionName = "ScalarOAuth";
    
    public required string ClientId { get; set; }
    public required string Audience { get; set; }
    public required string[] Scopes { get; set; }
    public required string AuthorizationUrl { get; set; }
    public required string TokenUrl { get; set; }
}