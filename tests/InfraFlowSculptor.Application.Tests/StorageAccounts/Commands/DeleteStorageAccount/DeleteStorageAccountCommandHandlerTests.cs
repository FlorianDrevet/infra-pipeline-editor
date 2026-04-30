using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Commands.DeleteStorageAccount;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.StorageAccounts.Commands.DeleteStorageAccount;

public sealed class DeleteStorageAccountCommandHandlerTests
{
    private readonly IStorageAccountRepository _storageAccountRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly StorageAccount _storageAccount;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly DeleteStorageAccountCommand _command;
    private readonly DeleteStorageAccountCommandHandler _sut;

    public DeleteStorageAccountCommandHandlerTests()
    {
        _storageAccountRepository = Substitute.For<IStorageAccountRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
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
        _command = new DeleteStorageAccountCommand(_storageAccount.Id);
        _sut = new DeleteStorageAccountCommandHandler(
            _storageAccountRepository, _resourceGroupRepository, _accessService);
    }

    [Fact]
    public async Task Given_StorageAccountNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _storageAccountRepository
            .GetByIdWithSubResourcesAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns((StorageAccount?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _storageAccountRepository.DidNotReceive().DeleteAsync(Arg.Any<AzureResourceId>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_DeletesStorageAccountAsync()
    {
        // Arrange
        _storageAccountRepository
            .GetByIdWithSubResourcesAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_storageAccount);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        await _storageAccountRepository.Received(1).DeleteAsync(_storageAccount.Id);
    }
}
