using FluentAssertions;
using InfraFlowSculptor.Contracts.RedisCaches.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.RedisCaches.Requests;

public sealed class CreateRedisCacheRequestTests
{
    private const string ValidLocation = "WestEurope";

    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new CreateRedisCacheRequest
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = "redis-prod",
            Location = ValidLocation,
            RedisVersion = 6,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyResourceGroupId_When_Validate_Then_ReturnsGuidError()
    {
        // Arrange
        var sut = new CreateRedisCacheRequest
        {
            ResourceGroupId = Guid.Empty,
            Name = "redis-prod",
            Location = ValidLocation,
            RedisVersion = 6,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateRedisCacheRequest.ResourceGroupId)).Should().BeTrue();
    }

    [Fact]
    public void Given_UnsupportedRedisVersion_When_Validate_Then_ReturnsVersionError()
    {
        // Arrange
        var sut = new CreateRedisCacheRequest
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = "redis-prod",
            Location = ValidLocation,
            RedisVersion = 5,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().Contain(r => r.ErrorMessage != null && r.ErrorMessage.Contains("RedisVersion"));
    }

    [Fact]
    public void Given_InvalidMinimumTlsVersion_When_Validate_Then_ReturnsEnumError()
    {
        // Arrange
        var sut = new CreateRedisCacheRequest
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = "redis-prod",
            Location = ValidLocation,
            RedisVersion = 6,
            MinimumTlsVersion = "DoesNotExist",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().HaveCount(1);
    }
}
