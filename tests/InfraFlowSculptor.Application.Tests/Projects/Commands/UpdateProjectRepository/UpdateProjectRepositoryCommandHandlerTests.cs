using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Commands.UpdateProjectRepository;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.UpdateProjectRepository;

public sealed class UpdateProjectRepositoryCommandHandlerTests
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAccessService _accessService;
    private readonly Project _project;
    private readonly UpdateProjectRepositoryCommandHandler _sut;

    public UpdateProjectRepositoryCommandHandlerTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());

        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);

        _sut = new UpdateProjectRepositoryCommandHandler(_projectRepository, _accessService);
    }

    [Fact]
    public async Task Given_InvalidProviderType_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = new UpdateProjectRepositoryCommand(
            _project.Id,
            ProjectRepositoryId.CreateUnique(),
            ProviderType: "Unsupported",
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            DefaultBranch: "main",
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
            ProjectRepositoryId.CreateUnique(),
            ProviderType: null,
            RepositoryUrl: null,
            DefaultBranch: null,
            ContentKinds: ["Unsupported"]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.ProjectRepository.NoContentKind().Code);
        _projectRepository.DidNotReceive().Update(Arg.Any<Project>());
    }
}
