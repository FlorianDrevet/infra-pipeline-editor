using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Common;

public sealed class NamingTemplateValidatorTests
{
    [Fact]
    public void Given_AllValidPlaceholders_When_GetUnknownPlaceholders_Then_ReturnsEmpty()
    {
        var result = NamingTemplateValidator.GetUnknownPlaceholders("{name}-{env}-{resourceAbbr}");

        result.Should().BeEmpty();
    }

    [Fact]
    public void Given_UnknownPlaceholder_When_GetUnknownPlaceholders_Then_ReturnsIt()
    {
        var result = NamingTemplateValidator.GetUnknownPlaceholders("{name}-{foo}");

        result.Should().ContainSingle().Which.Should().Be("foo");
    }

    [Fact]
    public void Given_ValidStaticChars_When_HasValidStaticChars_Then_ReturnsTrue()
    {
        var result = NamingTemplateValidator.HasValidStaticChars("{name}-test_1");

        result.Should().BeTrue();
    }

    [Fact]
    public void Given_InvalidStaticChars_When_HasValidStaticChars_Then_ReturnsFalse()
    {
        var result = NamingTemplateValidator.HasValidStaticChars("{name}@test");

        result.Should().BeFalse();
    }

    [Fact]
    public void Given_EmptyTemplate_When_HasValidStaticChars_Then_ReturnsTrue()
    {
        var result = NamingTemplateValidator.HasValidStaticChars("");

        result.Should().BeTrue();
    }

    [Fact]
    public void Given_KnownResourceType_When_HasValidStaticCharsForResourceType_Then_DelegatesToConstraint()
    {
        var result = NamingTemplateValidator.HasValidStaticCharsForResourceType("{name}-test", "StorageAccount");

        // StorageAccount constraint only allows lowercase alphanumeric — hyphen is invalid.
        result.Should().BeFalse();
    }

    [Fact]
    public void Given_UnknownResourceType_When_HasValidStaticCharsForResourceType_Then_FallsBackToGeneric()
    {
        var result = NamingTemplateValidator.HasValidStaticCharsForResourceType("{name}-test", "UnknownType");

        // Generic rule allows hyphens and underscores, so this should pass.
        result.Should().BeTrue();
    }
}
