using FluentAssertions;
using InfraFlowSculptor.Contracts.Imports.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.Imports.Requests;

public sealed class ImportPreviewDependencyRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new ImportPreviewDependencyRequest
        {
            FromResourceName = "myKeyVault",
            ToResourceName = "myWebApp",
            DependencyType = "Reference",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullFromResourceName_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new ImportPreviewDependencyRequest
        {
            FromResourceName = null!,
            ToResourceName = "myWebApp",
            DependencyType = "Reference",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(ImportPreviewDependencyRequest.FromResourceName)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullToResourceName_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new ImportPreviewDependencyRequest
        {
            FromResourceName = "myKeyVault",
            ToResourceName = null!,
            DependencyType = "Reference",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(ImportPreviewDependencyRequest.ToResourceName)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullDependencyType_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new ImportPreviewDependencyRequest
        {
            FromResourceName = "myKeyVault",
            ToResourceName = "myWebApp",
            DependencyType = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(ImportPreviewDependencyRequest.DependencyType)).Should().BeTrue();
    }
}
