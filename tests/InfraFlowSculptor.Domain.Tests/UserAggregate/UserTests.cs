using FluentAssertions;
using InfraFlowSculptor.Domain.UserAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.UserAggregate;

public sealed class UserTests
{
    private const string DefaultFirstName = "Ada";
    private const string DefaultLastName = "Lovelace";

    [Fact]
    public void Given_EntraIdAndName_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var entraId = new EntraId(Guid.NewGuid());
        var name = new Name(DefaultFirstName, DefaultLastName);

        // Act
        var sut = User.Create(entraId, name);

        // Assert
        sut.Id.Should().NotBeNull();
        sut.Id.Value.Should().NotBe(Guid.Empty);
        sut.EntraId.Should().Be(entraId);
        sut.Name.Should().Be(name);
    }

    [Fact]
    public void Given_TwoCreateCalls_When_Compared_Then_ProducesDifferentIds()
    {
        // Arrange
        var entraId = new EntraId(Guid.NewGuid());
        var name = new Name(DefaultFirstName, DefaultLastName);

        // Act
        var first = User.Create(entraId, name);
        var second = User.Create(entraId, name);

        // Assert
        first.Id.Should().NotBe(second.Id);
    }
}
