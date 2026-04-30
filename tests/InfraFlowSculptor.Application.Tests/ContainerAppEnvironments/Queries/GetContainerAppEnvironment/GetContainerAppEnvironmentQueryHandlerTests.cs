using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Common;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Queries.GetContainerAppEnvironment;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.ContainerAppEnvironments.Queries.GetContainerAppEnvironment;

public sealed class GetContainerAppEnvironmentQueryHandlerTests
{
    private readonly IContainerAppEnvironmentRepository _environmentRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly ContainerAppEnvironment _environment;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly GetContainerAppEnvironmentQuery _query;
    private readonly GetContainerAppEnvironmentQueryHandler _sut;

    public GetContainerAppEnvironmentQueryHandlerTests()
    {
        _environmentRepository = Substitute.For<IContainerAppEnvironmentRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _environment = ContainerAppEnvironment.Create(
            _resourceGroup.Id,
            new Name("cae-shared"),
            new Location(Location.LocationEnum.FranceCentral));
        _query = new GetContainerAppEnvironmentQuery(_environment.Id);
        _sut = new GetContainerAppEnvironmentQueryHandler(
            _environmentRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_EnvironmentNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _environmentRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((ContainerAppEnvironment?)null);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ReadAccessGranted_When_Handle_Then_MapsResultAsync()
    {
        // Arrange
        _environmentRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_environment);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _mapper.Received(1).Map<ContainerAppEnvironmentResult>(_environment);
    }
}
