using System.Security.Claims;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Auth;
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

    /// <inheritdoc />
    public Task<bool> HasPersonalAccessTokenScopeAsync(PatScopeType scope, CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext
                         ?? throw new UnauthorizedAccessException("No active HTTP context.");

        var user = httpContext.User;

        if (!IsPersonalAccessTokenAuthenticated(user))
            return Task.FromResult(true);

        var scopeName = scope.ToString();
        var hasScope = user.Claims.Any(claim =>
            claim.Type == PersonalAccessTokenClaimNames.Scope &&
            string.Equals(claim.Value, scopeName, StringComparison.Ordinal));

        return Task.FromResult(hasScope);
    }

    private static bool IsPersonalAccessTokenAuthenticated(ClaimsPrincipal user)
    {
        return user.Identities.Any(identity =>
            identity.IsAuthenticated &&
            string.Equals(
                identity.AuthenticationType,
                PersonalAccessTokenAuthenticationDefaults.AuthenticationScheme,
                StringComparison.Ordinal));
    }
}
