using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace InfraFlowSculptor.Api.Tests.Integration;

/// <summary>
/// Integration tests that exercise the real HTTP pipeline via <see cref="WebApplicationFactory{TEntryPoint}"/>
/// to verify security headers, health endpoints, and authentication enforcement.
/// </summary>
public sealed class SecurityMiddlewareIntegrationTests : IClassFixture<SecurityMiddlewareIntegrationTests.ApiFactory>
{
    private readonly HttpClient _client;

    public SecurityMiddlewareIntegrationTests(ApiFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task Given_HealthEndpoint_When_UnauthenticatedRequest_Then_Returns401Async()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert — health endpoints are protected by the fallback authorization policy
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Given_LivenessEndpoint_When_UnauthenticatedRequest_Then_Returns401Async()
    {
        // Act
        var response = await _client.GetAsync("/alive");

        // Assert — liveness endpoints are protected by the fallback authorization policy
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Given_AnyEndpoint_When_GetRequest_Then_ContainsSecurityHeadersAsync()
    {
        // Act — even unauthenticated responses must include security headers
        var response = await _client.GetAsync("/health");

        // Assert
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");

        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.GetValues("X-Frame-Options").Should().Contain("DENY");

        response.Headers.Should().ContainKey("Referrer-Policy");
        response.Headers.GetValues("Referrer-Policy").Should().Contain("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task Given_ProtectedEndpoint_When_UnauthenticatedRequest_Then_Returns401Async()
    {
        // Act
        var response = await _client.GetAsync("/api/projects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Custom <see cref="WebApplicationFactory{TEntryPoint}"/> that removes external dependencies
    /// (PostgreSQL, Azure Key Vault, authentication) so tests run without infrastructure.
    /// </summary>
    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Use a non-Development environment to avoid AzureKeyVaultEmulator registration
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    // EF Core / Npgsql — overridden by InMemory provider below, but required by AddPersistence
                    ["ConnectionStrings:infraDb"] = "Host=localhost;Database=test_db;",
                    // KeyVault — SecretClient is lazy-resolved, so a dummy URI suffices
                    ["ConnectionStrings:keyvault"] = "https://localhost:0",
                    // Blob storage — dummy value, BlobServiceClient is resolved lazily
                    ["ConnectionStrings:AzureBlobStorageConnectionString"] = "UseDevelopmentStorage=true",
                    // AzureAd — minimal config to satisfy AddMicrosoftIdentityWebApi
                    ["AzureAd:Instance"] = "https://login.microsoftonline.com/",
                    ["AzureAd:TenantId"] = "00000000-0000-0000-0000-000000000000",
                    ["AzureAd:ClientId"] = "00000000-0000-0000-0000-000000000001",
                });
            });

            builder.ConfigureTestServices(services =>
            {
                // Remove all hosted services that perform EF Core migration at startup
                var hostedServiceDescriptors = services
                    .Where(d => d.ServiceType == typeof(IHostedService))
                    .ToList();

                foreach (var descriptor in hostedServiceDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Remove all EF Core / Npgsql registrations to avoid dual-provider conflict
                var efDescriptors = services
                    .Where(d =>
                        d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true ||
                        d.ServiceType.FullName?.Contains("Npgsql") == true ||
                        d.ServiceType == typeof(DbContextOptions<InfraFlowSculptor.Infrastructure.Persistence.ProjectDbContext>) ||
                        d.ServiceType == typeof(DbContextOptions) ||
                        d.ServiceType == typeof(InfraFlowSculptor.Infrastructure.Persistence.ProjectDbContext))
                    .ToList();

                foreach (var descriptor in efDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Replace with in-memory database
                services.AddDbContext<InfraFlowSculptor.Infrastructure.Persistence.ProjectDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTests");
                });
            });
        }
    }
}
