using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Commands.DeleteProject;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.DeleteProject;

public sealed class DeleteProjectCommandHandlerTests
{
    private const string ProjectName = "RetailApi";

    private readonly IProjectAccessService _accessService;
    private readonly IProjectRepository _repository;
    private readonly ProjectId _projectId;
    private readonly Project _project;
    private readonly DeleteProjectCommandHandler _sut;

    public DeleteProjectCommandHandlerTests()
    {
        _accessService = Substitute.For<IProjectAccessService>();
        _repository = Substitute.For<IProjectRepository>();
        _project = Project.Create(new Name(ProjectName), null, UserId.CreateUnique());
        _projectId = _project.Id;
        _sut = new DeleteProjectCommandHandler(_accessService, _repository);
    }

    [Fact]
    public async Task Given_OwnerAccessGranted_When_Handle_Then_DeletesProjectAsync()
    {
        // Arrange
        _accessService.VerifyOwnerAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _repository.DeleteAsync(_projectId).Returns(true);
        var command = new DeleteProjectCommand(_projectId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        await _repository.Received(1).DeleteAsync(_projectId);
    }

    [Fact]
    public async Task Given_OwnerAccessDenied_When_Handle_Then_ReturnsErrorAndDoesNotDeleteAsync()
    {
        // Arrange
        var forbiddenError = Errors.Project.ForbiddenError();
        _accessService.VerifyOwnerAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(forbiddenError);
        var command = new DeleteProjectCommand(_projectId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<ProjectId>());
    }

    [Fact]
    public async Task Given_ProjectNotFound_When_Handle_Then_ReturnsNotFoundAndDoesNotDeleteAsync()
    {
        // Arrange
        _accessService.VerifyOwnerAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(Errors.Project.NotFoundError(_projectId));
        var command = new DeleteProjectCommand(_projectId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<ProjectId>());
    }
}
