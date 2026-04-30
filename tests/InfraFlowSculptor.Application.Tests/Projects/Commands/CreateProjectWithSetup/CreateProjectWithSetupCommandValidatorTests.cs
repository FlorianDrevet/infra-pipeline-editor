using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.CreateProjectWithSetup;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.CreateProjectWithSetup;

public sealed class CreateProjectWithSetupCommandValidatorTests
{
    private const string LayoutPresetProperty = nameof(CreateProjectWithSetupCommand.LayoutPreset);
    private const string EnvironmentsProperty = nameof(CreateProjectWithSetupCommand.Environments);
    private const string RepositoriesProperty = nameof(CreateProjectWithSetupCommand.Repositories);
    private const string AliasProperty = nameof(RepositorySetupItem.Alias);
    private const string ContentKindsProperty = nameof(RepositorySetupItem.ContentKinds);
    private const string ConnectionDetailsProperty = "ConnectionDetails";

    private readonly CreateProjectWithSetupCommandValidator _sut = new();

    [Fact]
    public void Given_ValidAllInOneCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = CreateAllInOneCommand();

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_InvalidLayoutPreset_When_Validate_Then_FailsOnLayoutPreset()
    {
        // Arrange
        var command = CreateAllInOneCommand() with
        {
            LayoutPreset = "Unsupported",
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == LayoutPresetProperty);
    }

    [Fact]
    public void Given_NoEnvironments_When_Validate_Then_FailsOnEnvironments()
    {
        // Arrange
        var command = CreateAllInOneCommand() with
        {
            Environments = [],
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == EnvironmentsProperty);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void Given_AllInOneWithWrongRepositoryCount_When_Validate_Then_FailsOnRepositories(int repositoryCount)
    {
        // Arrange
        var command = CreateAllInOneCommand() with
        {
            Repositories = CreateRepositories(repositoryCount),
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == RepositoriesProperty);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public void Given_SplitInfraCodeWithWrongRepositoryCount_When_Validate_Then_FailsOnRepositories(int repositoryCount)
    {
        // Arrange
        var command = CreateSplitInfraCodeCommand() with
        {
            Repositories = CreateRepositories(repositoryCount),
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == RepositoriesProperty);
    }

    [Fact]
    public void Given_MultiRepoWithProjectLevelRepositories_When_Validate_Then_FailsOnRepositories()
    {
        // Arrange
        var command = CreateMultiRepoCommand() with
        {
            Repositories = CreateRepositories(1),
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == RepositoriesProperty);
    }

    [Fact]
    public void Given_RepositoryAliasWithUppercaseCharacters_When_Validate_Then_FailsOnAlias()
    {
        // Arrange
        var command = CreateAllInOneCommand() with
        {
            Repositories =
            [
                ValidRepository() with { Alias = "InfraRepo" },
            ],
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains(AliasProperty, StringComparison.Ordinal));
    }

    [Fact]
    public void Given_RepositoryWithoutContentKinds_When_Validate_Then_FailsOnContentKinds()
    {
        // Arrange
        var command = CreateAllInOneCommand() with
        {
            Repositories =
            [
                ValidRepository() with { ContentKinds = [] },
            ],
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains(ContentKindsProperty, StringComparison.Ordinal));
    }

    [Fact]
    public void Given_PartialRepositoryConnectionDetails_When_Validate_Then_FailsOnConnectionDetails()
    {
        // Arrange
        var command = CreateAllInOneCommand() with
        {
            Repositories =
            [
                ValidRepository() with
                {
                    ProviderType = "GitHub",
                    RepositoryUrl = null,
                    DefaultBranch = "main",
                },
            ],
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains(ConnectionDetailsProperty, StringComparison.Ordinal));
    }

    private static CreateProjectWithSetupCommand CreateAllInOneCommand() => new(
        Name: "Retail Platform",
        Description: "Provision the retail platform.",
        LayoutPreset: "AllInOne",
        Environments: [ValidEnvironment()],
        Repositories: [ValidRepository()]);

    private static CreateProjectWithSetupCommand CreateSplitInfraCodeCommand() => new(
        Name: "Retail Platform",
        Description: "Provision the retail platform.",
        LayoutPreset: "SplitInfraCode",
        Environments: [ValidEnvironment()],
        Repositories:
        [
            ValidRepository() with
            {
                Alias = "infrastructure",
                ContentKinds = ["Infrastructure"],
            },
            ValidRepository() with
            {
                Alias = "application-code",
                ContentKinds = ["ApplicationCode"],
            },
        ]);

    private static CreateProjectWithSetupCommand CreateMultiRepoCommand() => new(
        Name: "Retail Platform",
        Description: "Provision the retail platform.",
        LayoutPreset: "MultiRepo",
        Environments: [ValidEnvironment()],
        Repositories: []);

    private static EnvironmentSetupItem ValidEnvironment() => new(
        Name: "Development",
        ShortName: "dev",
        Prefix: string.Empty,
        Suffix: string.Empty,
        Location: "WestEurope",
        SubscriptionId: Guid.Empty,
        Order: 0,
        RequiresApproval: false);

    private static RepositorySetupItem ValidRepository() => new(
        Alias: "infra-repo",
        ContentKinds: ["Infrastructure"],
        ProviderType: "GitHub",
        RepositoryUrl: "https://github.com/floriandrevet/infra-repo",
        DefaultBranch: "main");

    private static IReadOnlyList<RepositorySetupItem> CreateRepositories(int count)
        => Enumerable.Range(0, count)
            .Select(index => ValidRepository() with { Alias = $"infra-repo-{index}" })
            .ToArray();
}