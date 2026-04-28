using System.Text.Json;
using FluentAssertions;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Mcp.Drafts;
using InfraFlowSculptor.Mcp.Tools;
using NSubstitute;

namespace InfraFlowSculptor.Mcp.Tests.Tools;

public sealed class ProjectDraftToolsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly IProjectDraftService _draftService = Substitute.For<IProjectDraftService>();

    [Fact]
    public void DraftProjectFromPrompt_ReturnsValidJson_WithDraftId()
    {
        // Arrange
        var expectedDraft = new ProjectCreationDraft
        {
            DraftId = "draft_abc12345",
            Status = DraftStatus.ReadyToCreate,
            Intent = new DraftProjectIntent { ProjectName = "TestApp", LayoutPreset = LayoutPresetEnum.AllInOne },
        };
        _draftService.CreateDraftFromPrompt("create project TestApp mono repo")
            .Returns(expectedDraft);

        // Act
        var json = ProjectDraftTools.DraftProjectFromPrompt(_draftService, "create project TestApp mono repo");

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("draftId").GetString().Should().Be("draft_abc12345");
        doc.RootElement.GetProperty("status").GetString().Should().Be("ReadyToCreate");
    }

    [Fact]
    public void ValidateProjectDraft_WithOverrides_ReturnsUpdatedDraft()
    {
        // Arrange
        var updatedDraft = new ProjectCreationDraft
        {
            DraftId = "draft_abc12345",
            Status = DraftStatus.ReadyToCreate,
            Intent = new DraftProjectIntent { ProjectName = "RetailApi", LayoutPreset = LayoutPresetEnum.AllInOne },
        };
        _draftService.ValidateAndUpdate("draft_abc12345", Arg.Any<DraftOverrides>())
            .Returns(updatedDraft);

        var overridesJson = JsonSerializer.Serialize(
            new { projectName = "RetailApi", layoutPreset = "AllInOne" },
            JsonOptions);

        // Act
        var json = ProjectDraftTools.ValidateProjectDraft(_draftService, "draft_abc12345", overridesJson);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("status").GetString().Should().Be("ReadyToCreate");
    }

    [Fact]
    public void ValidateProjectDraft_InvalidDraftId_ReturnsError()
    {
        // Arrange
        _draftService.ValidateAndUpdate("draft_invalid", Arg.Any<DraftOverrides>())
            .Returns((ProjectCreationDraft?)null);

        // Act
        var json = ProjectDraftTools.ValidateProjectDraft(_draftService, "draft_invalid");

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().Be("draft_not_found");
    }
}
