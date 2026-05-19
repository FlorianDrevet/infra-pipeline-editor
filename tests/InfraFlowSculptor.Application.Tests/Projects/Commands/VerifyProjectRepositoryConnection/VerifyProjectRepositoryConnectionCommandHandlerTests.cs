using FluentAssertions;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Commands.VerifyProjectRepositoryConnection;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.VerifyProjectRepositoryConnection;

public sealed class VerifyProjectRepositoryConnectionCommandHandlerTests
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAccessService _accessService;
    private readonly IKeyVaultSecretClient _keyVaultSecretClient;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly IGitProviderService _gitProviderService;
    private readonly Project _project;
    private readonly ProjectRepository _repository;
    private readonly VerifyProjectRepositoryConnectionCommandHandler _sut;

    public VerifyProjectRepositoryConnectionCommandHandlerTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _keyVaultSecretClient = Substitute.For<IKeyVaultSecretClient>();
        _gitProviderFactory = Substitute.For<IGitProviderFactory>();
        _gitProviderService = Substitute.For<IGitProviderService>();
        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.AllInOne));
        _repository = AddRepository(_project);

        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _keyVaultSecretClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("stored-token");
        _gitProviderFactory.Create(Arg.Any<GitProviderType>())
            .Returns(_gitProviderService);
        _gitProviderService.ListBranchesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Branches(
                new GitBranchResult("main", false),
                new GitBranchResult("develop", true)));

        _sut = new VerifyProjectRepositoryConnectionCommandHandler(
            _projectRepository,
            _accessService,
            _keyVaultSecretClient,
            _gitProviderFactory);
    }

    [Fact]
    public async Task Given_CreateVerificationWithoutPersonalAccessToken_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = new VerifyProjectRepositoryConnectionCommand(
            _project.Id,
            RepositoryId: null,
            ProviderType: nameof(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            PersonalAccessToken: null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.ProjectRepository.PersonalAccessTokenRequired().Code);
        await _gitProviderService.DidNotReceive().ListBranchesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_CreateVerificationWithToken_When_Handle_Then_ReturnsBranchesAsync()
    {
        // Arrange
        var command = new VerifyProjectRepositoryConnectionCommand(
            _project.Id,
            RepositoryId: null,
            ProviderType: nameof(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            PersonalAccessToken: "token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Owner.Should().Be("floriandrevet");
        result.Value.RepositoryName.Should().Be("infra");
        result.Value.Branches.Should().Contain(branch => branch.Name == "main" && !branch.IsProtected);
        result.Value.DefaultBranchCandidate.Should().Be("main");
        await _gitProviderService.Received(1).ListBranchesAsync("token", "floriandrevet", "infra", Arg.Any<CancellationToken>());
        _projectRepository.DidNotReceive().Update(Arg.Any<Project>());
        await _keyVaultSecretClient.DidNotReceive().SetSecretAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_EditVerificationWithoutToken_When_Handle_Then_UsesStoredRepositorySecretAsync()
    {
        // Arrange
        var command = new VerifyProjectRepositoryConnectionCommand(
            _project.Id,
            _repository.Id,
            ProviderType: nameof(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            PersonalAccessToken: null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        await _keyVaultSecretClient.Received(1).GetSecretAsync(
            Arg.Is<string>(secretName => secretName.Contains(_repository.Id.Value.ToString(), StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
        await _gitProviderService.Received(1).ListBranchesAsync("stored-token", "floriandrevet", "infra", Arg.Any<CancellationToken>());
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