using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Commands.TestGitConnection;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.TestGitConnection;

public sealed class TestGitConnectionCommandHandlerTests
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAccessService _accessService;
    private readonly IKeyVaultSecretClient _keyVaultSecretClient;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly IRepositoryTargetResolver _targetResolver;
    private readonly IGitProviderService _gitProviderService;
    private readonly Project _project;
    private readonly TestGitConnectionCommandHandler _sut;

    public TestGitConnectionCommandHandlerTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _keyVaultSecretClient = Substitute.For<IKeyVaultSecretClient>();
        _gitProviderFactory = Substitute.For<IGitProviderFactory>();
        _targetResolver = Substitute.For<IRepositoryTargetResolver>();
        _gitProviderService = Substitute.For<IGitProviderService>();
        _project = Project.Create(new Name("test-project"), "Test project", UserId.CreateUnique());
        _sut = new TestGitConnectionCommandHandler(
            _projectRepository, _accessService, _keyVaultSecretClient, _gitProviderFactory, _targetResolver);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsUnauthorizedAsync()
    {
        // Arrange
        var command = new TestGitConnectionCommand(_project.Id);
        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Given_ProjectNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        var command = new TestGitConnectionCommand(_project.Id);
        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns((Project?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_TargetResolutionFails_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var command = new TestGitConnectionCommand(_project.Id);
        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _targetResolver.Resolve(_project, null, ArtifactKind.Infrastructure)
            .Returns(Error.Failure("GitRouting.NoRepositoryConfigured", "No repository configured."));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("GitRouting.NoRepositoryConfigured");
    }

    [Fact]
    public async Task Given_SecretRetrievalFails_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var command = new TestGitConnectionCommand(_project.Id);
        var target = new ResolvedRepositoryTarget(
            "default",
            new GitProviderType(GitProviderTypeEnum.GitHub),
            "https://github.com/owner/repo",
            "owner", "repo", "main", null, null, null);

        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _targetResolver.Resolve(_project, null, ArtifactKind.Infrastructure)
            .Returns(target);
        _keyVaultSecretClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("KeyVault.SecretNotFound", "Secret not found."));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("KeyVault.SecretNotFound");
    }

    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_ReturnsConnectionResultAsync()
    {
        // Arrange
        var command = new TestGitConnectionCommand(_project.Id);
        var target = new ResolvedRepositoryTarget(
            "default",
            new GitProviderType(GitProviderTypeEnum.GitHub),
            "https://github.com/owner/repo",
            "owner", "repo", "main", null, null, null);
        var expectedResult = new TestGitConnectionResult(true, "owner/repo", "main", null);

        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _targetResolver.Resolve(_project, null, ArtifactKind.Infrastructure)
            .Returns(target);
        _keyVaultSecretClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("ghp_token123");
        _gitProviderFactory.Create(target.ProviderType)
            .Returns(_gitProviderService);
        _gitProviderService.TestConnectionAsync("ghp_token123", "owner", "repo", Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Success.Should().BeTrue();
        result.Value.RepositoryFullName.Should().Be("owner/repo");
    }
}
