using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RoleAssignments.Queries.ListRoleAssignments;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAssignedIdentityAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.RoleAssignments.Queries.ListRoleAssignments;

public sealed class ListRoleAssignmentsQueryHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly UserAssignedIdentity _identity;
    private readonly WebApp _sourceResource;
    private readonly KeyVault _targetResource;
    private readonly ListRoleAssignmentsQuery _query;
    private readonly ListRoleAssignmentsQueryHandler _sut;

    public ListRoleAssignmentsQueryHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();

        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-rbac"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _identity = UserAssignedIdentity.Create(
            _resourceGroup.Id,
            new Name("uai-rbac"),
            new Location(Location.LocationEnum.FranceCentral));
        _sourceResource = WebApp.Create(
            _resourceGroup.Id,
            new Name("web-rbac"),
            new Location(Location.LocationEnum.FranceCentral),
            AzureResourceId.CreateUnique(),
            new WebAppRuntimeStack(WebAppRuntimeStack.WebAppRuntimeStackEnum.DotNet),
            "8.0",
            alwaysOn: true,
            httpsOnly: true,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Code),
            containerRegistryId: null,
            acrAuthMode: null,
            dockerImageName: null);
        _targetResource = KeyVault.Create(
            _resourceGroup.Id,
            new Name("kv-rbac"),
            new Location(Location.LocationEnum.FranceCentral));

        var roleDefinition = AzureRoleDefinitionCatalog.GetForResourceType(nameof(KeyVault))[0];
        _sourceResource.AddRoleAssignment(
            _targetResource.Id,
            new ManagedIdentityType(ManagedIdentityType.IdentityTypeEnum.UserAssigned),
            roleDefinition.Id,
            _identity.Id);
        _sourceResource.AssignUserAssignedIdentity(_identity.Id);

        _query = new ListRoleAssignmentsQuery(_sourceResource.Id);
        _sut = new ListRoleAssignmentsQueryHandler(
            _azureResourceRepository,
            _resourceGroupRepository,
            _accessService);
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsAuthErrorUsingReadOnlyLookupsAsync()
    {
        // Arrange
        _azureResourceRepository.GetByIdWithRoleAssignmentsReadOnlyAsync(_sourceResource.Id, Arg.Any<CancellationToken>())
            .Returns(_sourceResource);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_sourceResource.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(InfraFlowSculptor.Domain.Common.Errors.Errors.InfrastructureConfig.ForbiddenError());

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(InfraFlowSculptor.Domain.Common.Errors.Errors.InfrastructureConfig.ForbiddenError().Code);
        await _azureResourceRepository.Received(1)
            .GetByIdWithRoleAssignmentsReadOnlyAsync(_sourceResource.Id, Arg.Any<CancellationToken>());
        await _resourceGroupRepository.Received(1)
            .GetByIdReadOnlyAsync(_sourceResource.ResourceGroupId, Arg.Any<CancellationToken>());
        await _azureResourceRepository.DidNotReceive()
            .GetByIdWithRoleAssignmentsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<InfraFlowSculptor.Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ReadAccessGranted_When_Handle_Then_ReturnsAssignmentsAndAssignedIdentityUsingReadOnlyLookupsAsync()
    {
        // Arrange
        _azureResourceRepository.GetByIdWithRoleAssignmentsReadOnlyAsync(_sourceResource.Id, Arg.Any<CancellationToken>())
            .Returns(_sourceResource);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_sourceResource.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _azureResourceRepository.GetByIdReadOnlyAsync(_identity.Id, Arg.Any<CancellationToken>())
            .Returns(_identity);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.AssignedUserAssignedIdentityId.Should().Be(_identity.Id.Value.ToString());
        result.Value.AssignedUserAssignedIdentityName.Should().Be(_identity.Name.Value);
        result.Value.RoleAssignments.Should().ContainSingle().Which.TargetResourceId.Should().Be(_targetResource.Id);
        await _azureResourceRepository.Received(1)
            .GetByIdWithRoleAssignmentsReadOnlyAsync(_sourceResource.Id, Arg.Any<CancellationToken>());
        await _azureResourceRepository.Received(1)
            .GetByIdReadOnlyAsync(_identity.Id, Arg.Any<CancellationToken>());
        await _resourceGroupRepository.Received(1)
            .GetByIdReadOnlyAsync(_sourceResource.ResourceGroupId, Arg.Any<CancellationToken>());
        await _azureResourceRepository.DidNotReceive()
            .GetByIdWithRoleAssignmentsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>());
        await _azureResourceRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<InfraFlowSculptor.Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>());
    }
}
