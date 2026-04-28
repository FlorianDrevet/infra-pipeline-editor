using System.Security.Claims;
using System.Text.Encodings.Web;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InfraFlowSculptor.Infrastructure.Auth;

/// <summary>
/// ASP.NET Core authentication handler that validates personal access tokens (PAT).
/// Reads the <c>Authorization: Bearer ifs_...</c> header, hashes the token, and verifies
/// it against the database. On success, populates <see cref="HttpContext.Items"/> with
/// the provisioned <c>UserId</c> so that <c>ICurrentUser</c> resolves transparently.
/// </summary>
public sealed class PersonalAccessTokenAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IPersonalAccessTokenRepository repository)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    /// <summary>Key used by <c>CurrentUser</c> to resolve the authenticated user.</summary>
    private const string UserIdItemKey = "ProvisionedUserId";

    /// <inheritdoc />
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return AuthenticateResult.NoResult();

        var headerValue = authHeader.ToString();
        if (!headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var token = headerValue["Bearer ".Length..].Trim();
        if (!token.StartsWith(PersonalAccessTokenAuthenticationDefaults.TokenPrefix, StringComparison.Ordinal))
            return AuthenticateResult.NoResult();

        var hash = TokenHash.Compute(token);
        var pat = await repository.GetByTokenHashAsync(hash, Context.RequestAborted);

        if (pat is null)
            return AuthenticateResult.Fail("Invalid personal access token.");

        if (!pat.IsValid(DateTime.UtcNow))
            return AuthenticateResult.Fail("Personal access token is revoked or expired.");

        // Record usage (fire-and-forget; will be persisted by UnitOfWork if within a command scope,
        // or on next request if read-only). We intentionally do not SaveChanges here.
        pat.RecordUsage();

        // Populate HttpContext.Items so ICurrentUser resolves the user transparently.
        Context.Items[UserIdItemKey] = pat.UserId;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, pat.UserId.Value.ToString())
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
