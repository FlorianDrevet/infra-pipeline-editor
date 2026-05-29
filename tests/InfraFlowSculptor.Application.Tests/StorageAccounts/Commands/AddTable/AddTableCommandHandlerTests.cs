using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Commands.AddTable;
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

namespace InfraFlowSculptor.Application.Tests.StorageAccounts.Commands.AddTable;

public sealed class AddTableCommandHandlerTests
{
    private const string TableName = "my-table";

    private readonly IStorageAccountRepository _storageAccountRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly StorageAccount _storageAccount;
    private readonly AddTableCommand _command;
    private readonly AddTableCommandHandler _sut;

    public AddTableCommandHandlerTests()
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
        _storageAccount = StorageAccount.Create(
            _resourceGroup.Id,
            new Name("stshared"),
            new Location(Location.LocationEnum.FranceCentral),
            new StorageAccountKind(StorageAccountKind.Kind.StorageV2),
            new StorageAccessTier(StorageAccessTier.Tier.Hot),
            allowBlobPublicAccess: false,
            enableHttpsTrafficOnly: true,
            new StorageAccountTlsVersion(StorageAccountTlsVersion.Version.Tls12));
        _command = new AddTableCommand(_storageAccount.Id, TableName);
        _sut = new AddTableCommandHandler(
            _storageAccountRepository, _accessService, _mapper);
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
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsUnauthorizedAsync()
    {
        // Arrange
        _storageAccountRepository.GetByIdWithSubResourcesAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_storageAccount);
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
    public async Task Given_WriteAccessGranted_When_Handle_Then_AddsTableAndMapsResultAsync()
    {
        // Arrange
        _storageAccountRepository.GetByIdWithSubResourcesAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_storageAccount);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _storageAccountRepository.AddTableAsync(Arg.Any<Domain.StorageAccountAggregate.Entities.StorageTable>())
            .Returns(callInfo => callInfo.Arg<Domain.StorageAccountAggregate.Entities.StorageTable>());
        _storageAccountRepository.GetByIdWithSubResourcesAsync(_storageAccount.Id, Arg.Any<CancellationToken>())
            .Returns(_storageAccount);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _mapper.Received(1).Map<StorageAccountResult>(Arg.Any<StorageAccount>());
    }
}
