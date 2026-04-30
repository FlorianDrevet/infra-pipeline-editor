using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.Tests.ValidationAttributes;

public sealed class RedisVersionValidationAttributeTests
{
    private sealed class Holder
    {
        [RedisVersionValidation]
        public int? RedisVersion { get; init; }
    }

    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    public void Given_SupportedVersion_When_Validate_Then_NoError(int version)
    {
        // Arrange
        var sut = new Holder { RedisVersion = version };

        // Act
        var results = ValidateInstance(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    public void Given_UnsupportedVersion_When_Validate_Then_ReturnsError(int version)
    {
        // Arrange
        var sut = new Holder { RedisVersion = version };

        // Act
        var results = ValidateInstance(sut);

        // Assert
        results.Should().ContainSingle()
            .Which.MemberNames.Should().Contain(nameof(Holder.RedisVersion));
    }

    [Fact]
    public void Given_NullVersion_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new Holder { RedisVersion = null };

        // Act
        var results = ValidateInstance(sut);

        // Assert
        results.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("RedisVersion");
    }

    private static IList<ValidationResult> ValidateInstance(object instance)
    {
        var context = new ValidationContext(instance);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, context, results, validateAllProperties: true);
        return results;
    }
}
