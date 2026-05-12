using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
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
}