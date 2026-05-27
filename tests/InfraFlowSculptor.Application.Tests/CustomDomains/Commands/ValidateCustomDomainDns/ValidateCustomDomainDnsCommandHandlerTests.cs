using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.CustomDomains.Commands.ValidateCustomDomainDns;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.CustomDomains.Commands.ValidateCustomDomainDns;

public sealed class ValidateCustomDomainDnsCommandHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly ValidateCustomDomainDnsCommandHandler _sut;

    public ValidateCustomDomainDnsCommandHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _sut = new ValidateCustomDomainDnsCommandHandler(
            _azureResourceRepository, _resourceGroupRepository, _accessService);
    }

    [Fact]
    public async Task Given_ResourceNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        var command = new ValidateCustomDomainDnsCommand(
            AzureResourceId.CreateUnique(), CustomDomainId.CreateUnique());
        _azureResourceRepository.GetByIdWithCustomDomainsAsync(command.ResourceId, Arg.Any<CancellationToken>())
            .Returns((AzureResource?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        var resource = KeyVault.Create(
            _resourceGroup.Id,
            new Name("kv-test"),
            new Location(Location.LocationEnum.FranceCentral));
        var command = new ValidateCustomDomainDnsCommand(resource.Id, CustomDomainId.CreateUnique());

        _azureResourceRepository.GetByIdWithCustomDomainsAsync(resource.Id, Arg.Any<CancellationToken>())
            .Returns(resource);
        _resourceGroupRepository.GetByIdAsync(resource.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsForbiddenAsync()
    {
        // Arrange
        var resource = KeyVault.Create(
            _resourceGroup.Id,
            new Name("kv-test"),
            new Location(Location.LocationEnum.FranceCentral));
        var command = new ValidateCustomDomainDnsCommand(resource.Id, CustomDomainId.CreateUnique());

        _azureResourceRepository.GetByIdWithCustomDomainsAsync(resource.Id, Arg.Any<CancellationToken>())
            .Returns(resource);
        _resourceGroupRepository.GetByIdAsync(resource.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Errors.InfrastructureConfig.ForbiddenError());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Given_CustomDomainNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        var resource = KeyVault.Create(
            _resourceGroup.Id,
            new Name("kv-test"),
            new Location(Location.LocationEnum.FranceCentral));
        var command = new ValidateCustomDomainDnsCommand(resource.Id, CustomDomainId.CreateUnique());

        _azureResourceRepository.GetByIdWithCustomDomainsAsync(resource.Id, Arg.Any<CancellationToken>())
            .Returns(resource);
        _resourceGroupRepository.GetByIdAsync(resource.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_ValidatesDnsAndReturnsResultAsync()
    {
        // Arrange
        var resource = KeyVault.Create(
            _resourceGroup.Id,
            new Name("kv-test"),
            new Location(Location.LocationEnum.FranceCentral));
        var addResult = resource.AddCustomDomain("production", "api.example.com");
        addResult.IsError.Should().BeFalse();
        var customDomain = addResult.Value;

        var command = new ValidateCustomDomainDnsCommand(resource.Id, customDomain.Id);

        _azureResourceRepository.GetByIdWithCustomDomainsAsync(resource.Id, Arg.Any<CancellationToken>())
            .Returns(resource);
        _resourceGroupRepository.GetByIdAsync(resource.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.DnsValidationStatus.Should().Be("Validated");
        result.Value.DomainName.Should().Be("api.example.com");
        _azureResourceRepository.Received(1).Update(resource);
    }
}
