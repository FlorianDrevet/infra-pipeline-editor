using FluentAssertions;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Common;

public sealed class InfraConfigAccessServiceTests
{
    private readonly IInfrastructureConfigRepository _configRepository;
    private readonly IProjectAccessService _projectAccessService;
    private readonly InfraConfigAccessService _sut;

    public InfraConfigAccessServiceTests()
    {
        _configRepository = Substitute.For<IInfrastructureConfigRepository>();
        _projectAccessService = Substitute.For<IProjectAccessService>();
        _sut = new InfraConfigAccessService(_configRepository, _projectAccessService);
    }

    [Fact]
    public async Task Given_ExistingConfigInReadableProject_When_VerifyReadAccessAsync_Then_UsesReadOnlyConfigLookup_Async()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var project = Project.Create(new Name("alpha-project"), "shared workload", userId);
        var config = DomainInfrastructureConfig.Create(new Name("primary"), project.Id);

        _configRepository.GetByIdReadOnlyAsync(config.Id, Arg.Any<CancellationToken>())
            .Returns(config);
        _projectAccessService.VerifyReadAccessAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);

        // Act
        var result = await _sut.VerifyReadAccessAsync(config.Id, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(config);
        await _configRepository.Received(1)
            .GetByIdReadOnlyAsync(config.Id, Arg.Any<CancellationToken>());
        await _configRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<InfraFlowSculptor.Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ProjectReadAccessError_When_VerifyReadAccessAsync_Then_MasksAsConfigNotFound_Async()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var project = Project.Create(new Name("alpha-project"), "shared workload", userId);
        var config = DomainInfrastructureConfig.Create(new Name("primary"), project.Id);

        _configRepository.GetByIdReadOnlyAsync(config.Id, Arg.Any<CancellationToken>())
            .Returns(config);
        _projectAccessService.VerifyReadAccessAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(Errors.Project.NotFoundError(project.Id));

        // Act
        var result = await _sut.VerifyReadAccessAsync(config.Id, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be(Errors.InfrastructureConfig.NotFoundError(config.Id).Code);
    }

    [Fact]
    public async Task Given_ProjectWriteAccessForbidden_When_VerifyWriteAccessAsync_Then_PropagatesForbidden_Async()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var project = Project.Create(new Name("alpha-project"), "shared workload", userId);
        var config = DomainInfrastructureConfig.Create(new Name("primary"), project.Id);

        _configRepository.GetByIdAsync(config.Id, Arg.Any<CancellationToken>())
            .Returns(config);
        _projectAccessService.VerifyWriteAccessAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(Errors.Project.ForbiddenError());

        // Act
        var result = await _sut.VerifyWriteAccessAsync(config.Id, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        result.FirstError.Code.Should().Be(Errors.Project.ForbiddenError().Code);
    }
}
