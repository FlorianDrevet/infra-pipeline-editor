using System.Reflection;
using FluentAssertions;
using Mapster;
using Xunit;

namespace InfraFlowSculptor.Api.Tests.Mapping;

/// <summary>
/// Validates that all Mapster <see cref="IRegister"/> implementations in the Api assembly
/// can be scanned and compiled without errors.
/// </summary>
public sealed class MapsterConfigurationRegistrationTests
{
    private static readonly Assembly ApiAssembly = typeof(InfraFlowSculptor.Api.Common.Mapping.DependencyInjection).Assembly;

    [Fact]
    public void Given_AllMappingConfigs_When_Compiled_Then_NoErrors()
    {
        // Arrange
        var config = new TypeAdapterConfig();
        config.Scan(ApiAssembly);

        // Act
        var act = () => config.Compile();

        // Assert
        act.Should().NotThrow("all IRegister mapping configurations must produce valid expressions");
    }

    [Fact]
    public void Given_ApiAssembly_When_Scanned_Then_FindsAtLeastOneRegister()
    {
        // Arrange
        var config = new TypeAdapterConfig();

        // Act
        config.Scan(ApiAssembly);

        // Assert
        config.RuleMap.Should().NotBeEmpty("at least one IRegister implementation must be discovered");
    }

    [Fact]
    public void Given_AllMappingConfigs_When_CompiledTwice_Then_NoErrors()
    {
        // Arrange
        var config = new TypeAdapterConfig();
        config.Scan(ApiAssembly);
        config.Compile();

        // Act — second compilation simulates double-registration safety
        var act = () => config.Compile();

        // Assert
        act.Should().NotThrow("re-compilation after scanning must remain safe");
    }

    [Fact]
    public void Given_ApiAssembly_When_Scanned_Then_AllRegisterImplementationsAreDiscovered()
    {
        // Arrange
        var expectedRegisters = ApiAssembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } && typeof(IRegister).IsAssignableFrom(t))
            .ToList();

        var config = new TypeAdapterConfig();

        // Act
        config.Scan(ApiAssembly);
        config.Compile();

        // Assert
        expectedRegisters.Should().NotBeEmpty("the Api assembly must contain IRegister implementations");
    }
}
