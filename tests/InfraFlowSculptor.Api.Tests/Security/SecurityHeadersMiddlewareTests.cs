using FluentAssertions;
using InfraFlowSculptor.Api.Common;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace InfraFlowSculptor.Api.Tests.Security;

public sealed class SecurityHeadersMiddlewareTests
{
    private const string ContentSecurityPolicyHeaderName = "Content-Security-Policy";
    private const string CrossOriginOpenerPolicyHeaderName = "Cross-Origin-Opener-Policy";
    private const string CrossOriginResourcePolicyHeaderName = "Cross-Origin-Resource-Policy";
    private const string PermissionsPolicyHeaderName = "Permissions-Policy";
    private const string ReferrerPolicyHeaderName = "Referrer-Policy";
    private const string XContentTypeOptionsHeaderName = "X-Content-Type-Options";
    private const string XFrameOptionsHeaderName = "X-Frame-Options";

    private const string StrictApiContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'";
    private const string ScalarContentSecurityPolicy = "default-src 'none'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self' data:; connect-src 'self'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'";

    [Fact]
    public async Task Given_ApiRequest_When_InvokeAsync_Then_AppliesStrictApiContentSecurityPolicyAsync()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/projects";
        var sut = CreateSut();

        // Act
        await sut.InvokeAsync(context);

        // Assert
        context.Response.Headers[ContentSecurityPolicyHeaderName].ToString().Should().Be(StrictApiContentSecurityPolicy);
    }

    [Fact]
    public async Task Given_ScalarRequest_When_InvokeAsync_Then_AppliesScalarCompatibleContentSecurityPolicyAsync()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/scalar/v1";
        var sut = CreateSut();

        // Act
        await sut.InvokeAsync(context);

        // Assert
        context.Response.Headers[ContentSecurityPolicyHeaderName].ToString().Should().Be(ScalarContentSecurityPolicy);
    }

    [Fact]
    public async Task Given_OpenApiRequest_When_InvokeAsync_Then_KeepsStrictApiContentSecurityPolicyAsync()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/openapi/v1.json";
        var sut = CreateSut();

        // Act
        await sut.InvokeAsync(context);

        // Assert
        context.Response.Headers[ContentSecurityPolicyHeaderName].ToString().Should().Be(StrictApiContentSecurityPolicy);
    }

    [Fact]
    public async Task Given_AnyRequest_When_InvokeAsync_Then_AppliesSharedSecurityHeadersAsync()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/openapi/v1.json";
        var sut = CreateSut();

        // Act
        await sut.InvokeAsync(context);

        // Assert
        context.Response.Headers[XFrameOptionsHeaderName].ToString().Should().Be("DENY");
        context.Response.Headers[XContentTypeOptionsHeaderName].ToString().Should().Be("nosniff");
        context.Response.Headers[ReferrerPolicyHeaderName].ToString().Should().Be("strict-origin-when-cross-origin");
        context.Response.Headers[PermissionsPolicyHeaderName].ToString().Should().Be("geolocation=(), microphone=(), camera=()");
        context.Response.Headers[CrossOriginOpenerPolicyHeaderName].ToString().Should().Be("same-origin");
        context.Response.Headers[CrossOriginResourcePolicyHeaderName].ToString().Should().Be("same-site");
    }

    private static SecurityHeadersMiddleware CreateSut()
    {
        return new SecurityHeadersMiddleware(_ => Task.CompletedTask);
    }
}