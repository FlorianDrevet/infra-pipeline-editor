using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Configurations.Converters;

public sealed class IdValueConverterTests
{
    private readonly IdValueConverter<AzureResourceId> _sut = new();

    [Fact]
    public void ConvertToProvider_Given_Id_Should_Return_UnderlyingGuid()
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();
        var guid = Guid.NewGuid();
        var id = new AzureResourceId(guid);

        // Act
        var result = toProvider(id);

        // Assert
        result.Should().Be(guid);
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
        result.Value.Should().Be(guid);
    }

    [Fact]
    public void RoundTrip_Given_Id_Should_Return_EqualId()
    {
        // Arrange
        var toProvider = _sut.ConvertToProviderExpression.Compile();
        var fromProvider = _sut.ConvertFromProviderExpression.Compile();
        var original = AzureResourceId.CreateUnique();

        // Act
        var dbValue = toProvider(original);
        var roundTrip = fromProvider(dbValue);

        // Assert
        roundTrip.Should().Be(original);
    }
}
