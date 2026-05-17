using FluentAssertions;
using InfraFlowSculptor.Application.RedisCaches.Commands.CreateRedisCache;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.RedisCaches.Commands.CreateRedisCache;

public sealed class CreateRedisCacheCommandValidatorTests
{
    private readonly CreateRedisCacheCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new CreateRedisCacheCommand(
            ResourceGroupId.CreateUnique(),
            new Name("my-redis"),
            new Location(Location.LocationEnum.WestEurope),
            RedisVersion: 6,
            EnableNonSslPort: false,
            MinimumTlsVersion: "1.2",
            DisableAccessKeyAuthentication: false,
            EnableAadAuth: false);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var command = new CreateRedisCacheCommand(
            ResourceGroupId.CreateUnique(),
            null!,
            new Location(Location.LocationEnum.WestEurope),
            RedisVersion: 6,
            EnableNonSslPort: false,
            MinimumTlsVersion: "1.2",
            DisableAccessKeyAuthentication: false,
            EnableAadAuth: false);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateRedisCacheCommand.Name));
    }

    [Fact]
    public void Given_EmptyResourceGroupId_When_Validate_Then_FailsOnResourceGroupId()
    {
        // Arrange
        var command = new CreateRedisCacheCommand(
            null!,
            new Name("my-redis"),
            new Location(Location.LocationEnum.WestEurope),
            RedisVersion: 6,
            EnableNonSslPort: false,
            MinimumTlsVersion: "1.2",
            DisableAccessKeyAuthentication: false,
            EnableAadAuth: false);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateRedisCacheCommand.ResourceGroupId));
    }

    [Fact]
    public void Given_DisableAccessKeyWithoutAadAuth_When_Validate_Then_FailsOnEnableAadAuth()
    {
        // Arrange
        var command = new CreateRedisCacheCommand(
            ResourceGroupId.CreateUnique(),
            new Name("my-redis"),
            new Location(Location.LocationEnum.WestEurope),
            RedisVersion: 6,
            EnableNonSslPort: false,
            MinimumTlsVersion: "1.2",
            DisableAccessKeyAuthentication: true,
            EnableAadAuth: false);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateRedisCacheCommand.EnableAadAuth));
    }

    [Fact]
    public void Given_DisableAccessKeyWithAadAuth_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new CreateRedisCacheCommand(
            ResourceGroupId.CreateUnique(),
            new Name("my-redis"),
            new Location(Location.LocationEnum.WestEurope),
            RedisVersion: 6,
            EnableNonSslPort: false,
            MinimumTlsVersion: "1.2",
            DisableAccessKeyAuthentication: true,
            EnableAadAuth: true);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
