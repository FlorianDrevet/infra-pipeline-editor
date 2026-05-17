using FluentAssertions;
using InfraFlowSculptor.Contracts.Imports.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.Imports.Requests;

public sealed class PreviewIacImportRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new PreviewIacImportRequest
        {
            SourceFormat = "arm-json",
            SourceContent = "{ \"resources\": [] }",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullSourceFormat_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new PreviewIacImportRequest
        {
            SourceFormat = null!,
            SourceContent = "{ \"resources\": [] }",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(PreviewIacImportRequest.SourceFormat)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullSourceContent_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new PreviewIacImportRequest
        {
            SourceFormat = "arm-json",
            SourceContent = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(PreviewIacImportRequest.SourceContent)).Should().BeTrue();
    }
}
