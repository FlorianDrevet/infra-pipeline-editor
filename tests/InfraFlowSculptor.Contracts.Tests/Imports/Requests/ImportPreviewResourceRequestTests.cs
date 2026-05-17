using FluentAssertions;
using InfraFlowSculptor.Contracts.Imports.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.Imports.Requests;

public sealed class ImportPreviewResourceRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new ImportPreviewResourceRequest
        {
            SourceType = "Microsoft.KeyVault/vaults",
            SourceName = "myKeyVault",
            Confidence = "High",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullSourceType_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new ImportPreviewResourceRequest
        {
            SourceType = null!,
            SourceName = "myKeyVault",
            Confidence = "High",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(ImportPreviewResourceRequest.SourceType)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullSourceName_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new ImportPreviewResourceRequest
        {
            SourceType = "Microsoft.KeyVault/vaults",
            SourceName = null!,
            Confidence = "High",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(ImportPreviewResourceRequest.SourceName)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullConfidence_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new ImportPreviewResourceRequest
        {
            SourceType = "Microsoft.KeyVault/vaults",
            SourceName = "myKeyVault",
            Confidence = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(ImportPreviewResourceRequest.Confidence)).Should().BeTrue();
    }
}
