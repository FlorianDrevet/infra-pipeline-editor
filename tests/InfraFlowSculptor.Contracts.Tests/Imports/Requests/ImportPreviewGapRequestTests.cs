using FluentAssertions;
using InfraFlowSculptor.Contracts.Imports.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.Imports.Requests;

public sealed class ImportPreviewGapRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new ImportPreviewGapRequest
        {
            Severity = "Warning",
            Category = "UnsupportedProperty",
            Message = "Property X is not supported",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullSeverity_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new ImportPreviewGapRequest
        {
            Severity = null!,
            Category = "UnsupportedProperty",
            Message = "Property X is not supported",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(ImportPreviewGapRequest.Severity)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullCategory_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new ImportPreviewGapRequest
        {
            Severity = "Warning",
            Category = null!,
            Message = "Property X is not supported",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(ImportPreviewGapRequest.Category)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullMessage_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new ImportPreviewGapRequest
        {
            Severity = "Warning",
            Category = "UnsupportedProperty",
            Message = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(ImportPreviewGapRequest.Message)).Should().BeTrue();
    }
}
