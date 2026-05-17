namespace InfraFlowSculptor.Api.Configuration;

/// <summary>
/// Registers the API authorization policies.
/// </summary>
public static class AuthorizationServiceCollectionExtensions
{
    /// <summary>
    /// Adds the authorization policies used by the API.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddApiAuthorization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicyNames.IsAdmin, policy =>
                policy.RequireRole(AuthorizationPolicyNames.AdminRole));

        return services;
    }
}
