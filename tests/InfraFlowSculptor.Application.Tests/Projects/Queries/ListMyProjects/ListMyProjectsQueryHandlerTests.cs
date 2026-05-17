using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Queries.ListMyProjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Queries.ListMyProjects;

public sealed class ListMyProjectsQueryHandlerTests
{
    private readonly IProjectRepository _repository;
    private readonly ICurrentUser _currentUser;
    private readonly UserId _userId;
    private readonly ListMyProjectsQueryHandler _sut;

    public ListMyProjectsQueryHandlerTests()
    {
        _repository = Substitute.For<IProjectRepository>();
        _currentUser = Substitute.For<ICurrentUser>();
        _userId = UserId.CreateUnique();
        _currentUser.GetUserIdAsync(Arg.Any<CancellationToken>()).Returns(_userId);
        _sut = new ListMyProjectsQueryHandler(_repository, _currentUser);
    }

    [Fact]
    public async Task Given_UserHasProjects_When_Handle_Then_ReturnsProjectListAsync()
    {
        // Arrange
        var projects = new List<Project>
        {
            Project.Create(new Name("ProjectA"), "First project", _userId),
            Project.Create(new Name("ProjectB"), null, _userId)
        };
        _repository.GetAllForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(projects);
        var query = new ListMyProjectsQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value[0].Name.Value.Should().Be("ProjectA");
        result.Value[1].Name.Value.Should().Be("ProjectB");
    }

    [Fact]
    public async Task Given_UserHasNoProjects_When_Handle_Then_ReturnsEmptyListAsync()
    {
        // Arrange
        _repository.GetAllForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new List<Project>());
        var query = new ListMyProjectsQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }
}
