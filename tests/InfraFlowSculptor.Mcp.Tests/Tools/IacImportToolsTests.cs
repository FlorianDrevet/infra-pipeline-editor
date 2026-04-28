using System.Text.Json;
using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Imports.Commands.ApplyImportPreview;
using InfraFlowSculptor.Application.Imports.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Mcp.Drafts;
using InfraFlowSculptor.Mcp.Imports;
using InfraFlowSculptor.Mcp.Imports.Models;
using InfraFlowSculptor.Mcp.Tools;
using MediatR;
using NSubstitute;

namespace InfraFlowSculptor.Mcp.Tests.Tools;

public sealed class IacImportToolsTests
{
    private readonly IImportPreviewService _previewService = Substitute.For<IImportPreviewService>();
    private readonly IProjectDraftService _draftService = Substitute.For<IProjectDraftService>();
    private readonly ISender _mediator = Substitute.For<ISender>();

    private const string ValidKeyVaultArm = """
        {
          "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
          "contentVersion": "1.0.0.0",
          "resources": [
            {
              "type": "Microsoft.KeyVault/vaults",
              "apiVersion": "2023-07-01",
              "name": "myKeyVault",
              "location": "[resourceGroup().location]",
              "properties": {
                "sku": { "family": "A", "name": "standard" },
                "tenantId": "[subscription().tenantId]"
              }
            }
          ]
        }
        """;

    [Fact]
    public void PreviewIacImport_UnsupportedFormat_ReturnsUnsupportedMessage()
    {
        // Act
        var json = IacImportTools.PreviewIacImport(_previewService, "terraform", "content");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("summary").GetString().Should().Contain("not supported");
        doc.RootElement.TryGetProperty("previewId", out _).Should().BeFalse("previewId is null and ignored");
    }

    [Fact]
    public void PreviewIacImport_ValidArm_ReturnsPreviewWithMappedResources()
    {
        // Arrange
        var preview = new ImportPreview
        {
            PreviewId = "preview_abc12345",
            Analysis = new ImportPreviewAnalysisResult
            {
                SourceFormat = "arm-json",
                Resources =
                [
                    new ImportedResourceAnalysisResult
                    {
                        SourceType = "Microsoft.KeyVault/vaults",
                        SourceName = "myKeyVault",
                        MappedResourceType = "KeyVault",
                        MappedName = "myKeyVault",
                        Confidence = ImportPreviewMappingConfidence.High,
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
        _previewService.CreatePreviewFromArm(ValidKeyVaultArm).Returns(preview);

        // Act
        var json = IacImportTools.PreviewIacImport(_previewService, "arm-json", ValidKeyVaultArm);

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("previewId").GetString().Should().Be("preview_abc12345");
        doc.RootElement.GetProperty("parsedResourceCount").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("mappedResources").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public void PreviewIacImport_InvalidJson_ReturnsError()
    {
        // Arrange
        _previewService.CreatePreviewFromArm("not json")
            .Returns(_ => throw new JsonException("Invalid JSON"));

        // Act
        var json = IacImportTools.PreviewIacImport(_previewService, "arm-json", "not json");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("invalid_json");
    }

    [Fact]
    public async Task ApplyImportPreview_PreviewNotFound_ReturnsError()
    {
        // Arrange
        _previewService.GetPreview("preview_unknown").Returns((ImportPreview?)null);

        // Act
        var json = await IacImportTools.ApplyImportPreview(
            _previewService, _draftService, _mediator,
            "preview_unknown", "MyProject", "AllInOne");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("preview_not_found");
    }

    [Fact]
    public async Task ApplyImportPreview_ValidPreview_DelegatesToSharedCommand()
    {
        // Arrange
        var preview = new ImportPreview
        {
            PreviewId = "preview_abc12345",
            Analysis = new ImportPreviewAnalysisResult
            {
                SourceFormat = "arm-json",
                Resources =
                [
                    new ImportedResourceAnalysisResult
                    {
                        SourceType = "Microsoft.KeyVault/vaults",
                        SourceName = "myKeyVault",
                        MappedResourceType = "KeyVault",
                        MappedName = "myKeyVault",
                        Confidence = ImportPreviewMappingConfidence.High,
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
        _previewService.GetPreview("preview_abc12345").Returns(preview);

        var projectId = Guid.NewGuid().ToString();
        _mediator.Send(Arg.Any<ApplyImportPreviewCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ApplyImportPreviewResult>>(
                new ApplyImportPreviewResult
                {
                    Status = "applied",
                    ProjectId = projectId,
                    ProjectName = "MyProject",
                    InfrastructureConfigId = Guid.NewGuid().ToString(),
                    ResourceGroupId = Guid.NewGuid().ToString(),
                    CreatedResources =
                    [
                        new ApplyImportPreviewCreatedResourceResult("KeyVault", Guid.NewGuid().ToString(), "myKeyVault"),
                    ],
                    SkippedResources = [],
                    NextSuggestedActions = ["Generate Bicep with 'generate_project_bicep'."],
                }));

        // Act
        var json = await IacImportTools.ApplyImportPreview(
            _previewService, _draftService, _mediator,
            "preview_abc12345", "MyProject", "AllInOne", resourceFilter: "[\"myKeyVault\"]");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("status").GetString().Should().Be("applied");
        doc.RootElement.GetProperty("projectId").GetString().Should().Be(projectId);
        doc.RootElement.GetProperty("previewId").GetString().Should().Be("preview_abc12345");
        doc.RootElement.GetProperty("createdResources").GetArrayLength().Should().Be(1);
        _previewService.Received(1).RemovePreview("preview_abc12345");
        await _mediator.Received(1).Send(
            Arg.Is<ApplyImportPreviewCommand>(command =>
                command.ProjectName == "MyProject"
                && command.LayoutPreset == "AllInOne"
                && command.Preview.SourceFormat == "arm-json"
                && command.ResourceFilter != null
                && command.ResourceFilter.Count == 1
                && command.ResourceFilter[0] == "myKeyVault"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApplyImportPreview_MediatRError_ReturnsError()
    {
        // Arrange
        var preview = new ImportPreview
        {
            PreviewId = "preview_abc12345",
            Analysis = new ImportPreviewAnalysisResult
            {
                SourceFormat = "arm-json",
                Resources = [],
                Dependencies = [],
                Metadata = new Dictionary<string, string>(),
                Gaps = [],
                UnsupportedResources = [],
                Summary = "summary",
            },
        };
        _previewService.GetPreview("preview_abc12345").Returns(preview);

        _mediator.Send(Arg.Any<ApplyImportPreviewCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ApplyImportPreviewResult>>(Error.Failure(description: "Validation failed")));

        // Act
        var json = await IacImportTools.ApplyImportPreview(
            _previewService, _draftService, _mediator,
            "preview_abc12345", "MyProject", "AllInOne");

        // Assert
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("creation_failed");
        _previewService.DidNotReceive().RemovePreview("preview_abc12345");
    }
}
