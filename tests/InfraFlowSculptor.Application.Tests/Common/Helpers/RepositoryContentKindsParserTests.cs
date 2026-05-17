using FluentAssertions;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Common.Helpers;

public sealed class RepositoryContentKindsParserTests
{
    [Fact]
    public void Given_ValidContentKinds_When_Parse_Then_ReturnsCombinedFlags()
    {
        // Arrange
        IReadOnlyList<string> rawKinds = ["Infrastructure", "applicationcode"];

        // Act
        var result = RepositoryContentKindsParser.Parse(rawKinds);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Has(RepositoryContentKindsEnum.Infrastructure).Should().BeTrue();
        result.Value.Has(RepositoryContentKindsEnum.ApplicationCode).Should().BeTrue();
    }

    [Fact]
    public void Given_NoneContentKind_When_Parse_Then_ReturnsValidationError()
    {
        // Arrange
        IReadOnlyList<string> rawKinds = [nameof(RepositoryContentKindsEnum.None)];

        // Act
        var result = RepositoryContentKindsParser.Parse(rawKinds);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.ProjectRepository.NoContentKind().Code);
    }

    [Fact]
    public void Given_UnsupportedContentKind_When_Parse_Then_ReturnsValidationError()
    {
        // Arrange
        IReadOnlyList<string> rawKinds = ["Unsupported"];

        // Act
        var result = RepositoryContentKindsParser.Parse(rawKinds);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.ProjectRepository.NoContentKind().Code);
    }
}
