using FluentAssertions;
using InfraFlowSculptor.Mcp.DependencyInjection;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace InfraFlowSculptor.Mcp.Tests.DependencyInjection;

public sealed class McpMappingServiceCollectionExtensionsTests
{
    [Fact]
    public void Given_EmptyServiceCollection_When_AddMcpMappings_Then_RegistersMapper()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMcpMappings();
        using var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<IMapper>().Should().NotBeNull();
    }
}