using Microsoft.AspNetCore.RateLimiting;

namespace BicepGenerator.Api.Common.RateLimiting;

public static class DependencyInjection
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("Login", opt =>
            {
                opt.Window = TimeSpan.FromSeconds(10);
                opt.PermitLimit = 3;
            });
        });
        return services;
    }
}