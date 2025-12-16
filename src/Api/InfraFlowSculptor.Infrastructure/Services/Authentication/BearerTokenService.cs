using InfraFlowSculptor.Infrastructure.Services.Authentication;
using Microsoft.Extensions.Caching.Memory;

namespace InfraFlowSculptor.Infrastructure.Services;

/// <summary>
///  Interface for Bearer Token Service
/// </summary>
public interface IBearerTokenService
{
    Task<string> GetBearerTokenAsync(CancellationToken cancellationToken);
}

public sealed class BearerTokenService : IBearerTokenService, IDisposable
{
    private readonly IAzureAdService _azureAdService;
    private readonly IMemoryCache _cache;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public BearerTokenService(IAzureAdService azureAdService, IMemoryCache cache)
    {
        _azureAdService = azureAdService;
        _cache = cache;
    }

    public async Task<string> GetBearerTokenAsync(CancellationToken cancellationToken)
    {
        string cacheKey = "BearerToken";

        if (_cache.TryGetValue(cacheKey, out string? accessToken))
        {
            return accessToken ?? "";
        }

        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            if (_cache.TryGetValue(cacheKey, out string? accessTokenWhenSemaphore))
            {
                return accessTokenWhenSemaphore ?? "";
            }

            var authenticationResult = await _azureAdService.GetAccessTokenAsync(cancellationToken);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(authenticationResult.ExpiresOn);
            _cache.Set(cacheKey, authenticationResult.AccessToken, cacheEntryOptions);

            return authenticationResult.AccessToken;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        _semaphore.Dispose();
        _cache.Dispose();
    }
}