using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.UpdateServiceBusNamespace;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Common;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.ServiceBusNamespaces.Commands.UpdateServiceBusNamespace;

public sealed class UpdateServiceBusNamespaceCommandHandlerTests
{
    private readonly IServiceBusNamespaceRepository _serviceBusNamespaceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly ServiceBusNamespace _existingEntity;
    private readonly UpdateServiceBusNamespaceCommand _command;
    private readonly UpdateServiceBusNamespaceCommandHandler _sut;

    public UpdateServiceBusNamespaceCommandHandlerTests()
    {
        _serviceBusNamespaceRepository = Substitute.For<IServiceBusNamespaceRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _existingEntity = ServiceBusNamespace.Create(
            _resourceGroup.Id,
            new Name("sbn-old"),
            new Location(Location.LocationEnum.FranceCentral));
        _command = new UpdateServiceBusNamespaceCommand(
            _existingEntity.Id,
            new Name("sbn-renamed"),
            new Location(Location.LocationEnum.WestEurope));
        _serviceBusNamespaceRepository.Update(Arg.Any<ServiceBusNamespace>())
            .Returns(callInfo => (ServiceBusNamespace)callInfo.Args()[0]);
        _sut = new UpdateServiceBusNamespaceCommandHandler(
            _serviceBusNamespaceRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_EntityNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _serviceBusNamespaceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((ServiceBusNamespace?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _serviceBusNamespaceRepository.DidNotReceive().Update(Arg.Any<ServiceBusNamespace>());
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _serviceBusNamespaceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _serviceBusNamespaceRepository.DidNotReceive().Update(Arg.Any<ServiceBusNamespace>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsUpdatedEntityAndMapsResultAsync()
    {
        // Arrange
        _serviceBusNamespaceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _serviceBusNamespaceRepository.Received(1).Update(Arg.Is<ServiceBusNamespace>(e =>
            e.Name.Value == "sbn-renamed"));
        _mapper.Received(1).Map<ServiceBusNamespaceResult>(Arg.Any<ServiceBusNamespace>());
    }
}
