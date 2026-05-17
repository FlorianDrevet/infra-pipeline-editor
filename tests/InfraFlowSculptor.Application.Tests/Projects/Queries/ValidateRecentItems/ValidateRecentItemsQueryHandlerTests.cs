using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Application.Projects.Queries.ValidateRecentItems;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.Projects.Queries.ValidateRecentItems;

public sealed class ValidateRecentItemsQueryHandlerTests
{
    private readonly IProjectRepository _projectRepository;
    private readonly IInfrastructureConfigRepository _configRepository;
    private readonly ICurrentUser _currentUser;
    private readonly UserId _userId;
    private readonly ValidateRecentItemsQueryHandler _sut;

    public ValidateRecentItemsQueryHandlerTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _configRepository = Substitute.For<IInfrastructureConfigRepository>();
        _currentUser = Substitute.For<ICurrentUser>();
        _userId = UserId.CreateUnique();
        _currentUser.GetUserIdAsync(Arg.Any<CancellationToken>()).Returns(_userId);
        _sut = new ValidateRecentItemsQueryHandler(_projectRepository, _configRepository, _currentUser);
    }

    [Fact]
    public async Task Given_EmptyItemList_When_Handle_Then_ReturnsEmptyListAsync()
    {
        // Arrange
        var query = new ValidateRecentItemsQuery([]);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        await _projectRepository.DidNotReceive()
            .GetProjectSummariesForUserAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_AccessibleProjectItems_When_Handle_Then_ReturnsMatchingItemsAsync()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectSummaries = new List<ProjectSummary>
        {
            new(projectId, "RetailApi", "A retail API project")
        };
        _projectRepository.GetProjectSummariesForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(projectSummaries);
        _configRepository.GetConfigSummariesForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new List<InfraConfigSummary>());
        var items = new List<RecentItemReference>
        {
            new(projectId, "project")
        };
        var query = new ValidateRecentItemsQuery(items);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value[0].Id.Should().Be(projectId.ToString());
        result.Value[0].Name.Should().Be("RetailApi");
        result.Value[0].Type.Should().Be("project");
        result.Value[0].Description.Should().Be("A retail API project");
    }

    [Fact]
    public async Task Given_InaccessibleItems_When_Handle_Then_FiltersThemOutAsync()
    {
        // Arrange
        var inaccessibleId = Guid.NewGuid();
        _projectRepository.GetProjectSummariesForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new List<ProjectSummary>());
        _configRepository.GetConfigSummariesForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new List<InfraConfigSummary>());
        var items = new List<RecentItemReference>
        {
            new(inaccessibleId, "project"),
            new(Guid.NewGuid(), "config")
        };
        var query = new ValidateRecentItemsQuery(items);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_MixedProjectAndConfigItems_When_Handle_Then_ReturnsBothTypesAsync()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var configId = Guid.NewGuid();
        _projectRepository.GetProjectSummariesForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new List<ProjectSummary> { new(projectId, "MyProject", null) });
        _configRepository.GetConfigSummariesForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new List<InfraConfigSummary> { new(configId, "DevConfig") });
        var items = new List<RecentItemReference>
        {
            new(projectId, "project"),
            new(configId, "config")
        };
        var query = new ValidateRecentItemsQuery(items);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(r => r.Type == "project" && r.Name == "MyProject");
        result.Value.Should().Contain(r => r.Type == "config" && r.Name == "DevConfig");
    }
}
