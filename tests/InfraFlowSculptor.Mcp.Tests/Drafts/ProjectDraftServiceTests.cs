using FluentAssertions;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Mcp.Drafts;
using InfraFlowSculptor.Mcp.Drafts.Models;

namespace InfraFlowSculptor.Mcp.Tests.Drafts;

public sealed class ProjectDraftServiceTests
{
    private readonly ProjectDraftService _sut = new();

    // ── CreateDraftFromPrompt ──────────────────────────────────────────

    [Fact]
    public void CreateDraftFromPrompt_CompletePrompt_ReturnsReadyToCreate()
    {
        // Arrange
        const string prompt = "Crée-moi un projet RetailApi en mono repo avec un Key Vault";

        // Act
        var draft = _sut.CreateDraftFromPrompt(prompt);

        // Assert
        draft.Status.Should().Be(DraftStatus.ReadyToCreate);
        draft.DraftId.Should().StartWith("draft_");
        draft.Intent.ProjectName.Should().Be("RetailApi");
        draft.Intent.LayoutPreset.Should().Be(LayoutPresetEnum.AllInOne);
        draft.Intent.Resources.Should().ContainSingle(r => r.ResourceType == "KeyVault");
        draft.MissingFields.Should().BeEmpty();
    }

    [Fact]
    public void CreateDraftFromPrompt_MissingLayout_ReturnsRequiresClarification()
    {
        // Arrange
        const string prompt = "Crée un projet avec un Key Vault";

        // Act
        var draft = _sut.CreateDraftFromPrompt(prompt);

        // Assert
        draft.Status.Should().Be(DraftStatus.RequiresClarification);
        draft.MissingFields.Should().Contain("layoutPreset");
        draft.ClarificationQuestions.Should().Contain(q => q.Field == "layoutPreset");
        draft.ClarificationQuestions.First(q => q.Field == "layoutPreset")
            .Options.Should().HaveCount(3);
    }

    [Fact]
    public void CreateDraftFromPrompt_MissingProjectName_HasMissingFieldProjectName()
    {
        // Arrange
        const string prompt = "Crée un projet en mono repo";

        // Act
        var draft = _sut.CreateDraftFromPrompt(prompt);

        // Assert
        draft.MissingFields.Should().Contain("projectName");
        draft.ClarificationQuestions.Should().Contain(q => q.Field == "projectName");
    }

    [Fact]
    public void CreateDraftFromPrompt_ExtractsResourceTypes_CaseInsensitive()
    {
        // Arrange
        const string prompt = "I want a project MyApp in mono repo with a key vault and storage account";

        // Act
        var draft = _sut.CreateDraftFromPrompt(prompt);

        // Assert
        draft.Intent.Resources.Should().HaveCount(2);
        draft.Intent.Resources.Should().Contain(r => r.ResourceType == "KeyVault");
        draft.Intent.Resources.Should().Contain(r => r.ResourceType == "StorageAccount");
    }

    [Fact]
    public void CreateDraftFromPrompt_DefaultEnvironment_HasWarning()
    {
        // Arrange
        const string prompt = "Crée-moi un projet TestApp en mono repo";
        const string expectedWarning = "No environments specified - defaulting to a single 'Development' environment.";

        // Act
        var draft = _sut.CreateDraftFromPrompt(prompt);

        // Assert
        draft.Intent.Environments.Should().ContainSingle();
        draft.Intent.Environments![0].Name.Should().Be("Development");
        draft.Intent.Environments[0].Location.Should().Be(Location.DefaultAzureRegionKey);
        draft.Warnings.Should().Contain(expectedWarning);
    }

    [Fact]
    public void CreateDraftFromPrompt_PricingIntent_ExtractsCheapest()
    {
        // Arrange
        const string prompt = "Crée un projet Budget en mono repo le moins cher possible";

        // Act
        var draft = _sut.CreateDraftFromPrompt(prompt);

        // Assert
        draft.Intent.PricingIntent.Should().Be("cheapest");
    }

