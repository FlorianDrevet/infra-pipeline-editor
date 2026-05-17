using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Application.Projects.Queries.ListGitBranches;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using static InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects.GitProviderTypeEnum;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Queries.ListGitBranches;

public sealed class ListGitBranchesQueryHandlerTests
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAccessService _accessService;
    private readonly IKeyVaultSecretClient _keyVaultSecretClient;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly IRepositoryTargetResolver _targetResolver;
    private readonly IGitProviderService _gitProviderService;
    private readonly Project _project;
    private readonly ProjectId _projectId;
    private readonly ListGitBranchesQueryHandler _sut;

    public ListGitBranchesQueryHandlerTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _keyVaultSecretClient = Substitute.For<IKeyVaultSecretClient>();
        _gitProviderFactory = Substitute.For<IGitProviderFactory>();
        _targetResolver = Substitute.For<IRepositoryTargetResolver>();
        _gitProviderService = Substitute.For<IGitProviderService>();
        _project = Project.Create(new Name("TestProject"), null, UserId.CreateUnique());
        _projectId = _project.Id;
        _sut = new ListGitBranchesQueryHandler(
            _projectRepository, _accessService, _keyVaultSecretClient,
            _gitProviderFactory, _targetResolver);
    }

    [Fact]
    public async Task Given_AccessGrantedAndBranchesExist_When_Handle_Then_ReturnsBranchListAsync()
    {
        // Arrange
        var expectedBranches = new List<GitBranchResult>
        {
            new("main", true),
            new("develop", false)
        };
        var providerType = new GitProviderType(GitProviderTypeEnum.GitHub);
        var resolvedTarget = new ResolvedRepositoryTarget(
            "default", providerType, "https://github.com/org/repo",
            "org", "repo", "main", null, null, null);

        _accessService.VerifyReadAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _targetResolver.Resolve(_project, null, ArtifactKind.Infrastructure)
            .Returns(resolvedTarget);
        _keyVaultSecretClient.GetSecretAsync($"git-pat-{_project.Id.Value}", Arg.Any<CancellationToken>())
            .Returns("fake-pat-token");
        _gitProviderFactory.Create(providerType)
            .Returns(_gitProviderService);
        _gitProviderService.ListBranchesAsync("fake-pat-token", "org", "repo", Arg.Any<CancellationToken>())
            .Returns(expectedBranches);
        var query = new ListGitBranchesQuery(_projectId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value.Should().BeEquivalentTo(expectedBranches);
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        _accessService.VerifyReadAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(Errors.Project.NotFoundError(_projectId));
        var query = new ListGitBranchesQuery(_projectId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _projectRepository.DidNotReceive().GetByIdWithAllAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_AccessGrantedButProjectNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _accessService.VerifyReadAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns((Project?)null);
        var query = new ListGitBranchesQuery(_projectId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}
