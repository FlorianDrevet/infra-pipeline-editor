using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.EventHubNamespaces.Commands.UpdateEventHubNamespace;
using InfraFlowSculptor.Application.EventHubNamespaces.Common;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.EventHubNamespaces.Commands.UpdateEventHubNamespace;

public sealed class UpdateEventHubNamespaceCommandHandlerTests
{
    private readonly IEventHubNamespaceRepository _eventHubNamespaceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly EventHubNamespace _existingEntity;
    private readonly UpdateEventHubNamespaceCommand _command;
    private readonly UpdateEventHubNamespaceCommandHandler _sut;

    public UpdateEventHubNamespaceCommandHandlerTests()
    {
        _eventHubNamespaceRepository = Substitute.For<IEventHubNamespaceRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _existingEntity = EventHubNamespace.Create(
            _resourceGroup.Id,
            new Name("ehn-old"),
            new Location(Location.LocationEnum.FranceCentral));
        _command = new UpdateEventHubNamespaceCommand(
            _existingEntity.Id,
            new Name("ehn-renamed"),
            new Location(Location.LocationEnum.WestEurope));
        _eventHubNamespaceRepository.Update(Arg.Any<EventHubNamespace>())
            .Returns(callInfo => (EventHubNamespace)callInfo.Args()[0]);
        _sut = new UpdateEventHubNamespaceCommandHandler(
            _eventHubNamespaceRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_EntityNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _eventHubNamespaceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((EventHubNamespace?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _eventHubNamespaceRepository.DidNotReceive().Update(Arg.Any<EventHubNamespace>());
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _eventHubNamespaceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _eventHubNamespaceRepository.DidNotReceive().Update(Arg.Any<EventHubNamespace>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsUpdatedEntityAndMapsResultAsync()
    {
        // Arrange
        _eventHubNamespaceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _eventHubNamespaceRepository.Received(1).Update(Arg.Is<EventHubNamespace>(e =>
            e.Name.Value == "ehn-renamed"));
        _mapper.Received(1).Map<EventHubNamespaceResult>(Arg.Any<EventHubNamespace>());
    }
}
