using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectGitPat;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.SetProjectGitPat;

public sealed class SetProjectGitPatCommandHandlerTests
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAccessService _accessService;
    private readonly IKeyVaultSecretClient _keyVaultSecretClient;
    private readonly Project _project;
    private readonly ProjectRepository _repository;
    private readonly SetProjectGitPatCommandHandler _sut;

    public SetProjectGitPatCommandHandlerTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _keyVaultSecretClient = Substitute.For<IKeyVaultSecretClient>();
        _project = CreateProjectWithRepository();
        _repository = _project.Repositories.First();
        _sut = new SetProjectGitPatCommandHandler(_projectRepository, _accessService, _keyVaultSecretClient);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsUnauthorizedAsync()
    {
        // Arrange
        var command = new SetProjectGitPatCommand(_project.Id, _repository.Id, "ghp_token123");
        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
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
        var command = new SetProjectGitPatCommand(_project.Id, _repository.Id, "ghp_token123");
        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
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
    public async Task Given_RepositoryNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        var command = new SetProjectGitPatCommand(_project.Id, ProjectRepositoryId.CreateUnique(), "ghp_token123");
        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_KeyVaultStorageFails_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var command = new SetProjectGitPatCommand(_project.Id, _repository.Id, "ghp_token123");
        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _keyVaultSecretClient.SetSecretAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("KeyVault.WriteFailed", "Secret write failed."));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("KeyVault.WriteFailed");
    }

    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_StoresRepositoryPatAsync()
    {
        // Arrange
        const string personalAccessToken = "ghp_token123";
        var command = new SetProjectGitPatCommand(_project.Id, _repository.Id, personalAccessToken);

        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _keyVaultSecretClient.SetSecretAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        await _keyVaultSecretClient.Received(1).SetSecretAsync(
            $"git-pat-repo-{_repository.Id.Value}",
            personalAccessToken,
            Arg.Any<CancellationToken>());
    }

    private static Project CreateProjectWithRepository()
    {
        var project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.AllInOne));
        var contentKinds = RepositoryContentKinds.Create(
            RepositoryContentKindsEnum.Infrastructure | RepositoryContentKindsEnum.ApplicationCode).Value;
        project.AddRepository(
            new GitProviderType(GitProviderTypeEnum.GitHub),
            "https://github.com/owner/repo",
            "main",
            contentKinds);
        return project;
    }
}