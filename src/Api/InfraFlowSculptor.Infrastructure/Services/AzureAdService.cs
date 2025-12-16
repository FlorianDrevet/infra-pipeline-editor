using System.Diagnostics.CodeAnalysis;
using InfraFlowSculptor.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace InfraFlowSculptor.Infrastructure.Services;

public interface IAzureAdService
{
    Task<AuthenticationResult> GetAccessTokenAsync(CancellationToken cancellationToken);
}

public sealed class AzureAdService : IAzureAdService
{
    private readonly IConfidentialClientApplication _app;
    private readonly string[] _scopes; // Scopes nécessaires pour l'acquisition de token

    public AzureAdService(IOptions<AzureAdOptions> options)
    {
        var azureAdOptions = options.Value;
        _app = ConfidentialClientApplicationBuilder.Create(azureAdOptions.ClientId)
            .WithClientSecret(azureAdOptions.ClientSecret)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{azureAdOptions.TenantId}/v2.0"))
            .Build();
        
        _scopes = azureAdOptions.Scopes.ToArray();
    }

    public async Task<AuthenticationResult> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _app.AcquireTokenForClient(_scopes).ExecuteAsync(cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception("Erreur lors de l'acquisition du token : " + ex.Message, ex);
        }
    }
}