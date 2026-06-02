using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Commands.AddQueue;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using System.Reflection;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.StorageAccounts.Commands.AddQueue;

public sealed class AddQueueCommandHandlerTests
{
    private const string QueueName = "my-queue";

    private readonly IStorageAccountRepository _storageAccountRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly StorageAccount _storageAccount;
    private readonly AddQueueCommand _command;
    private readonly AddQueueCommandHandler _sut;

    public AddQueueCommandHandlerTests()
    {
        _storageAccountRepository = Substitute.For<IStorageAccountRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();

        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));

        _storageAccount = StorageAccount.Create(
            _resourceGroup.Id,
            new Name("stshared"),
            new Location(Location.LocationEnum.FranceCentral),
            new StorageAccountKind(StorageAccountKind.Kind.StorageV2),
            new StorageAccessTier(StorageAccessTier.Tier.Hot),
            allowBlobPublicAccess: false,
            enableHttpsTrafficOnly: true,
            new StorageAccountTlsVersion(StorageAccountTlsVersion.Version.Tls12));

        // Set the navigation property via reflection — EF normally hydrates this from an Include,
        // but in unit tests the aggregate is constructed in memory and ResourceGroup remains null.
        typeof(InfraFlowSculptor.Domain.Common.BaseModels.AzureResource)
            .GetProperty("ResourceGroup", BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(_storageAccount, _resourceGroup);

        _command = new AddQueueCommand(_storageAccount.Id, QueueName);
        _sut = new AddQueueCommandHandler(_storageAccountRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_StorageAccountNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _storageAccountRepository.GetByIdWithSubResourcesAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns((StorageAccount?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        _storageAccountRepository.GetByIdWithSubResourcesAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_storageAccount);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_AddsQueueAndMapsResultAsync()
    {
        // Arrange
        _storageAccountRepository.GetByIdWithSubResourcesAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_storageAccount);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _storageAccountRepository.AddQueue(Arg.Any<StorageQueue>())
            .Returns(callInfo => callInfo.Arg<StorageQueue>());
        _storageAccountRepository.GetByIdWithSubResourcesAsync(_storageAccount.Id, Arg.Any<CancellationToken>())
            .Returns(_storageAccount);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _storageAccountRepository.Received(1).AddQueue(Arg.Any<StorageQueue>());
        _mapper.Received(1).Map<StorageAccountResult>(Arg.Any<StorageAccount>());
    }
}
