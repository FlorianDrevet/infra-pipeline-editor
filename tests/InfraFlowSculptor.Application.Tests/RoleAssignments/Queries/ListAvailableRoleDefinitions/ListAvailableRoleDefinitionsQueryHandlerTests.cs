using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RoleAssignments.Queries.ListAvailableRoleDefinitions;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.RoleAssignments.Queries.ListAvailableRoleDefinitions;

public sealed class ListAvailableRoleDefinitionsQueryHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly KeyVault _resource;
    private readonly ListAvailableRoleDefinitionsQuery _query;
    private readonly ListAvailableRoleDefinitionsQueryHandler _sut;

    public ListAvailableRoleDefinitionsQueryHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();

        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-rbac"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _resource = KeyVault.Create(
            _resourceGroup.Id,
            new Name("kv-rbac"),
            new Location(Location.LocationEnum.FranceCentral));
        _query = new ListAvailableRoleDefinitionsQuery(_resource.Id);
        _sut = new ListAvailableRoleDefinitionsQueryHandler(
            _azureResourceRepository,
            _resourceGroupRepository,
            _accessService);
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsAuthErrorUsingReadOnlyLookupsAsync()
    {
        // Arrange
        _azureResourceRepository.GetByIdReadOnlyAsync(_resource.Id, Arg.Any<CancellationToken>())
            .Returns(_resource);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_resource.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Errors.InfrastructureConfig.ForbiddenError());

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.InfrastructureConfig.ForbiddenError().Code);
        await _azureResourceRepository.Received(1)
            .GetByIdReadOnlyAsync(_resource.Id, Arg.Any<CancellationToken>());
        await _resourceGroupRepository.Received(1)
            .GetByIdReadOnlyAsync(_resource.ResourceGroupId, Arg.Any<CancellationToken>());
        await _azureResourceRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<InfraFlowSculptor.Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ReadAccessGranted_When_Handle_Then_ReturnsRoleDefinitionsUsingReadOnlyLookupsAsync()
    {
        // Arrange
        var expectedRoles = AzureRoleDefinitionCatalog.GetForResourceType(nameof(KeyVault));
        _azureResourceRepository.GetByIdReadOnlyAsync(_resource.Id, Arg.Any<CancellationToken>())
            .Returns(_resource);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_resource.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(expectedRoles.Count);
        result.Value.Should().ContainSingle(role => role.Id == expectedRoles[0].Id && role.Name == expectedRoles[0].Name);
        await _azureResourceRepository.Received(1)
            .GetByIdReadOnlyAsync(_resource.Id, Arg.Any<CancellationToken>());
        await _resourceGroupRepository.Received(1)
            .GetByIdReadOnlyAsync(_resource.ResourceGroupId, Arg.Any<CancellationToken>());
        await _azureResourceRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<InfraFlowSculptor.Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>());
    }
}