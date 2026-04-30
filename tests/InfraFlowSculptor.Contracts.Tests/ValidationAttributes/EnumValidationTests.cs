using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.Tests.ValidationAttributes;

public sealed class EnumValidationTests
{
    private enum SampleEnum
    {
        First = 1,
        Second = 2,
    }

    private sealed class StringHolder
    {
        [EnumValidation(typeof(SampleEnum))]
        public string? Value { get; init; }
    }

    private sealed class EnumHolder
    {
        [EnumValidation(typeof(SampleEnum))]
        public SampleEnum Value { get; init; }
    }

    [Fact]
    public void Given_NullValue_When_IsValid_Then_ReturnsTrue()
    {
        // Arrange
        var sut = new EnumValidation(typeof(SampleEnum));

        // Act
        var result = sut.IsValid(null);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("First")]
    [InlineData("Second")]
    public void Given_DefinedEnumName_When_Validate_Then_NoError(string value)
    {
        // Arrange
        var sut = new StringHolder { Value = value };

        // Act
        var results = ValidateInstance(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_UndefinedEnumName_When_Validate_Then_ReturnsError()
    {
        // Arrange
        const string undefinedValue = "Third";
        var sut = new StringHolder { Value = undefinedValue };

        // Act
        var results = ValidateInstance(sut);

        // Assert
        results.Should().ContainSingle()
            .Which.MemberNames.Should().Contain(nameof(StringHolder.Value));
    }

    [Fact]
    public void Given_DefinedEnumValue_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new EnumHolder { Value = SampleEnum.First };

        // Act
        var results = ValidateInstance(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_UndefinedEnumIntValue_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new EnumHolder { Value = (SampleEnum)999 };

        // Act
        var results = ValidateInstance(sut);

        // Assert
        results.Should().ContainSingle()
            .Which.MemberNames.Should().Contain(nameof(EnumHolder.Value));
    }

    [Fact]
    public void Given_NonEnumType_When_Construct_Then_ThrowsArgumentException()
    {
        // Arrange
        var nonEnumType = typeof(string);

        // Act
        var act = () => new EnumValidation(nonEnumType);

        // Assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("enumType");
    }

    private static IList<ValidationResult> ValidateInstance(object instance)
    {
        var context = new ValidationContext(instance);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, context, results, validateAllProperties: true);
        return results;
    }
}
