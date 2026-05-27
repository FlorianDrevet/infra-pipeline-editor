using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListIncomingCrossConfigReferences;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Queries.ListIncomingCrossConfigReferences;

public sealed class ListIncomingCrossConfigReferencesQueryHandlerTests
{
    private readonly IInfraConfigAccessService _accessService;
    private readonly IInfrastructureConfigRepository _infraConfigRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly ListIncomingCrossConfigReferencesQueryHandler _sut;

    public ListIncomingCrossConfigReferencesQueryHandlerTests()
    {
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _infraConfigRepository = Substitute.For<IInfrastructureConfigRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _sut = new ListIncomingCrossConfigReferencesQueryHandler(
            _accessService, _infraConfigRepository, _resourceGroupRepository);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var query = new ListIncomingCrossConfigReferencesQuery(configId);
        _accessService.VerifyReadAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(Error.NotFound("InfrastructureConfig.NotFound", "Not found"));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_NoSiblingConfigs_When_Handle_Then_ReturnsEmptyListAsync()
    {
        // Arrange
        var projectId = ProjectId.CreateUnique();
        var config = DomainInfrastructureConfig.Create(new Name("MainConfig"), projectId);
        var configId = config.Id.Value;
        var query = new ListIncomingCrossConfigReferencesQuery(configId);

        _accessService.VerifyReadAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(config);
        _infraConfigRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<DomainInfrastructureConfig> { config });

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_SiblingWithNoCrossConfigRefs_When_Handle_Then_ReturnsEmptyListAsync()
    {
        // Arrange
        var projectId = ProjectId.CreateUnique();
        var config = DomainInfrastructureConfig.Create(new Name("MainConfig"), projectId);
        var sibling = DomainInfrastructureConfig.Create(new Name("SiblingConfig"), projectId);
        var query = new ListIncomingCrossConfigReferencesQuery(config.Id.Value);

        _accessService.VerifyReadAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(config);
        _infraConfigRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<DomainInfrastructureConfig> { config, sibling });
        _infraConfigRepository.GetByIdWithMembersAsync(sibling.Id, Arg.Any<CancellationToken>())
            .Returns(sibling);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }
}
