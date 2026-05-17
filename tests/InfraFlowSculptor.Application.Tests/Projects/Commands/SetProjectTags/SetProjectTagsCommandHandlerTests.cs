using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectTags;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.SetProjectTags;

public sealed class SetProjectTagsCommandHandlerTests
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAccessService _accessService;
    private readonly Project _project;
    private readonly SetProjectTagsCommandHandler _sut;

    public SetProjectTagsCommandHandlerTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _project = Project.Create(new Name("test-project"), "Test project", UserId.CreateUnique());
        _sut = new SetProjectTagsCommandHandler(_projectRepository, _accessService);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var command = new SetProjectTagsCommand(
            _project.Id.Value, [("env", "prod")]);
        _accessService.VerifyWriteAccessAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns(Error.Forbidden());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        _projectRepository.DidNotReceive().Update(Arg.Any<Project>());
    }

    [Fact]
    public async Task Given_ValidTags_When_Handle_Then_SetsTagsAndReturnsUpdatedAsync()
    {
        // Arrange
        var tags = new List<(string Name, string Value)>
        {
            ("environment", "production"),
            ("team", "backend")
        };
        var command = new SetProjectTagsCommand(_project.Id.Value, tags);
        _accessService.VerifyWriteAccessAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns(_project);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Updated);
        _project.Tags.Should().HaveCount(2);
        _projectRepository.Received(1).Update(_project);
    }

    [Fact]
    public async Task Given_EmptyTags_When_Handle_Then_ClearsTagsAndReturnsUpdatedAsync()
    {
        // Arrange
        _project.SetTags([new Tag("old-key", "old-value")]);
        var command = new SetProjectTagsCommand(_project.Id.Value, []);
        _accessService.VerifyWriteAccessAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns(_project);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Updated);
        _project.Tags.Should().BeEmpty();
        _projectRepository.Received(1).Update(_project);
    }
}
