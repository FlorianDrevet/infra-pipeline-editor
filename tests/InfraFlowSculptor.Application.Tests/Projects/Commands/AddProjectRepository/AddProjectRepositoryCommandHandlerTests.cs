using FluentAssertions;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectRepository;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.AddProjectRepository;

public sealed class AddProjectRepositoryCommandHandlerTests
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAccessService _accessService;
    private readonly IKeyVaultSecretClient _keyVaultSecretClient;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly IGitProviderService _gitProviderService;
    private readonly Project _project;
    private readonly AddProjectRepositoryCommandHandler _sut;

    public AddProjectRepositoryCommandHandlerTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _keyVaultSecretClient = Substitute.For<IKeyVaultSecretClient>();
        _gitProviderFactory = Substitute.For<IGitProviderFactory>();
        _gitProviderService = Substitute.For<IGitProviderService>();
        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.AllInOne));

        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _gitProviderFactory.Create(Arg.Any<GitProviderType>())
            .Returns(_gitProviderService);
        _gitProviderService.ListBranchesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Branches(new GitBranchResult("main", false)));
        _keyVaultSecretClient.SetSecretAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        _sut = new AddProjectRepositoryCommandHandler(
            _projectRepository,
            _accessService,
            _keyVaultSecretClient,
            _gitProviderFactory);
    }

    [Fact]
    public async Task Given_InvalidProviderType_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = new AddProjectRepositoryCommand(
            _project.Id,
            ProviderType: "Unsupported",
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            DefaultBranch: "main",
            PersonalAccessToken: "token",
            ContentKinds: [nameof(RepositoryContentKindsEnum.Infrastructure), nameof(RepositoryContentKindsEnum.ApplicationCode)]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.GitRepository.InvalidProviderType("Unsupported").Code);
        _projectRepository.DidNotReceive().Update(Arg.Any<Project>());
    }

    [Fact]
    public async Task Given_InvalidContentKinds_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = new AddProjectRepositoryCommand(
            _project.Id,
            ProviderType: null,
            RepositoryUrl: null,
            DefaultBranch: null,
            PersonalAccessToken: null,
            ContentKinds: ["Unsupported"]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.ProjectRepository.NoContentKind().Code);
        _projectRepository.DidNotReceive().Update(Arg.Any<Project>());
    }

    [Fact]
    public async Task Given_ConfiguredRepositoryWithoutPersonalAccessToken_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = new AddProjectRepositoryCommand(
            _project.Id,
            ProviderType: nameof(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            DefaultBranch: "main",
            PersonalAccessToken: null,
            ContentKinds: [nameof(RepositoryContentKindsEnum.Infrastructure), nameof(RepositoryContentKindsEnum.ApplicationCode)]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.ProjectRepository.PersonalAccessTokenRequired().Code);
        await _gitProviderService.DidNotReceive().ListBranchesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        _projectRepository.DidNotReceive().Update(Arg.Any<Project>());
    }

    [Fact]
    public async Task Given_DefaultBranchMissingFromRemote_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        _gitProviderService.ListBranchesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Branches(new GitBranchResult("develop", false)));
        var command = new AddProjectRepositoryCommand(
            _project.Id,
            ProviderType: nameof(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            DefaultBranch: "main",
            PersonalAccessToken: "token",
            ContentKinds: [nameof(RepositoryContentKindsEnum.Infrastructure), nameof(RepositoryContentKindsEnum.ApplicationCode)]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.ProjectRepository.DefaultBranchNotFound("main").Code);
        _projectRepository.DidNotReceive().Update(Arg.Any<Project>());
    }

    [Fact]
    public async Task Given_ConfiguredRepositoryWithValidBranch_When_Handle_Then_StoresSecretAndAddsRepositoryAsync()
    {
        // Arrange
        const string personalAccessToken = "token";
        var command = new AddProjectRepositoryCommand(
            _project.Id,
            ProviderType: nameof(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            DefaultBranch: "main",
            PersonalAccessToken: personalAccessToken,
            ContentKinds: [nameof(RepositoryContentKindsEnum.Infrastructure), nameof(RepositoryContentKindsEnum.ApplicationCode)]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Value.Should().NotBeEmpty();
        await _gitProviderService.Received(1).ListBranchesAsync(personalAccessToken, "floriandrevet", "infra", Arg.Any<CancellationToken>());
        await _keyVaultSecretClient.Received(1).SetSecretAsync(
            Arg.Is<string>(secretName => secretName.Contains(result.Value.Value.ToString(), StringComparison.Ordinal)),
            personalAccessToken,
            Arg.Any<CancellationToken>());
        _projectRepository.Received(1).Update(_project);
    }

    private static Task<ErrorOr<IReadOnlyList<GitBranchResult>>> Branches(params GitBranchResult[] branches)
    {
        ErrorOr<IReadOnlyList<GitBranchResult>> result = branches;
        return Task.FromResult(result);
    }
}
