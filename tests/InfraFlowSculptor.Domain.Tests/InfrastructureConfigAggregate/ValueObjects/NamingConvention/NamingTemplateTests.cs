using FluentAssertions;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.InfrastructureConfigAggregate.ValueObjects.NamingConvention;

public sealed class NamingTemplateTests
{
    [Theory]
    [InlineData("{name}-{resourceAbbr}{suffix}")]
    [InlineData("{prefix}-{name}-{resourceAbbr}-{env}")]
    [InlineData("{name}")]
    public void Given_TemplateString_When_Constructed_Then_ExposesValue(string template)
    {
        // Act
        var sut = new NamingTemplate(template);

        // Assert
        sut.Value.Should().Be(template);
    }

    [Fact]
    public void Given_TwoInstancesWithSameTemplate_When_Compared_Then_AreEqual()
    {
        // Arrange
        const string template = "{prefix}-{name}";
        var first = new NamingTemplate(template);
        var second = new NamingTemplate(template);

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeTrue();
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void Given_TwoInstancesWithDifferentTemplates_When_Compared_Then_AreNotEqual()
    {
        // Arrange
        var first = new NamingTemplate("{name}");
        var second = new NamingTemplate("{prefix}-{name}");

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeFalse();
    }
}
