using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Commands.UpdateStorageAccount;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.StorageAccounts.Commands.UpdateStorageAccount;

public sealed class UpdateStorageAccountCommandHandlerTests
{
    private readonly IStorageAccountRepository _storageAccountRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly StorageAccount _existingEntity;
    private readonly UpdateStorageAccountCommand _command;
    private readonly UpdateStorageAccountCommandHandler _sut;

    public UpdateStorageAccountCommandHandlerTests()
    {
        _storageAccountRepository = Substitute.For<IStorageAccountRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _existingEntity = StorageAccount.Create(
            _resourceGroup.Id,
            new Name("stold"),
            new Location(Location.LocationEnum.FranceCentral),
            new StorageAccountKind(StorageAccountKind.Kind.StorageV2),
            new StorageAccessTier(StorageAccessTier.Tier.Hot),
            allowBlobPublicAccess: false,
            enableHttpsTrafficOnly: true,
            new StorageAccountTlsVersion(StorageAccountTlsVersion.Version.Tls12));
        _command = new UpdateStorageAccountCommand(
            _existingEntity.Id,
            new Name("strenamed"),
            new Location(Location.LocationEnum.WestEurope),
            Kind: nameof(StorageAccountKind.Kind.StorageV2),
            AccessTier: nameof(StorageAccessTier.Tier.Hot),
            AllowBlobPublicAccess: false,
            EnableHttpsTrafficOnly: true,
            MinimumTlsVersion: nameof(StorageAccountTlsVersion.Version.Tls12));
        _storageAccountRepository.Update(Arg.Any<StorageAccount>())
            .Returns(callInfo => (StorageAccount)callInfo.Args()[0]);
        _sut = new UpdateStorageAccountCommandHandler(
            _storageAccountRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_StorageAccountNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange — StorageAccountAccessHelper uses GetByIdWithSubResourcesAsync internally
        _storageAccountRepository.GetByIdWithSubResourcesAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns((StorageAccount?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _storageAccountRepository.DidNotReceive().Update(Arg.Any<StorageAccount>());
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _storageAccountRepository.GetByIdWithSubResourcesAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _storageAccountRepository.DidNotReceive().Update(Arg.Any<StorageAccount>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsUpdatedEntityAndMapsResultAsync()
    {
        // Arrange
        _storageAccountRepository.GetByIdWithSubResourcesAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _storageAccountRepository.Received(1).Update(Arg.Is<StorageAccount>(s =>
            s.Name.Value == "strenamed"));
        _mapper.Received(1).Map<StorageAccountResult>(Arg.Any<StorageAccount>());
    }
}
