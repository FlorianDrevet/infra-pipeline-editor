using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Projects;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectResources;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Queries.ListProjectResources;

public sealed class ListProjectResourcesQueryHandlerTests
{
    private readonly IProjectAccessService _projectAccessService;
    private readonly IProjectResourceReadRepository _projectResourceReadRepository;
    private readonly Project _project;
    private readonly Guid _projectGuid;
    private readonly ListProjectResourcesQueryHandler _sut;

    public ListProjectResourcesQueryHandlerTests()
    {
        _projectAccessService = Substitute.For<IProjectAccessService>();
        _projectResourceReadRepository = Substitute.For<IProjectResourceReadRepository>();
        _project = Project.Create(new Name("TestProject"), null, UserId.CreateUnique());
        _projectGuid = _project.Id.Value;
        _sut = new ListProjectResourcesQueryHandler(
            _projectAccessService, _projectResourceReadRepository);
    }

    [Fact]
    public async Task Given_AccessGrantedAndNoResources_When_Handle_Then_ReturnsEmptyListAsync()
    {
        // Arrange
        _projectAccessService.VerifyReadAccessAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectResourceReadRepository.GetByProjectIdAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(new List<ProjectResourceResult>());
        var query = new ListProjectResourcesQuery(_projectGuid);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_AccessGranted_When_Handle_Then_ReturnsProjectedResourcesAsync()
    {
        // Arrange
        var resources = new List<ProjectResourceResult>
        {
            new(
                Guid.NewGuid(),
                "ca-api",
                "ContainerApp",
                "rg-app",
                Guid.NewGuid(),
                "dev")
        };
        _projectAccessService.VerifyReadAccessAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectResourceReadRepository.GetByProjectIdAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(resources);
        var query = new ListProjectResourcesQuery(_projectGuid);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(resources);
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        _projectAccessService.VerifyReadAccessAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns(Errors.Project.NotFoundError(new ProjectId(_projectGuid)));
        var query = new ListProjectResourcesQuery(_projectGuid);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _projectResourceReadRepository.DidNotReceive()
            .GetByProjectIdAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
    }
}
