using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Commands.UpdateBlobContainerPublicAccess;
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

namespace InfraFlowSculptor.Application.Tests.StorageAccounts.Commands.UpdateBlobContainerPublicAccess;

public sealed class UpdateBlobContainerPublicAccessCommandHandlerTests
{
    private readonly IStorageAccountRepository _storageAccountRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly StorageAccount _existingEntity;
    private readonly BlobContainerId _containerId;
    private readonly UpdateBlobContainerPublicAccessCommand _command;
    private readonly UpdateBlobContainerPublicAccessCommandHandler _sut;

    public UpdateBlobContainerPublicAccessCommandHandlerTests()
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
            new Name("stblob"),
            new Location(Location.LocationEnum.FranceCentral),
            new StorageAccountKind(StorageAccountKind.Kind.StorageV2),
            new StorageAccessTier(StorageAccessTier.Tier.Hot),
            allowBlobPublicAccess: true,
            enableHttpsTrafficOnly: true,
            new StorageAccountTlsVersion(StorageAccountTlsVersion.Version.Tls12));

        // Add a blob container so the domain has something to update
        var addResult = _existingEntity.AddBlobContainer("test-container",
            new BlobContainerPublicAccess(BlobContainerPublicAccess.AccessLevel.None));
        _containerId = addResult.Value.Id;

        _command = new UpdateBlobContainerPublicAccessCommand(
            _existingEntity.Id,
            _containerId,
            new BlobContainerPublicAccess(BlobContainerPublicAccess.AccessLevel.Blob));
        _sut = new UpdateBlobContainerPublicAccessCommandHandler(
            _storageAccountRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_StorageAccountNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _storageAccountRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((StorageAccount?)null);

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
        _storageAccountRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ContainerNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange — use a non-existent container ID
        var badCommand = new UpdateBlobContainerPublicAccessCommand(
            _existingEntity.Id,
            new BlobContainerId(Guid.NewGuid()),
            new BlobContainerPublicAccess(BlobContainerPublicAccess.AccessLevel.Blob));
        _storageAccountRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(badCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_UpdatesPublicAccessAndMapsResultAsync()
    {
        // Arrange
        _storageAccountRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _storageAccountRepository.GetByIdWithSubResourcesAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_existingEntity);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _storageAccountRepository.Received(1).Update(Arg.Any<StorageAccount>());
        _mapper.Received(1).Map<StorageAccountResult>(Arg.Any<StorageAccount>());
    }
}
