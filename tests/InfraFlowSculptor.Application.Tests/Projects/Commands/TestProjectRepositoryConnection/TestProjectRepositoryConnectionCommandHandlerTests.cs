using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Commands.TestProjectRepositoryConnection;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.TestProjectRepositoryConnection;

public sealed class TestProjectRepositoryConnectionCommandHandlerTests
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAccessService _accessService;
    private readonly IKeyVaultSecretClient _keyVaultSecretClient;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly IGitProviderService _gitProviderService;
    private readonly Project _project;
    private readonly ProjectRepository _repository;
    private readonly TestProjectRepositoryConnectionCommandHandler _sut;

    public TestProjectRepositoryConnectionCommandHandlerTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _keyVaultSecretClient = Substitute.For<IKeyVaultSecretClient>();
        _gitProviderFactory = Substitute.For<IGitProviderFactory>();
        _gitProviderService = Substitute.For<IGitProviderService>();
        _project = CreateProject();
        _repository = AddRepository(_project, providerType: new GitProviderType(GitProviderTypeEnum.GitHub));
        _sut = new TestProjectRepositoryConnectionCommandHandler(
            _projectRepository,
            _accessService,
            _keyVaultSecretClient,
            _gitProviderFactory);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsUnauthorizedAsync()
    {
        // Arrange
        var command = new TestProjectRepositoryConnectionCommand(_project.Id, _repository.Id);
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
        var command = new TestProjectRepositoryConnectionCommand(_project.Id, _repository.Id);
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
    public async Task Given_RepositoryNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        var missingRepositoryId = ProjectRepositoryId.CreateUnique();
        var command = new TestProjectRepositoryConnectionCommand(_project.Id, missingRepositoryId);
        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.ProjectRepository.NotFound(missingRepositoryId).Code);
    }

    [Fact]
    public async Task Given_RepositorySlotIsNotConfigured_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var project = CreateProject();
        var repository = AddRepository(project, providerType: null);
        var command = new TestProjectRepositoryConnectionCommand(project.Id, repository.Id);

        _accessService.VerifyWriteAccessAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);
        _projectRepository.GetByIdWithAllAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.GitRouting.RepositorySlotNotConfigured(repository.Id).Code);
    }

    [Fact]
    public async Task Given_SecretRetrievalFails_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var command = new TestProjectRepositoryConnectionCommand(_project.Id, _repository.Id);
        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
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
        const string personalAccessToken = "ghp_token123";
        var command = new TestProjectRepositoryConnectionCommand(_project.Id, _repository.Id);
        var expectedResult = new TestGitConnectionResult(true, "owner/repo", "main", null);

        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _keyVaultSecretClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(personalAccessToken);
        _gitProviderFactory.Create(_repository.ProviderType!)
            .Returns(_gitProviderService);
        _gitProviderService.TestConnectionAsync(personalAccessToken, _repository.Owner!, _repository.RepositoryName!, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expectedResult);
        _gitProviderFactory.Received(1).Create(_repository.ProviderType!);
        await _gitProviderService.Received(1).TestConnectionAsync(
            personalAccessToken,
            _repository.Owner!,
            _repository.RepositoryName!,
            Arg.Any<CancellationToken>());
    }

    private static Project CreateProject()
    {
        var project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.AllInOne));
        return project;
    }

    private static ProjectRepository AddRepository(Project project, GitProviderType? providerType)
    {
        var contentKinds = RepositoryContentKinds.Create(
            RepositoryContentKindsEnum.Infrastructure | RepositoryContentKindsEnum.ApplicationCode).Value;
        var repositoryResult = project.AddRepository(
            providerType,
            providerType is null ? null : "https://github.com/owner/repo",
            providerType is null ? null : "main",
            contentKinds);

        return repositoryResult.Value;
    }
}