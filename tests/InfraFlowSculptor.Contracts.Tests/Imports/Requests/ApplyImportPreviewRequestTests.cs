using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using InfraFlowSculptor.Contracts.Imports.Requests;

namespace InfraFlowSculptor.Contracts.Tests.Imports.Requests;

public sealed class ApplyImportPreviewRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoValidationErrorIsReturned()
    {
        // Arrange
        var sut = CreateValidRequest();

        // Act
        var results = Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_MissingProjectName_When_Validate_Then_ReturnsValidationError()
    {
        // Arrange
        var sut = CreateValidRequest() with
        {
            ProjectName = string.Empty,
        };

        // Act
        var results = Validate(sut);

        // Assert
        results.Should().Contain(result => result.MemberNames.Contains(nameof(ApplyImportPreviewRequest.ProjectName)));
    }

    [Fact]
    public void Given_InvalidNestedPreviewResource_When_Validate_Then_PrefixesNestedMemberNames()
    {
        // Arrange
        var sut = CreateValidRequest() with
        {
            Preview = new ImportPreviewPayloadRequest
            {
                SourceFormat = "arm-json",
                Resources =
                [
                    new ImportPreviewResourceRequest
                    {
                        SourceType = "Microsoft.KeyVault/vaults",
                        SourceName = string.Empty,
                        MappedResourceType = "KeyVault",
                        MappedName = "myKeyVault",
                        Confidence = "High",
                        ExtractedProperties = new Dictionary<string, object?>(),
                        UnmappedProperties = [],
                    },
                ],
                Dependencies = [],
                Metadata = new Dictionary<string, string>(),
                Gaps = [],
                UnsupportedResources = [],
                Summary = "summary",
            },
        };

        // Act
        var results = Validate(sut);

        // Assert
        var result = results.Should().ContainSingle().Which;
        var memberName = result.MemberNames.Should().ContainSingle().Which;
        memberName.Should().StartWith("Preview.Resources[");
        memberName.Should().EndWith("].SourceName");
    }

    private static ApplyImportPreviewRequest CreateValidRequest()
    {
        return new ApplyImportPreviewRequest
        {
            ProjectName = "ImportedProject",
            LayoutPreset = "AllInOne",
            Preview = new ImportPreviewPayloadRequest
            {
                SourceFormat = "arm-json",
                Resources =
                [
                    new ImportPreviewResourceRequest
                    {
                        SourceType = "Microsoft.KeyVault/vaults",
                        SourceName = "myKeyVault",
                        MappedResourceType = "KeyVault",
                        MappedName = "myKeyVault",
                        Confidence = "High",
                        ExtractedProperties = new Dictionary<string, object?>(),
                        UnmappedProperties = [],
                    },
                ],
                Dependencies = [],
                Metadata = new Dictionary<string, string>(),
                Gaps = [],
                UnsupportedResources = [],
                Summary = "summary",
            },
        };
    }

    private static List<ValidationResult> Validate(ApplyImportPreviewRequest request)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, context, results, validateAllProperties: true);
        return results;
    }
}