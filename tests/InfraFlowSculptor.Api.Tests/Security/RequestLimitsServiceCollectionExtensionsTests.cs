using FluentAssertions;
using InfraFlowSculptor.Api.Configuration;
using InfraFlowSculptor.Api.Options;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace InfraFlowSculptor.Api.Tests.Security;

public sealed class RequestLimitsServiceCollectionExtensionsTests
{
    [Fact]
    public void Given_ConfiguredMaxRequestBodySize_When_AddingApiRequestLimits_Then_KestrelUsesConfiguredValue()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{ApiRequestLimitsOptions.SectionName}:MaxRequestBodySizeBytes"] = "1048576",
            })
            .Build();
        var services = new ServiceCollection();

        // Act
        services.AddApiRequestLimits(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        var kestrelOptions = serviceProvider.GetRequiredService<IOptions<KestrelServerOptions>>().Value;

        // Assert
        kestrelOptions.Limits.MaxRequestBodySize.Should().Be(1_048_576);
    }

    [Fact]
    public void Given_MissingMaxRequestBodySize_When_AddingApiRequestLimits_Then_KestrelFallsBackToDefaultValue()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        var services = new ServiceCollection();

        // Act
        services.AddApiRequestLimits(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        var kestrelOptions = serviceProvider.GetRequiredService<IOptions<KestrelServerOptions>>().Value;

        // Assert
        kestrelOptions.Limits.MaxRequestBodySize.Should().Be(ApiRequestLimitsOptions.DefaultMaxRequestBodySizeBytes);
    }
}
