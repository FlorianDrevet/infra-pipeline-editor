using FluentAssertions;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Configurations.Converters;

public sealed class RepositoryContentKindsConverterTests
{
    private readonly RepositoryContentKindsConverter _sut = new();

    [Fact]
    public void ConvertToProvider_Given_SingleFlag_Should_Return_FlagName()
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();
        var kinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        // Act
        var result = toProvider(kinds);

        // Assert
        result.Should().Be("Infrastructure");
    }

    [Fact]
    public void ConvertToProvider_Given_MultipleFlags_Should_Return_CommaSeparated()
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();
        var kinds = RepositoryContentKinds.Create(
            RepositoryContentKindsEnum.Infrastructure | RepositoryContentKindsEnum.ApplicationCode).Value;

        // Act
        var result = toProvider(kinds);

        // Assert
        result.Should().Be("Infrastructure,ApplicationCode");
    }

    [Fact]
    public void ConvertFromProvider_Given_SingleFlagString_Should_Return_RepositoryContentKinds()
    {
        // Arrange
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();

        // Act
        var result = fromProvider("ApplicationCode");

        // Assert
        result.Should().NotBeNull();
        result.Has(RepositoryContentKindsEnum.ApplicationCode).Should().BeTrue();
        result.Has(RepositoryContentKindsEnum.Infrastructure).Should().BeFalse();
    }

    [Fact]
    public void ConvertFromProvider_Given_MultipleFlagString_Should_Return_RepositoryContentKinds()
    {
        // Arrange
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();

        // Act
        var result = fromProvider("Infrastructure,ApplicationCode");

        // Assert
        result.Should().NotBeNull();
        result.Has(RepositoryContentKindsEnum.Infrastructure).Should().BeTrue();
        result.Has(RepositoryContentKindsEnum.ApplicationCode).Should().BeTrue();
    }

    [Fact]
    public void RoundTrip_Given_SingleFlag_Should_Return_EqualKinds()
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();
        var original = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        // Act
        var dbValue = toProvider(original);
        var roundTrip = fromProvider(dbValue);

        // Assert
        roundTrip.Should().Be(original);
    }

    [Fact]
    public void RoundTrip_Given_MultipleFlags_Should_Return_EqualKinds()
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();
        var original = RepositoryContentKinds.Create(
            RepositoryContentKindsEnum.Infrastructure | RepositoryContentKindsEnum.ApplicationCode).Value;

        // Act
        var dbValue = toProvider(original);
        var roundTrip = fromProvider(dbValue);

        // Assert
        roundTrip.Should().Be(original);
    }
}
