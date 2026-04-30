using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.KeyVaults.Commands.UpdateKeyVault;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.KeyVaults.Commands.UpdateKeyVault;

public sealed class UpdateKeyVaultCommandHandlerTests
{
    private readonly IKeyVaultRepository _keyVaultRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly KeyVault _existingKeyVault;
    private readonly UpdateKeyVaultCommand _command;
    private readonly UpdateKeyVaultCommandHandler _sut;

    public UpdateKeyVaultCommandHandlerTests()
    {
        _keyVaultRepository = Substitute.For<IKeyVaultRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _existingKeyVault = KeyVault.Create(
            _resourceGroup.Id,
            new Name("kv-shared"),
            new Location(Location.LocationEnum.FranceCentral));
        _command = new UpdateKeyVaultCommand(
            _existingKeyVault.Id,
            new Name("kv-renamed"),
            new Location(Location.LocationEnum.WestEurope),
            EnableRbacAuthorization: false,
            EnabledForDeployment: true,
            EnabledForDiskEncryption: true,
            EnabledForTemplateDeployment: true,
            EnablePurgeProtection: false,
            EnableSoftDelete: false);
        _keyVaultRepository.UpdateAsync(Arg.Any<KeyVault>())
            .Returns(callInfo => Task.FromResult((KeyVault)callInfo.Args()[0]));
        _sut = new UpdateKeyVaultCommandHandler(
            _keyVaultRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_KeyVaultNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _keyVaultRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((KeyVault?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _keyVaultRepository.DidNotReceive().UpdateAsync(Arg.Any<KeyVault>());
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _keyVaultRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingKeyVault);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _keyVaultRepository.DidNotReceive().UpdateAsync(Arg.Any<KeyVault>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsUpdatedKeyVaultAndMapsResultAsync()
    {
        // Arrange
        _keyVaultRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_existingKeyVault);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        await _keyVaultRepository.Received(1).UpdateAsync(Arg.Is<KeyVault>(kv =>
            kv.Name.Value == "kv-renamed"
            && kv.EnableRbacAuthorization == false
            && kv.EnabledForDeployment == true));
        _mapper.Received(1).Map<KeyVaultResult>(Arg.Any<KeyVault>());
    }
}
