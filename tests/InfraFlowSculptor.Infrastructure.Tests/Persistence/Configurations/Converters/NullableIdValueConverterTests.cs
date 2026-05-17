using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Configurations.Converters;

public sealed class NullableIdValueConverterTests
{
    private readonly NullableIdValueConverter<AzureResourceId> _sut = new();

    [Fact]
    public void ConvertToProvider_Given_NonNullId_Should_Return_Guid()
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();
        var guid = Guid.NewGuid();
        AzureResourceId? id = new AzureResourceId(guid);

        // Act
        var result = toProvider(id);

        // Assert
        result.Should().Be(guid);
    }

    [Fact]
    public void ConvertToProvider_Given_Null_Should_Return_Null()
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();

        // Act
        var result = toProvider(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ConvertFromProvider_Given_Guid_Should_Return_Id()
    {
        // Arrange
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();
        var guid = Guid.NewGuid();

        // Act
        var result = fromProvider(guid);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be(guid);
    }

    [Fact]
    public void ConvertFromProvider_Given_Null_Should_Return_Null()
    {
        // Arrange
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();

        // Act
        var result = fromProvider(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void RoundTrip_Given_NonNullId_Should_Return_EqualId()
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();
        AzureResourceId? original = AzureResourceId.CreateUnique();

        // Act
        var dbValue = toProvider(original);
        var roundTrip = fromProvider(dbValue);

        // Assert
        roundTrip.Should().Be(original);
    }

    [Fact]
    public void RoundTrip_Given_Null_Should_Return_Null()
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();

        // Act
        var dbValue = toProvider(null);
        var roundTrip = fromProvider(dbValue);

        // Assert
        roundTrip.Should().BeNull();
    }
}
