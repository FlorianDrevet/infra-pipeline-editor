using FluentAssertions;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Configurations.Converters;

public sealed class RepositoryAliasConverterTests
{
    private readonly RepositoryAliasConverter _sut = new();

    [Theory]
    [InlineData("infra")]
    [InlineData("app-code")]
    [InlineData("my-repo")]
    public void ConvertToProvider_Given_RepositoryAlias_Should_Return_UnderlyingString(string raw)
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();
        var alias = RepositoryAlias.Create(raw).Value;

        // Act
        var result = toProvider(alias);

        // Assert
        result.Should().Be(raw);
    }

    [Theory]
    [InlineData("infra")]
    [InlineData("app-code")]
    [InlineData("my-repo")]
    public void ConvertFromProvider_Given_String_Should_Return_RepositoryAlias(string raw)
    {
        // Arrange
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();

        // Act
        var result = fromProvider(raw);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(raw);
    }

    [Theory]
    [InlineData("infra")]
    [InlineData("app-code")]
    [InlineData("my-repo")]
    public void RoundTrip_Given_RepositoryAlias_Should_Return_EqualAlias(string raw)
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();
        var original = RepositoryAlias.Create(raw).Value;

        // Act
        var dbValue = toProvider(original);
        var roundTrip = fromProvider(dbValue);

        // Assert
        roundTrip.Should().Be(original);
    }
}
