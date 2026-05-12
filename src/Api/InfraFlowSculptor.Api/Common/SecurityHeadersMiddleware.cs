namespace InfraFlowSculptor.Api.Common;

/// <summary>
/// Middleware that applies API security headers and selects a route-specific Content-Security-Policy.
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    private const string ContentSecurityPolicyHeaderName = "Content-Security-Policy";
    private const string CrossOriginOpenerPolicyHeaderName = "Cross-Origin-Opener-Policy";
    private const string CrossOriginResourcePolicyHeaderName = "Cross-Origin-Resource-Policy";
    private const string PermissionsPolicyHeaderName = "Permissions-Policy";
    private const string ReferrerPolicyHeaderName = "Referrer-Policy";
    private const string XContentTypeOptionsHeaderName = "X-Content-Type-Options";
    private const string XFrameOptionsHeaderName = "X-Frame-Options";

    private const string XFrameOptionsValue = "DENY";
    private const string XContentTypeOptionsValue = "nosniff";
    private const string ReferrerPolicyValue = "strict-origin-when-cross-origin";
    private const string PermissionsPolicyValue = "geolocation=(), microphone=(), camera=()";
    private const string CrossOriginOpenerPolicyValue = "same-origin";
    private const string CrossOriginResourcePolicyValue = "same-site";

    private const string StrictApiContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'";
    private const string ScalarContentSecurityPolicy = "default-src 'none'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self' data:; connect-src 'self'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'";

    private static readonly PathString ScalarPathPrefix = new("/scalar");

    /// <summary>
    /// Applies shared security headers to the current response and selects the Content-Security-Policy for the request path.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task that completes when the next middleware has finished processing.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        ApplySharedSecurityHeaders(context.Response.Headers);
        context.Response.Headers[ContentSecurityPolicyHeaderName] = GetContentSecurityPolicy(context.Request.Path);

        await next(context);
    }

    private static void ApplySharedSecurityHeaders(IHeaderDictionary headers)
    {
        headers[XFrameOptionsHeaderName] = XFrameOptionsValue;
        headers[XContentTypeOptionsHeaderName] = XContentTypeOptionsValue;
        headers[ReferrerPolicyHeaderName] = ReferrerPolicyValue;
        headers[PermissionsPolicyHeaderName] = PermissionsPolicyValue;
        headers[CrossOriginOpenerPolicyHeaderName] = CrossOriginOpenerPolicyValue;
        headers[CrossOriginResourcePolicyHeaderName] = CrossOriginResourcePolicyValue;
    }

    private static string GetContentSecurityPolicy(PathString requestPath)
    {
        return requestPath.StartsWithSegments(ScalarPathPrefix)
            ? ScalarContentSecurityPolicy
            : StrictApiContentSecurityPolicy;
    }
}