    [Fact]
    public void CreateDraftFromPrompt_QuotedProjectName_ExtractsCorrectly()
    {
        // Arrange
        const string prompt = """Crée un projet "My Cool App" en mono repo""";

        // Act
        var draft = _sut.CreateDraftFromPrompt(prompt);

        // Assert
        draft.Intent.ProjectName.Should().Be("My Cool App");
    }

    [Fact]
    public void CreateDraftFromPrompt_AllInOneLayout_CreatesOneRepository()
    {
        // Arrange
        const string prompt = "projet TestApp mono repo";

        // Act
        var draft = _sut.CreateDraftFromPrompt(prompt);

        // Assert
        draft.Intent.Repositories.Should().ContainSingle();
        draft.Intent.Repositories![0].Alias.Should().Be("main");
        draft.Intent.Repositories[0].ContentKinds.Should().BeEquivalentTo(["Infrastructure", "ApplicationCode"]);
    }

    [Fact]
    public void CreateDraftFromPrompt_SplitLayout_CreatesTwoRepositories()
    {
        // Arrange
        const string prompt = "projet TestApp split infra code";

        // Act
        var draft = _sut.CreateDraftFromPrompt(prompt);

        // Assert
        draft.Intent.Repositories.Should().HaveCount(2);
        draft.Intent.Repositories![0].ContentKinds.Should().Contain("Infrastructure");
        draft.Intent.Repositories[1].ContentKinds.Should().Contain("ApplicationCode");
    }

    // ── ValidateAndUpdate ──────────────────────────────────────────────

    [Fact]
    public void ValidateAndUpdate_AppliesOverrides_UpdatesStatus()
    {
        // Arrange — create a draft with missing fields
        var draft = _sut.CreateDraftFromPrompt("Crée un projet avec un Key Vault");
        draft.Status.Should().Be(DraftStatus.RequiresClarification);

        var overrides = new DraftOverrides
        {
            ProjectName = "RetailApi",
            LayoutPreset = LayoutPresetEnum.AllInOne,
        };

        // Act
        var updated = _sut.ValidateAndUpdate(draft.DraftId, overrides);

        // Assert
        updated.Should().NotBeNull();
        updated!.Status.Should().Be(DraftStatus.ReadyToCreate);
        updated.Intent.ProjectName.Should().Be("RetailApi");
        updated.Intent.LayoutPreset.Should().Be(LayoutPresetEnum.AllInOne);
        updated.MissingFields.Should().BeEmpty();
        updated.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateAndUpdate_InvalidProjectName_HasError()
    {
        // Arrange
        var draft = _sut.CreateDraftFromPrompt("Crée un projet en mono repo");
        var overrides = new DraftOverrides { ProjectName = "AB" }; // too short (< 3 chars)

        // Act
        var updated = _sut.ValidateAndUpdate(draft.DraftId, overrides);

        // Assert
        updated.Should().NotBeNull();
        updated!.Status.Should().Be(DraftStatus.RequiresClarification);
        updated.Errors.Should().ContainSingle(e => e.Field == "projectName");
    }

    [Fact]
    public void ValidateAndUpdate_NonExistentDraftId_ReturnsNull()
    {
        // Act
        var result = _sut.ValidateAndUpdate("draft_nonexistent", new DraftOverrides());

        // Assert
        result.Should().BeNull();
    }

    // ── GetDraft ───────────────────────────────────────────────────────

    [Fact]
    public void GetDraft_ExistingDraft_ReturnsDraft()
    {
        // Arrange
        var created = _sut.CreateDraftFromPrompt("projet TestApp mono repo");

        // Act
        var draft = _sut.GetDraft(created.DraftId);

        // Assert
        draft.Should().NotBeNull();
        draft!.DraftId.Should().Be(created.DraftId);
    }

    [Fact]
    public void GetDraft_NonExistentId_ReturnsNull()
    {
        // Act
        var draft = _sut.GetDraft("draft_nonexistent");

        // Assert
        draft.Should().BeNull();
    }
}
