using FluentAssertions;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Common.Helpers;

public sealed class EnumValueObjectParserTests
{
    [Fact]
    public void Given_NullRawValue_When_ParseOrNull_Then_ReturnsNull()
    {
        // Arrange
        string? raw = null;

        // Act
        var result = EnumValueObjectParser.ParseOrNull<TlsVersion.Version, TlsVersion>(
            raw,
            static parsed => new TlsVersion(parsed),
            Errors.RedisCache.InvalidMinimumTlsVersion);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeNull();
    }

    [Fact]
    public void Given_ValidRawValue_When_ParseOrNull_Then_ReturnsValueObject()
    {
        // Arrange
        const string raw = "Tls12";

        // Act
        var result = EnumValueObjectParser.ParseOrNull<TlsVersion.Version, TlsVersion>(
            raw,
            static parsed => new TlsVersion(parsed),
            Errors.RedisCache.InvalidMinimumTlsVersion);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value!.Value.Should().Be(TlsVersion.Version.Tls12);
    }

    [Fact]
    public void Given_WhitespaceRawValue_When_ParseOrNull_Then_ReturnsValidationError()
    {
        // Arrange
        const string raw = " ";

        // Act
        var result = EnumValueObjectParser.ParseOrNull<RedisCacheSku.Sku, RedisCacheSku>(
            raw,
            static parsed => new RedisCacheSku(parsed),
            Errors.RedisCache.InvalidSku);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.RedisCache.InvalidSku(raw).Code);
    }

    [Fact]
    public void Given_LowercaseRawValue_When_Parse_Then_ReturnsCaseInsensitiveValueObject()
    {
        // Arrange
        const string raw = "contributor";

        // Act
        var result = EnumValueObjectParser.Parse<Role.RoleEnum, Role>(
            raw,
            static parsed => new Role(parsed),
            Errors.Project.InvalidRoleError);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Value.Should().Be(Role.RoleEnum.Contributor);
    }

    [Fact]
    public void Given_InvalidRawValue_When_Parse_Then_ReturnsValidationError()
    {
        // Arrange
        const string raw = "unsupported";

        // Act
        var result = EnumValueObjectParser.Parse<Role.RoleEnum, Role>(
            raw,
            static parsed => new Role(parsed),
            Errors.Project.InvalidRoleError);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.Project.InvalidRoleError(raw).Code);
    }
}