using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.CheckResourceNameAvailability;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Queries.CheckResourceNameAvailability;

public sealed class CheckResourceNameAvailabilityQueryHandlerTests
{
    private readonly IProjectAccessService _projectAccessService;
    private readonly IResourceNameResolver _resolver;
    private readonly IAzureNameAvailabilityChecker _checker;
    private readonly CheckResourceNameAvailabilityQueryHandler _sut;
    private readonly ProjectId _projectId;

    public CheckResourceNameAvailabilityQueryHandlerTests()
    {
        _projectAccessService = Substitute.For<IProjectAccessService>();
        _resolver = Substitute.For<IResourceNameResolver>();
        _checker = Substitute.For<IAzureNameAvailabilityChecker>();
        _sut = new CheckResourceNameAvailabilityQueryHandler(_projectAccessService, _resolver, _checker);
        _projectId = ProjectId.CreateUnique();
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var query = new CheckResourceNameAvailabilityQuery(
            _projectId, null, "ContainerRegistry", "myacr");

        _projectAccessService.VerifyReadAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(Error.NotFound());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ResolverFails_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var query = new CheckResourceNameAvailabilityQuery(
            _projectId, null, "ContainerRegistry", "myacr");

        _projectAccessService.VerifyReadAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(Domain.ProjectAggregate.Project.Create(
                new Domain.Common.ValueObjects.Name("test-project"),
                "desc",
                UserId.CreateUnique()));

        _resolver.ResolveAsync(
                _projectId, Arg.Any<InfrastructureConfigId?>(), "ContainerRegistry", "myacr", Arg.Any<CancellationToken>())
            .Returns(Error.Failure("resolve.failed", "Template resolution failed"));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Given_NameAvailable_When_Handle_Then_ReturnsAvailableStatusAsync()
    {
        // Arrange
        var query = new CheckResourceNameAvailabilityQuery(
            _projectId, null, "ContainerRegistry", "myacr");

        _projectAccessService.VerifyReadAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(Domain.ProjectAggregate.Project.Create(
                new Domain.Common.ValueObjects.Name("test-project"),
                "desc",
                UserId.CreateUnique()));

        var resolvedNames = new List<ResolvedResourceName>
        {
            new("Development", "dev", "sub-123", "acrdevmyacr", "{env}{name}")
        };

        _resolver.ResolveAsync(
                _projectId, Arg.Any<InfrastructureConfigId?>(), "ContainerRegistry", "myacr", Arg.Any<CancellationToken>())
            .Returns(resolvedNames);

        _checker.Supports("ContainerRegistry").Returns(true);
        _checker.CheckAsync("ContainerRegistry", "sub-123", "acrdevmyacr", Arg.Any<CancellationToken>())
            .Returns(AzureNameAvailabilityResult.Available);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ResourceType.Should().Be("ContainerRegistry");
        result.Value.RawName.Should().Be("myacr");
        result.Value.Supported.Should().BeTrue();
        result.Value.Environments.Should().HaveCount(1);
        result.Value.Environments[0].Status.Should().Be("available");
    }

    [Fact]
    public async Task Given_NameUnchanged_When_Handle_Then_ReturnsCurrentStatusAsync()
    {
        // Arrange
        var query = new CheckResourceNameAvailabilityQuery(
            _projectId, null, "ContainerRegistry", "myacr", CurrentPersistedName: "myacr");

        _projectAccessService.VerifyReadAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(Domain.ProjectAggregate.Project.Create(
                new Domain.Common.ValueObjects.Name("test-project"),
                "desc",
                UserId.CreateUnique()));

        var resolvedNames = new List<ResolvedResourceName>
        {
            new("Development", "dev", "sub-123", "acrdevmyacr", "{env}{name}")
        };

        _resolver.ResolveAsync(
                _projectId, Arg.Any<InfrastructureConfigId?>(), "ContainerRegistry", "myacr", Arg.Any<CancellationToken>())
            .Returns(resolvedNames);

        _checker.Supports("ContainerRegistry").Returns(true);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Environments.Should().HaveCount(1);
        result.Value.Environments[0].Status.Should().Be("current");
    }
}
