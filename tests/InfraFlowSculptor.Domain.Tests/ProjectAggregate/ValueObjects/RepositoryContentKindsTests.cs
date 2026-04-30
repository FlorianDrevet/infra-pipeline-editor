using FluentAssertions;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.ProjectAggregate.ValueObjects;

public sealed class RepositoryContentKindsTests
{
    [Fact]
    public void Given_NoneFlag_When_Create_Then_ReturnsValidationError()
    {
        // Act
        var result = RepositoryContentKinds.Create(RepositoryContentKindsEnum.None);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Theory]
    [InlineData(RepositoryContentKindsEnum.Infrastructure)]
    [InlineData(RepositoryContentKindsEnum.ApplicationCode)]
    [InlineData(RepositoryContentKindsEnum.Infrastructure | RepositoryContentKindsEnum.ApplicationCode)]
    public void Given_NonEmptyFlags_When_Create_Then_SucceedsWithMatchingFlags(RepositoryContentKindsEnum flags)
    {
        // Act
        var result = RepositoryContentKinds.Create(flags);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Flags.Should().Be(flags);
        result.Value.Value.Should().Be((int)flags);
    }

    [Fact]
    public void Given_InfrastructureFlag_When_HasInfrastructure_Then_ReturnsTrue()
    {
        // Arrange
        var sut = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        // Act
        var has = sut.Has(RepositoryContentKindsEnum.Infrastructure);

        // Assert
        has.Should().BeTrue();
    }

    [Fact]
    public void Given_InfrastructureOnly_When_HasApplicationCode_Then_ReturnsFalse()
    {
        // Arrange
        var sut = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        // Act
        var has = sut.Has(RepositoryContentKindsEnum.ApplicationCode);

        // Assert
        has.Should().BeFalse();
    }

    [Fact]
    public void Given_AnyFlags_When_HasNone_Then_ReturnsFalse()
    {
        // Arrange
        var sut = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        // Act
        var has = sut.Has(RepositoryContentKindsEnum.None);

        // Assert
        has.Should().BeFalse();
    }

    [Theory]
    [InlineData("Infrastructure", RepositoryContentKindsEnum.Infrastructure)]
    [InlineData("ApplicationCode", RepositoryContentKindsEnum.ApplicationCode)]
    [InlineData("infrastructure, applicationcode", RepositoryContentKindsEnum.Infrastructure | RepositoryContentKindsEnum.ApplicationCode)]
    public void Given_ValidCsv_When_Parse_Then_ReturnsMatchingFlags(string raw, RepositoryContentKindsEnum expected)
    {
        // Act
        var result = RepositoryContentKinds.Parse(raw);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Flags.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Garbage")]
    public void Given_InvalidCsv_When_Parse_Then_ReturnsError(string raw)
    {
        // Act
        var result = RepositoryContentKinds.Parse(raw);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public void Given_BothFlags_When_ToString_Then_ReturnsCommaSeparated()
    {
        // Arrange
        var sut = RepositoryContentKinds
            .Create(RepositoryContentKindsEnum.Infrastructure | RepositoryContentKindsEnum.ApplicationCode)
            .Value;

        // Act
        var text = sut.ToString();

        // Assert
        text.Should().Be("Infrastructure,ApplicationCode");
    }

    [Fact]
    public void Given_TwoInstancesWithSameFlags_When_Compared_Then_AreEqual()
    {
        // Arrange
        var first = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;
        var second = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeTrue();
        first.GetHashCode().Should().Be(second.GetHashCode());
    }
}
