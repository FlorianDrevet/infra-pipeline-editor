using FluentAssertions;
using InfraFlowSculptor.Api.Configuration;
using InfraFlowSculptor.Api.Options;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace InfraFlowSculptor.Api.Tests.Security;

public sealed class CorsServiceCollectionExtensionsTests
{
    private static readonly string[] ExpectedHeaders =
    [
        "Content-Type",
        "Authorization",
        "Accept",
        "X-Requested-With",
    ];

    private static readonly string[] ExpectedMethods =
    [
        "GET",
        "POST",
        "PUT",
        "DELETE",
        "PATCH",
        "OPTIONS",
    ];

    [Fact]
    public void Given_ConfiguredOrigins_When_AddingApiCors_Then_DefaultPolicyUsesConfiguredOrigins()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{ApiCorsOptions.SectionName}:AllowedOrigins:0"] = "https://portal.contoso.com",
                [$"{ApiCorsOptions.SectionName}:AllowedOrigins:1"] = "https://admin.contoso.com",
            })
            .Build();
        var services = new ServiceCollection();

        // Act
        services.AddApiCors(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        var corsOptions = serviceProvider.GetRequiredService<IOptions<CorsOptions>>().Value;
        var policy = corsOptions.GetPolicy(corsOptions.DefaultPolicyName);

        // Assert
        policy.Should().NotBeNull();
        policy!.Origins.Should().Equal("https://portal.contoso.com", "https://admin.contoso.com");
        policy.Methods.Should().Equal(ExpectedMethods);
        policy.Headers.Should().Equal(ExpectedHeaders);
        policy.SupportsCredentials.Should().BeTrue();
    }

    [Fact]
    public void Given_MissingOrigins_When_AddingApiCors_Then_DefaultPolicyFallsBackToAngularDevServer()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        var services = new ServiceCollection();

        // Act
        services.AddApiCors(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        var corsOptions = serviceProvider.GetRequiredService<IOptions<CorsOptions>>().Value;
        var policy = corsOptions.GetPolicy(corsOptions.DefaultPolicyName);

        // Assert
        policy.Should().NotBeNull();
        policy!.Origins.Should().Equal("http://localhost:4200");
        policy.Methods.Should().Equal(ExpectedMethods);
        policy.Headers.Should().Equal(ExpectedHeaders);
        policy.SupportsCredentials.Should().BeTrue();
    }
}
