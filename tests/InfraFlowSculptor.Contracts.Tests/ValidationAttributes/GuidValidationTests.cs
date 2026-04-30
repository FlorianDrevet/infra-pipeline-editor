using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.Tests.ValidationAttributes;

public sealed class GuidValidationTests
{
    private sealed class StringHolder
    {
        [GuidValidation]
        public string? Value { get; init; }
    }

    private sealed class GuidHolder
    {
        [GuidValidation]
        public Guid Value { get; init; }
    }

    private sealed class NullableGuidHolder
    {
        [GuidValidation]
        public Guid? Value { get; init; }
    }

    [Fact]
    public void Given_NullStringValue_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new StringHolder { Value = null };

        // Act
        var results = ValidateInstance(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Given_EmptyOrWhitespaceString_When_Validate_Then_NoError(string value)
    {
        // Arrange
        var sut = new StringHolder { Value = value };

        // Act
        var results = ValidateInstance(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_ValidGuidString_When_Validate_Then_NoError()
    {
        // Arrange
        const string validGuid = "11111111-1111-1111-1111-111111111111";
        var sut = new StringHolder { Value = validGuid };

        // Act
        var results = ValidateInstance(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyGuidString_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new StringHolder { Value = Guid.Empty.ToString() };

        // Act
        var results = ValidateInstance(sut);

        // Assert
        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Contain(nameof(StringHolder.Value));
    }

    [Fact]
    public void Given_MalformedGuidString_When_Validate_Then_ReturnsError()
    {
        // Arrange
        const string malformedGuid = "not-a-guid";
        var sut = new StringHolder { Value = malformedGuid };

        // Act
        var results = ValidateInstance(sut);

        // Assert
        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Contain(nameof(StringHolder.Value));
    }

    [Fact]
    public void Given_NonEmptyGuid_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new GuidHolder { Value = Guid.NewGuid() };

        // Act
        var results = ValidateInstance(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyGuid_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new GuidHolder { Value = Guid.Empty };

        // Act
        var results = ValidateInstance(sut);

        // Assert
        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Contain(nameof(GuidHolder.Value));
    }

    [Fact]
    public void Given_NullNullableGuid_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new NullableGuidHolder { Value = null };

        // Act
        var results = ValidateInstance(sut);

        // Assert
        results.Should().BeEmpty();
    }

    private static IList<ValidationResult> ValidateInstance(object instance)
    {
        var context = new ValidationContext(instance);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, context, results, validateAllProperties: true);
        return results;
    }
}
