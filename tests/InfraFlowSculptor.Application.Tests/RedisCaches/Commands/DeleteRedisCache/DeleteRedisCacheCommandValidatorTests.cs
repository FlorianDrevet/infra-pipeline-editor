using FluentAssertions;
using InfraFlowSculptor.Application.RedisCaches.Commands.DeleteRedisCache;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.RedisCaches.Commands.DeleteRedisCache;

public sealed class DeleteRedisCacheCommandValidatorTests
{
    private const string IdProperty = nameof(DeleteRedisCacheCommand.Id);

    private readonly DeleteRedisCacheCommandValidator _sut = new();

    [Fact]
    public void Given_NonNullId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeleteRedisCacheCommand(AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new DeleteRedisCacheCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == IdProperty);
    }
}
