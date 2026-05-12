using FluentAssertions;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Commands.CreateProjectWithSetup;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.CreateProjectWithSetup;

public sealed class CreateProjectWithSetupCommandHandlerTests
{
    private const string ProjectName = "Retail Platform";
    private const string ProjectDescription = "Provision the retail platform.";
    private const string ProjectAlias = "platform";
    private const string GitHubProvider = "GitHub";
    private const string MainBranch = "main";
    private const string DevelopmentEnvironment = "Development";
    private const string DevelopmentShortName = "dev";
    private const string WestEuropeLocation = "WestEurope";
    private const string UnsupportedValue = "Unsupported";
    private const string DefaultTemplate = "{name}-{resourceAbbr}{suffix}";
    private const string ResourceGroupResourceType = "ResourceGroup";
    private const string ResourceGroupTemplate = "{resourceAbbr}-{name}{suffix}";
    private const string StorageAccountResourceType = "StorageAccount";
    private const string StorageAccountTemplate = "{name}{resourceAbbr}{envShort}";

    private readonly IProjectRepository _repository;
    private readonly ICurrentUser _currentUser;
    private readonly UserId _userId;
    private readonly CreateProjectWithSetupCommandHandler _sut;

    public CreateProjectWithSetupCommandHandlerTests()
    {
        _repository = Substitute.For<IProjectRepository>();
        _currentUser = Substitute.For<ICurrentUser>();
        _userId = UserId.CreateUnique();

        _currentUser.GetUserIdAsync(Arg.Any<CancellationToken>()).Returns(_userId);
        _repository.AddAsync(Arg.Any<Project>())
            .Returns(callInfo => Task.FromResult((Project)callInfo.Args()[0]));

        _sut = new CreateProjectWithSetupCommandHandler(_repository, _currentUser);
    }

    [Fact]
    public async Task Given_ValidCommand_When_Handle_Then_PersistsProjectWithSetupAsync()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Value.Should().Be(ProjectName);
        result.Value.Description.Should().Be(ProjectDescription);
        result.Value.DefaultNamingTemplate.Should().Be(DefaultTemplate);
        result.Value.LayoutPreset.Should().Be(LayoutPresetEnum.AllInOne.ToString());
        result.Value.ResourceNamingTemplates.Should()
            .Contain(template => template.ResourceType == ResourceGroupResourceType && template.Template == ResourceGroupTemplate);
        result.Value.ResourceNamingTemplates.Should()
            .Contain(template => template.ResourceType == StorageAccountResourceType && template.Template == StorageAccountTemplate);
        result.Value.EnvironmentDefinitions.Should().ContainSingle(environment =>
            environment.Name.Value == DevelopmentEnvironment
            && environment.ShortName == DevelopmentShortName
            && environment.Location == WestEuropeLocation);
        result.Value.Repositories.Should().ContainSingle(repository =>
            repository.Alias == ProjectAlias
            && repository.ProviderType == GitHubProvider
            && repository.DefaultBranch == MainBranch
            && repository.IsConfigured);
        result.Value.Repositories![0].ContentKinds.Should().BeEquivalentTo("Infrastructure", "ApplicationCode");

        await _repository.Received(1).AddAsync(Arg.Is<Project>(project =>
            project.Name.Value == ProjectName
            && project.Description == ProjectDescription
            && project.Members.Count == 1
            && project.EnvironmentDefinitions.Count == 1
            && project.Repositories.Count == 1
            && project.LayoutPreset.Value == LayoutPresetEnum.AllInOne));
    }

    [Fact]
    public async Task Given_InvalidLayoutPreset_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            LayoutPreset = UnsupportedValue,
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.Project.InvalidLayoutPreset(UnsupportedValue).Code);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Project>());
    }

    [Fact]
    public async Task Given_InvalidEnvironmentLocation_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Environments =
            [
                ValidEnvironment() with
                {
                    Location = UnsupportedValue,
                },
            ],
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.Location.InvalidLocation(UnsupportedValue).Code);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Project>());
    }

    [Fact]
    public async Task Given_InvalidRepositoryProvider_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Repositories =
            [
                ValidRepository() with
                {
                    ProviderType = UnsupportedValue,
                },
            ],
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.GitRepository.InvalidProviderType(UnsupportedValue).Code);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Project>());
    }

    [Fact]
    public async Task Given_InvalidRepositoryContentKinds_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Repositories =
            [
                ValidRepository() with
                {
                    ContentKinds = [UnsupportedValue],
                },
            ],
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.ProjectRepository.NoContentKind().Code);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Project>());
    }

    [Fact]
    public async Task Given_CurrentUserNotProvisioned_When_Handle_Then_ReturnsUnauthorizedAsync()
    {
        // Arrange
        _currentUser.GetUserIdAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<UserId>(
                new UnauthorizedAccessException("User was not provisioned.")));
        var command = CreateValidCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Project>());
    }

    private static CreateProjectWithSetupCommand CreateValidCommand() => new(
        Name: ProjectName,
        Description: ProjectDescription,
        LayoutPreset: LayoutPresetEnum.AllInOne.ToString(),
        Environments: [ValidEnvironment()],
        Repositories: [ValidRepository()]);

    private static EnvironmentSetupItem ValidEnvironment() => new(
        Name: DevelopmentEnvironment,
        ShortName: DevelopmentShortName,
        Prefix: string.Empty,
        Suffix: string.Empty,
        Location: WestEuropeLocation,
        SubscriptionId: Guid.Empty,
        Order: 0,
        RequiresApproval: false);

    private static RepositorySetupItem ValidRepository() => new(
        Alias: ProjectAlias,
        ContentKinds: ["Infrastructure", "ApplicationCode"],
        ProviderType: GitHubProvider,
        RepositoryUrl: "https://github.com/floriandrevet/platform",
        DefaultBranch: MainBranch);
}