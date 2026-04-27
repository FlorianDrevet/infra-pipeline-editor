using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace InfraFlowSculptor.Infrastructure.Services;

/// <summary>
/// Resolves the current authenticated user's identifier from the HTTP context.
/// Relies on <c>UserProvisioningMiddleware</c> having already ensured the user exists in the database.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    /// <summary>Key used by the provisioning middleware to store the user identifier.</summary>
    private const string UserIdItemKey = "ProvisionedUserId";

    /// <inheritdoc />
    public Task<UserId> GetUserIdAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext
                         ?? throw new UnauthorizedAccessException("No active HTTP context.");

        if (httpContext.Items[UserIdItemKey] is UserId userId)
            return Task.FromResult(userId);

        throw new UnauthorizedAccessException("User was not provisioned. Ensure authentication middleware is configured.");
    }
}