using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Domain.Tests.InfrastructureConfigAggregate;

public sealed class InfrastructureConfigTests
{
    private const string DefaultConfigName = "prod";
    private const string KeyVaultResourceType = "KeyVault";
    private const string StorageResourceType = "StorageAccount";
    private const string KeyVaultAbbreviation = "kv";
    private const string StorageAbbreviation = "st";
    private const string DefaultBranch = "main";
    private const string InfraRepoUrl = "https://github.com/acme/infra";
    private const string AppRepoUrl = "https://github.com/acme/app";
    private const string AllInOneRepoUrl = "https://github.com/acme/everything";

    private static InfrastructureConfig CreateValidConfig()
    {
        return InfrastructureConfig.Create(new Name(DefaultConfigName), ProjectId.CreateUnique());
    }

    // ─── Factory ────────────────────────────────────────────────────────────

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesIdNameAndDefaults()
    {
        // Arrange
        var name = new Name(DefaultConfigName);
        var projectId = ProjectId.CreateUnique();

        // Act
        var sut = InfrastructureConfig.Create(name, projectId);

        // Assert
        sut.Id.Should().NotBeNull();
        sut.Id.Value.Should().NotBe(Guid.Empty);
        sut.Name.Should().Be(name);
        sut.ProjectId.Should().Be(projectId);
        sut.DefaultNamingTemplate.Should().BeNull();
        sut.UseProjectNamingConventions.Should().BeTrue();
        sut.AppPipelineMode.Should().Be(AppPipelineMode.Isolated);
        sut.LayoutMode.Should().BeNull();
        sut.ResourceGroups.Should().BeEmpty();
        sut.ResourceNamingTemplates.Should().BeEmpty();
        sut.ResourceAbbreviationOverrides.Should().BeEmpty();
        sut.ParameterDefinitions.Should().BeEmpty();
        sut.CrossConfigReferences.Should().BeEmpty();
        sut.Tags.Should().BeEmpty();
        sut.Repositories.Should().BeEmpty();
    }

    // ─── Rename ─────────────────────────────────────────────────────────────

    [Fact]
    public void Given_NewName_When_Rename_Then_NameIsUpdated()
    {
        // Arrange
        var sut = CreateValidConfig();
        var newName = new Name("staging");

        // Act
        sut.Rename(newName);

        // Assert
        sut.Name.Should().Be(newName);
    }

    // ─── Tags ───────────────────────────────────────────────────────────────

    [Fact]
    public void Given_NewTagSet_When_SetTags_Then_ReplacesExistingTags()
    {
        // Arrange
        var sut = CreateValidConfig();
        sut.SetTags(new[] { new Tag("env", "dev") });
        var replacement = new[] { new Tag("env", "prod"), new Tag("owner", "team-a") };

        // Act
        sut.SetTags(replacement);

        // Assert
        sut.Tags.Should().HaveCount(2);
        sut.Tags.Should().BeEquivalentTo(replacement);
    }

    // ─── Naming Convention ──────────────────────────────────────────────────

    [Fact]
    public void Given_Template_When_SetDefaultNamingTemplate_Then_StoresIt()
    {
        // Arrange
        var sut = CreateValidConfig();
        var template = new NamingTemplate("{name}-{env}");

        // Act
        sut.SetDefaultNamingTemplate(template);

        // Assert
        sut.DefaultNamingTemplate.Should().Be(template);
    }

    [Fact]
    public void Given_ExistingDefaultTemplate_When_SetDefaultNamingTemplateWithNull_Then_ClearsIt()
    {
        // Arrange
        var sut = CreateValidConfig();
        sut.SetDefaultNamingTemplate(new NamingTemplate("{name}"));

        // Act
        sut.SetDefaultNamingTemplate(null);

        // Assert
        sut.DefaultNamingTemplate.Should().BeNull();
    }

