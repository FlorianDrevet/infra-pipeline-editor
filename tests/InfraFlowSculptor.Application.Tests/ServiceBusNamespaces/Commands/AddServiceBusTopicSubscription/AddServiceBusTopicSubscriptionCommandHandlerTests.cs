using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.AddServiceBusTopicSubscription;
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

namespace InfraFlowSculptor.Application.Tests.ServiceBusNamespaces.Commands.AddServiceBusTopicSubscription;

public sealed class AddServiceBusTopicSubscriptionCommandHandlerTests
{
    private const string TopicName = "topic-events";
    private const string SubscriptionName = "sub-billing";

    private readonly IServiceBusNamespaceRepository _serviceBusNamespaceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly ServiceBusNamespace _serviceBusNamespace;
    private readonly AddServiceBusTopicSubscriptionCommand _command;
    private readonly AddServiceBusTopicSubscriptionCommandHandler _sut;

    public AddServiceBusTopicSubscriptionCommandHandlerTests()
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
        _serviceBusNamespace = ServiceBusNamespace.Create(
            _resourceGroup.Id,
            new Name("sbns-shared"),
            new Location(Location.LocationEnum.FranceCentral));
        _command = new AddServiceBusTopicSubscriptionCommand(
            _serviceBusNamespace.Id, TopicName, SubscriptionName);
        _sut = new AddServiceBusTopicSubscriptionCommandHandler(
            _serviceBusNamespaceRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_ServiceBusNamespaceNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _serviceBusNamespaceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((ServiceBusNamespace?)null);

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
        _serviceBusNamespaceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_serviceBusNamespace);
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
        _serviceBusNamespaceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_serviceBusNamespace);
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
    public async Task Given_WriteAccessGranted_When_Handle_Then_AddsTopicSubscriptionAndMapsResultAsync()
    {
        // Arrange
        _serviceBusNamespaceRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_serviceBusNamespace);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _serviceBusNamespaceRepository.Received(1).Update(Arg.Is<ServiceBusNamespace>(sb =>
            sb.TopicSubscriptions.Any(ts => ts.TopicName == TopicName && ts.SubscriptionName == SubscriptionName)));
        _mapper.Received(1).Map<ServiceBusNamespaceResult>(Arg.Any<ServiceBusNamespace>());
    }
}
