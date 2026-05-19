using FluentAssertions;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Commands.UpdateProjectRepository;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.UpdateProjectRepository;

public sealed class UpdateProjectRepositoryCommandHandlerTests
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAccessService _accessService;
    private readonly IKeyVaultSecretClient _keyVaultSecretClient;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly IGitProviderService _gitProviderService;
    private readonly Project _project;
    private readonly ProjectRepository _repository;
    private readonly UpdateProjectRepositoryCommandHandler _sut;

    public UpdateProjectRepositoryCommandHandlerTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _keyVaultSecretClient = Substitute.For<IKeyVaultSecretClient>();
        _gitProviderFactory = Substitute.For<IGitProviderFactory>();
        _gitProviderService = Substitute.For<IGitProviderService>();
        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.AllInOne));
        _repository = AddRepository(_project);

        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _keyVaultSecretClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("existing-token");
        _keyVaultSecretClient.SetSecretAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);
        _gitProviderFactory.Create(Arg.Any<GitProviderType>())
            .Returns(_gitProviderService);
        _gitProviderService.ListBranchesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Branches(new GitBranchResult("main", false)));

        _sut = new UpdateProjectRepositoryCommandHandler(
            _projectRepository,
            _accessService,
            _keyVaultSecretClient,
            _gitProviderFactory);
    }

    [Fact]
    public async Task Given_InvalidProviderType_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = new UpdateProjectRepositoryCommand(
            _project.Id,
            _repository.Id,
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
        var command = new UpdateProjectRepositoryCommand(
            _project.Id,
            _repository.Id,
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
    public async Task Given_ConfiguredRepositoryWithoutNewToken_When_Handle_Then_UsesStoredTokenForBranchValidationAsync()
    {
        // Arrange
        var command = new UpdateProjectRepositoryCommand(
            _project.Id,
            _repository.Id,
            ProviderType: nameof(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            DefaultBranch: "main",
            PersonalAccessToken: null,
            ContentKinds: [nameof(RepositoryContentKindsEnum.Infrastructure), nameof(RepositoryContentKindsEnum.ApplicationCode)]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        await _gitProviderService.Received(1).ListBranchesAsync("existing-token", "floriandrevet", "infra", Arg.Any<CancellationToken>());
        await _keyVaultSecretClient.DidNotReceive().SetSecretAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        _projectRepository.Received(1).Update(_project);
    }

    [Fact]
    public async Task Given_ConfiguredRepositoryWithNewToken_When_Handle_Then_StoresNewTokenAfterBranchValidationAsync()
    {
        // Arrange
        const string personalAccessToken = "new-token";
        var command = new UpdateProjectRepositoryCommand(
            _project.Id,
            _repository.Id,
            ProviderType: nameof(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            DefaultBranch: "main",
            PersonalAccessToken: personalAccessToken,
            ContentKinds: [nameof(RepositoryContentKindsEnum.Infrastructure), nameof(RepositoryContentKindsEnum.ApplicationCode)]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        await _gitProviderService.Received(1).ListBranchesAsync(personalAccessToken, "floriandrevet", "infra", Arg.Any<CancellationToken>());
        await _keyVaultSecretClient.Received(1).SetSecretAsync(
            Arg.Is<string>(secretName => secretName.Contains(_repository.Id.Value.ToString(), StringComparison.Ordinal)),
            personalAccessToken,
            Arg.Any<CancellationToken>());
        _projectRepository.Received(1).Update(_project);
    }

    [Fact]
    public async Task Given_DefaultBranchMissingFromRemote_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        _gitProviderService.ListBranchesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Branches(new GitBranchResult("develop", false)));
        var command = new UpdateProjectRepositoryCommand(
            _project.Id,
            _repository.Id,
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

    private static ProjectRepository AddRepository(Project project)
    {
        var contentKinds = RepositoryContentKinds.Create(
            RepositoryContentKindsEnum.Infrastructure | RepositoryContentKindsEnum.ApplicationCode).Value;

        return project.AddRepository(
            new GitProviderType(GitProviderTypeEnum.GitHub),
            "https://github.com/floriandrevet/original",
            "main",
            contentKinds).Value;
    }

    private static Task<ErrorOr<IReadOnlyList<GitBranchResult>>> Branches(params GitBranchResult[] branches)
    {
        ErrorOr<IReadOnlyList<GitBranchResult>> result = branches;
        return Task.FromResult(result);
    }
}