    [Fact]
    public void Given_NewResourceType_When_SetResourceNamingTemplate_Then_AddsEntry()
    {
        // Arrange
        var sut = CreateValidConfig();
        var template = new NamingTemplate("{name}-kv");

        // Act
        var entry = sut.SetResourceNamingTemplate(KeyVaultResourceType, template);

        // Assert
        sut.ResourceNamingTemplates.Should().ContainSingle();
        entry.ResourceType.Should().Be(KeyVaultResourceType);
        entry.Template.Should().Be(template);
        entry.InfraConfigId.Should().Be(sut.Id);
    }

    [Fact]
    public void Given_ExistingResourceType_When_SetResourceNamingTemplate_Then_UpdatesInPlace()
    {
        // Arrange
        var sut = CreateValidConfig();
        var first = sut.SetResourceNamingTemplate(KeyVaultResourceType, new NamingTemplate("{name}-kv"));
        var newTemplate = new NamingTemplate("{prefix}-{name}-kv-{env}");

        // Act
        var second = sut.SetResourceNamingTemplate(KeyVaultResourceType, newTemplate);

        // Assert
        sut.ResourceNamingTemplates.Should().ContainSingle();
        second.Should().BeSameAs(first);
        second.Template.Should().Be(newTemplate);
    }

    [Fact]
    public void Given_ExistingResourceType_When_RemoveResourceNamingTemplate_Then_ReturnsTrueAndRemoves()
    {
        // Arrange
        var sut = CreateValidConfig();
        sut.SetResourceNamingTemplate(KeyVaultResourceType, new NamingTemplate("{name}-kv"));

        // Act
        var removed = sut.RemoveResourceNamingTemplate(KeyVaultResourceType);

        // Assert
        removed.Should().BeTrue();
        sut.ResourceNamingTemplates.Should().BeEmpty();
    }

    [Fact]
    public void Given_UnknownResourceType_When_RemoveResourceNamingTemplate_Then_ReturnsFalse()
    {
        // Arrange
        var sut = CreateValidConfig();

        // Act
        var removed = sut.RemoveResourceNamingTemplate(KeyVaultResourceType);

        // Assert
        removed.Should().BeFalse();
    }

    // ─── Abbreviation Overrides ─────────────────────────────────────────────

    [Fact]
    public void Given_NewResourceType_When_SetResourceAbbreviationOverride_Then_AddsEntry()
    {
        // Arrange
        var sut = CreateValidConfig();

        // Act
        var entry = sut.SetResourceAbbreviationOverride(KeyVaultResourceType, KeyVaultAbbreviation);

        // Assert
        sut.ResourceAbbreviationOverrides.Should().ContainSingle();
        entry.ResourceType.Should().Be(KeyVaultResourceType);
        entry.Abbreviation.Should().Be(KeyVaultAbbreviation);
        entry.InfraConfigId.Should().Be(sut.Id);
    }

    [Fact]
    public void Given_ExistingResourceType_When_SetResourceAbbreviationOverride_Then_UpdatesInPlace()
    {
        // Arrange
        var sut = CreateValidConfig();
        var first = sut.SetResourceAbbreviationOverride(KeyVaultResourceType, KeyVaultAbbreviation);
        const string newAbbreviation = "vault";

        // Act
        var second = sut.SetResourceAbbreviationOverride(KeyVaultResourceType, newAbbreviation);

        // Assert
        sut.ResourceAbbreviationOverrides.Should().ContainSingle();
        second.Should().BeSameAs(first);
        second.Abbreviation.Should().Be(newAbbreviation);
    }

    [Fact]
    public void Given_ExistingOverride_When_RemoveResourceAbbreviationOverride_Then_ReturnsTrue()
    {
        // Arrange
        var sut = CreateValidConfig();
        sut.SetResourceAbbreviationOverride(StorageResourceType, StorageAbbreviation);

        // Act
        var removed = sut.RemoveResourceAbbreviationOverride(StorageResourceType);

        // Assert
        removed.Should().BeTrue();
        sut.ResourceAbbreviationOverrides.Should().BeEmpty();
    }

