using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Commands.CreateStorageAccount;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.Errors;
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

namespace InfraFlowSculptor.Application.Tests.StorageAccounts.Commands.CreateStorageAccount;

public sealed class CreateStorageAccountCommandHandlerTests
{
    private const string StorageAccountName = "stshared";

    private readonly IStorageAccountRepository _storageAccountRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly CreateStorageAccountCommand _command;
    private readonly CreateStorageAccountCommandHandler _sut;

    public CreateStorageAccountCommandHandlerTests()
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
        _command = new CreateStorageAccountCommand(
            _resourceGroup.Id,
            new Name(StorageAccountName),
            new Location(Location.LocationEnum.FranceCentral),
            Kind: nameof(StorageAccountKind.Kind.StorageV2),
            AccessTier: nameof(StorageAccessTier.Tier.Hot),
            AllowBlobPublicAccess: false,
            EnableHttpsTrafficOnly: true,
            MinimumTlsVersion: nameof(StorageAccountTlsVersion.Version.Tls12));
        _storageAccountRepository.AddAsync(Arg.Any<StorageAccount>())
            .Returns(callInfo => Task.FromResult((StorageAccount)callInfo.Args()[0]));
        _sut = new CreateStorageAccountCommandHandler(
            _storageAccountRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _storageAccountRepository.DidNotReceive().AddAsync(Arg.Any<StorageAccount>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsStorageAccountAndMapsResultAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        await _storageAccountRepository.Received(1).AddAsync(Arg.Is<StorageAccount>(s =>
            s.ResourceGroupId == _resourceGroup.Id && s.Name.Value == StorageAccountName));
        _mapper.Received(1).Map<StorageAccountResult>(Arg.Any<StorageAccount>());
    }
}
