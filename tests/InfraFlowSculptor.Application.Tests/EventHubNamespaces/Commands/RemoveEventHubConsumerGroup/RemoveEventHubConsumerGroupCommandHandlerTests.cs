using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.EventHubNamespaces.Commands.RemoveEventHubConsumerGroup;
using InfraFlowSculptor.Application.EventHubNamespaces.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.EventHubNamespaces.Commands.RemoveEventHubConsumerGroup;

public sealed class RemoveEventHubConsumerGroupCommandHandlerTests
{
    private readonly IEventHubNamespaceRepository _eventHubNamespaceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly EventHubNamespace _eventHubNamespace;
    private readonly RemoveEventHubConsumerGroupCommand _command;
    private readonly RemoveEventHubConsumerGroupCommandHandler _sut;

    public RemoveEventHubConsumerGroupCommandHandlerTests()
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
        _eventHubNamespace = EventHubNamespace.Create(
            _resourceGroup.Id,
            new Name("ehn-shared"),
            new Location(Location.LocationEnum.FranceCentral));
        _command = new RemoveEventHubConsumerGroupCommand(
            _eventHubNamespace.Id,
            Guid.NewGuid());
        _sut = new RemoveEventHubConsumerGroupCommandHandler(
            _eventHubNamespaceRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_NamespaceNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _eventHubNamespaceRepository.GetByIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns((EventHubNamespace?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _eventHubNamespaceRepository.GetByIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_eventHubNamespace);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsUnauthorizedAsync()
    {
        // Arrange
        _eventHubNamespaceRepository.GetByIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_eventHubNamespace);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Given_ConsumerGroupNotFound_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange — no consumer group with this ID
        _eventHubNamespaceRepository.GetByIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_eventHubNamespace);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}