    [Fact]
    public void Given_UnknownResourceType_When_RemoveResourceAbbreviationOverride_Then_ReturnsFalse()
    {
        // Arrange
        var sut = CreateValidConfig();

        // Act
        var removed = sut.RemoveResourceAbbreviationOverride(StorageResourceType);

        // Assert
        removed.Should().BeFalse();
    }

    // ─── Inheritance Toggles ────────────────────────────────────────────────

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Given_BooleanFlag_When_SetUseProjectNamingConventions_Then_StoresValue(bool value)
    {
        // Arrange
        var sut = CreateValidConfig();

        // Act
        sut.SetUseProjectNamingConventions(value);

        // Assert
        sut.UseProjectNamingConventions.Should().Be(value);
    }

    [Theory]
    [InlineData(AppPipelineMode.Isolated)]
    [InlineData(AppPipelineMode.Combined)]
    public void Given_PipelineMode_When_UpdateAppPipelineMode_Then_StoresValue(AppPipelineMode mode)
    {
        // Arrange
        var sut = CreateValidConfig();

        // Act
        sut.UpdateAppPipelineMode(mode);

        // Assert
        sut.AppPipelineMode.Should().Be(mode);
    }

    // ─── Cross-Config References ────────────────────────────────────────────

    [Fact]
    public void Given_DifferentTargetConfig_When_AddCrossConfigReference_Then_ReturnsReference()
    {
        // Arrange
        var sut = CreateValidConfig();
        var targetConfigId = InfrastructureConfigId.CreateUnique();
        var targetResourceId = AzureResourceId.CreateUnique();

        // Act
        var result = sut.AddCrossConfigReference(targetConfigId, targetResourceId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TargetConfigId.Should().Be(targetConfigId);
        result.Value.TargetResourceId.Should().Be(targetResourceId);
        sut.CrossConfigReferences.Should().ContainSingle();
    }

    [Fact]
    public void Given_SameConfigAsTarget_When_AddCrossConfigReference_Then_ReturnsValidationError()
    {
        // Arrange
        var sut = CreateValidConfig();
        var targetResourceId = AzureResourceId.CreateUnique();

        // Act
        var result = sut.AddCrossConfigReference(sut.Id, targetResourceId);

        // Assert
        result.IsError.Should().BeTrue();
        sut.CrossConfigReferences.Should().BeEmpty();
    }

    [Fact]
    public void Given_DuplicateTargetResource_When_AddCrossConfigReference_Then_ReturnsConflictError()
    {
        // Arrange
        var sut = CreateValidConfig();
        var targetConfigId = InfrastructureConfigId.CreateUnique();
        var targetResourceId = AzureResourceId.CreateUnique();
        sut.AddCrossConfigReference(targetConfigId, targetResourceId);

        // Act
        var result = sut.AddCrossConfigReference(targetConfigId, targetResourceId);

        // Assert
        result.IsError.Should().BeTrue();
        sut.CrossConfigReferences.Should().ContainSingle();
    }

    [Fact]
    public void Given_ExistingReference_When_RemoveCrossConfigReference_Then_ReturnsDeleted()
    {
        // Arrange
        var sut = CreateValidConfig();
        var targetConfigId = InfrastructureConfigId.CreateUnique();
        var targetResourceId = AzureResourceId.CreateUnique();
        var added = sut.AddCrossConfigReference(targetConfigId, targetResourceId);

        // Act
        var result = sut.RemoveCrossConfigReference(added.Value.Id);

        // Assert
        result.IsError.Should().BeFalse();
        sut.CrossConfigReferences.Should().BeEmpty();
    }

    [Fact]
    public void Given_UnknownReferenceId_When_RemoveCrossConfigReference_Then_ReturnsNotFoundError()
    {
        // Arrange
        var sut = CreateValidConfig();

        // Act
        var result = sut.RemoveCrossConfigReference(CrossConfigResourceReferenceId.CreateUnique());

        // Assert
        result.IsError.Should().BeTrue();
    }

    // ─── Layout Mode ────────────────────────────────────────────────────────

    [Fact]
    public void Given_NoLayoutMode_When_SetLayoutMode_Then_StoresValue()
    {
        // Arrange
        var sut = CreateValidConfig();
        var mode = new ConfigLayoutMode(ConfigLayoutModeEnum.AllInOne);

        // Act
        sut.SetLayoutMode(mode);

        // Assert
        sut.LayoutMode.Should().Be(mode);
    }

    [Fact]
    public void Given_RepositoriesPresent_When_SetLayoutModeChanges_Then_ClearsRepositories()
    {
        // Arrange
        var sut = CreateValidConfig();
        sut.SetLayoutMode(new ConfigLayoutMode(ConfigLayoutModeEnum.AllInOne));
        var alias = RepositoryAlias.Create("everything").Value;
        var contentKinds = RepositoryContentKinds.Create(
            RepositoryContentKindsEnum.Infrastructure | RepositoryContentKindsEnum.ApplicationCode).Value;
        sut.AddRepository(alias, new GitProviderType(GitProviderTypeEnum.GitHub), AllInOneRepoUrl, DefaultBranch, contentKinds);

        // Act
        sut.SetLayoutMode(new ConfigLayoutMode(ConfigLayoutModeEnum.SplitInfraCode));

        // Assert
        sut.Repositories.Should().BeEmpty();
    }

    [Fact]
    public void Given_SameLayoutMode_When_SetLayoutMode_Then_DoesNotClearRepositories()
    {
        // Arrange
        var sut = CreateValidConfig();
        var mode = new ConfigLayoutMode(ConfigLayoutModeEnum.AllInOne);
        sut.SetLayoutMode(mode);
        var alias = RepositoryAlias.Create("everything").Value;
        var contentKinds = RepositoryContentKinds.Create(
            RepositoryContentKindsEnum.Infrastructure | RepositoryContentKindsEnum.ApplicationCode).Value;
        sut.AddRepository(alias, new GitProviderType(GitProviderTypeEnum.GitHub), AllInOneRepoUrl, DefaultBranch, contentKinds);

        // Act
        sut.SetLayoutMode(new ConfigLayoutMode(ConfigLayoutModeEnum.AllInOne));

        // Assert
        sut.Repositories.Should().ContainSingle();
    }

    // ─── Repositories ───────────────────────────────────────────────────────

    [Fact]
    public void Given_NoLayoutMode_When_AddRepository_Then_ReturnsLayoutModeRequiredError()
    {
        // Arrange
        var sut = CreateValidConfig();
        var alias = RepositoryAlias.Create("infra").Value;
        var contentKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        // Act
        var result = sut.AddRepository(alias, new GitProviderType(GitProviderTypeEnum.GitHub), InfraRepoUrl, DefaultBranch, contentKinds);

        // Assert
        result.IsError.Should().BeTrue();
        sut.Repositories.Should().BeEmpty();
    }

    [Fact]
    public void Given_AllInOneLayoutWithBothKinds_When_AddRepository_Then_Succeeds()
    {
        // Arrange
        var sut = CreateValidConfig();
        sut.SetLayoutMode(new ConfigLayoutMode(ConfigLayoutModeEnum.AllInOne));
        var alias = RepositoryAlias.Create("everything").Value;
        var contentKinds = RepositoryContentKinds.Create(
            RepositoryContentKindsEnum.Infrastructure | RepositoryContentKindsEnum.ApplicationCode).Value;

        // Act
        var result = sut.AddRepository(alias, new GitProviderType(GitProviderTypeEnum.GitHub), AllInOneRepoUrl, DefaultBranch, contentKinds);

        // Assert
        result.IsError.Should().BeFalse();
        sut.Repositories.Should().ContainSingle();
    }

    [Fact]
    public void Given_AllInOneLayoutMissingApplicationCode_When_AddRepository_Then_ReturnsConflict()
    {
        // Arrange
        var sut = CreateValidConfig();
        sut.SetLayoutMode(new ConfigLayoutMode(ConfigLayoutModeEnum.AllInOne));
        var alias = RepositoryAlias.Create("infra-only").Value;
        var contentKinds = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        // Act
        var result = sut.AddRepository(alias, new GitProviderType(GitProviderTypeEnum.GitHub), InfraRepoUrl, DefaultBranch, contentKinds);

        // Assert
        result.IsError.Should().BeTrue();
        sut.Repositories.Should().BeEmpty();
    }

    [Fact]
    public void Given_AllInOneLayoutWithExistingRepo_When_AddSecondRepository_Then_ReturnsConflict()
    {
        // Arrange
        var sut = CreateValidConfig();
        sut.SetLayoutMode(new ConfigLayoutMode(ConfigLayoutModeEnum.AllInOne));
        var bothKinds = RepositoryContentKinds.Create(
            RepositoryContentKindsEnum.Infrastructure | RepositoryContentKindsEnum.ApplicationCode).Value;
        sut.AddRepository(RepositoryAlias.Create("first").Value,
            new GitProviderType(GitProviderTypeEnum.GitHub), AllInOneRepoUrl, DefaultBranch, bothKinds);

        // Act
        var result = sut.AddRepository(RepositoryAlias.Create("second").Value,
            new GitProviderType(GitProviderTypeEnum.GitHub), InfraRepoUrl, DefaultBranch, bothKinds);

        // Assert
        result.IsError.Should().BeTrue();
        sut.Repositories.Should().ContainSingle();
    }

    [Fact]
    public void Given_SplitInfraCodeLayoutWithInfraAndAppRepos_When_AddBoth_Then_Succeeds()
    {
        // Arrange
        var sut = CreateValidConfig();
        sut.SetLayoutMode(new ConfigLayoutMode(ConfigLayoutModeEnum.SplitInfraCode));
        var infraOnly = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;
        var appOnly = RepositoryContentKinds.Create(RepositoryContentKindsEnum.ApplicationCode).Value;

        // Act
        var first = sut.AddRepository(RepositoryAlias.Create("infra").Value,
            new GitProviderType(GitProviderTypeEnum.GitHub), InfraRepoUrl, DefaultBranch, infraOnly);
        var second = sut.AddRepository(RepositoryAlias.Create("app").Value,
            new GitProviderType(GitProviderTypeEnum.GitHub), AppRepoUrl, DefaultBranch, appOnly);

        // Assert
        first.IsError.Should().BeFalse();
        second.IsError.Should().BeFalse();
        sut.Repositories.Should().HaveCount(2);
    }

    [Fact]
    public void Given_SplitInfraCodeLayoutWithBothKindsInOneRepo_When_AddRepository_Then_ReturnsConflict()
    {
        // Arrange
        var sut = CreateValidConfig();
        sut.SetLayoutMode(new ConfigLayoutMode(ConfigLayoutModeEnum.SplitInfraCode));
        var bothKinds = RepositoryContentKinds.Create(
            RepositoryContentKindsEnum.Infrastructure | RepositoryContentKindsEnum.ApplicationCode).Value;

        // Act
        var result = sut.AddRepository(RepositoryAlias.Create("hybrid").Value,
            new GitProviderType(GitProviderTypeEnum.GitHub), AllInOneRepoUrl, DefaultBranch, bothKinds);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public void Given_SplitInfraCodeLayoutAndDuplicateInfraRepo_When_AddSecond_Then_ReturnsConflict()
    {
        // Arrange
        var sut = CreateValidConfig();
        sut.SetLayoutMode(new ConfigLayoutMode(ConfigLayoutModeEnum.SplitInfraCode));
        var infraOnly = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;
        sut.AddRepository(RepositoryAlias.Create("infra-1").Value,
            new GitProviderType(GitProviderTypeEnum.GitHub), InfraRepoUrl, DefaultBranch, infraOnly);

        // Act
        var result = sut.AddRepository(RepositoryAlias.Create("infra-2").Value,
            new GitProviderType(GitProviderTypeEnum.GitHub), AppRepoUrl, DefaultBranch, infraOnly);

        // Assert
        result.IsError.Should().BeTrue();
        sut.Repositories.Should().ContainSingle();
    }

    [Fact]
    public void Given_DuplicateAlias_When_AddRepository_Then_ReturnsConflict()
    {
        // Arrange
        var sut = CreateValidConfig();
        sut.SetLayoutMode(new ConfigLayoutMode(ConfigLayoutModeEnum.SplitInfraCode));
        var infraOnly = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;
        var appOnly = RepositoryContentKinds.Create(RepositoryContentKindsEnum.ApplicationCode).Value;
        var alias = RepositoryAlias.Create("shared").Value;
        sut.AddRepository(alias, new GitProviderType(GitProviderTypeEnum.GitHub), InfraRepoUrl, DefaultBranch, infraOnly);

        // Act
        var result = sut.AddRepository(alias,
            new GitProviderType(GitProviderTypeEnum.GitHub), AppRepoUrl, DefaultBranch, appOnly);

        // Assert
        result.IsError.Should().BeTrue();
        sut.Repositories.Should().ContainSingle();
    }

    [Fact]
    public void Given_ExistingRepository_When_UpdateRepository_Then_AppliesNewValues()
    {
        // Arrange
        var sut = CreateValidConfig();
        sut.SetLayoutMode(new ConfigLayoutMode(ConfigLayoutModeEnum.SplitInfraCode));
        var infraOnly = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;
        var added = sut.AddRepository(RepositoryAlias.Create("infra").Value,
            new GitProviderType(GitProviderTypeEnum.GitHub), InfraRepoUrl, DefaultBranch, infraOnly);
        const string updatedUrl = "https://github.com/acme/infra-renamed";

        // Act
        var result = sut.UpdateRepository(added.Value.Id,
            new GitProviderType(GitProviderTypeEnum.GitHub), updatedUrl, DefaultBranch, infraOnly);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.RepositoryUrl.Should().Be(updatedUrl);
    }

    [Fact]
    public void Given_UnknownRepositoryId_When_UpdateRepository_Then_ReturnsNotFound()
    {
        // Arrange
        var sut = CreateValidConfig();
        sut.SetLayoutMode(new ConfigLayoutMode(ConfigLayoutModeEnum.SplitInfraCode));
        var infraOnly = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;

        // Act
        var result = sut.UpdateRepository(InfraConfigRepositoryId.CreateUnique(),
            new GitProviderType(GitProviderTypeEnum.GitHub), InfraRepoUrl, DefaultBranch, infraOnly);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public void Given_ExistingRepository_When_RemoveRepository_Then_ReturnsDeleted()
    {
        // Arrange
        var sut = CreateValidConfig();
        sut.SetLayoutMode(new ConfigLayoutMode(ConfigLayoutModeEnum.SplitInfraCode));
        var infraOnly = RepositoryContentKinds.Create(RepositoryContentKindsEnum.Infrastructure).Value;
        var added = sut.AddRepository(RepositoryAlias.Create("infra").Value,
            new GitProviderType(GitProviderTypeEnum.GitHub), InfraRepoUrl, DefaultBranch, infraOnly);

        // Act
        var result = sut.RemoveRepository(added.Value.Id);

        // Assert
        result.IsError.Should().BeFalse();
        sut.Repositories.Should().BeEmpty();
    }

    [Fact]
    public void Given_UnknownRepositoryId_When_RemoveRepository_Then_ReturnsNotFound()
    {
        // Arrange
        var sut = CreateValidConfig();

        // Act
        var result = sut.RemoveRepository(InfraConfigRepositoryId.CreateUnique());

        // Assert
        result.IsError.Should().BeTrue();
    }
}
