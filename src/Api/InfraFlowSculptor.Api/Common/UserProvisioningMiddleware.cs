using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Microsoft.Identity.Web;

namespace InfraFlowSculptor.Api.Common;

/// <summary>
/// Middleware that ensures the authenticated user exists in the database.
/// Runs after authentication/authorization but before endpoint execution.
/// </summary>
public sealed class UserProvisioningMiddleware(RequestDelegate next)
{
    /// <summary>Key used to store the provisioned <see cref="UserId"/> in <see cref="HttpContext.Items"/>.</summary>
    public const string UserIdItemKey = "ProvisionedUserId";

    /// <summary>
    /// Processes the HTTP request, provisioning the user if authenticated and not yet persisted.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="userProvisioningService">The application service responsible for provisioning users.</param>
    public async Task InvokeAsync(HttpContext context, IUserProvisioningService userProvisioningService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var entraIdClaim = context.User.FindFirst(ClaimConstants.ObjectId)?.Value;
            if (entraIdClaim is not null && Guid.TryParse(entraIdClaim, out var entraGuid))
            {
                var entraId = new EntraId(entraGuid);
                var nameClaim = context.User.FindFirst(ClaimConstants.Name)?.Value ?? string.Empty;
                var (firstName, lastName) = ParseName(nameClaim);
                var userId = await userProvisioningService.EnsureProvisionedAsync(
                    entraId,
                    new Name(firstName, lastName),
                    context.RequestAborted);

                context.Items[UserIdItemKey] = userId;
            }
        }

        await next(context);
    }

    private static (string FirstName, string LastName) ParseName(string nameClaim)
    {
        if (string.IsNullOrWhiteSpace(nameClaim))
            return (string.Empty, string.Empty);

        var spaceIndex = nameClaim.IndexOf(' ');
        return spaceIndex < 0
            ? (string.Empty, nameClaim)
            : (nameClaim[..spaceIndex], nameClaim[(spaceIndex + 1)..]);
    }
}
