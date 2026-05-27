using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RoleAssignments.Queries.ListRoleAssignmentsByIdentity;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAssignedIdentityAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.RoleAssignments.Queries.ListRoleAssignmentsByIdentity;

public sealed class ListRoleAssignmentsByIdentityQueryHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IUserAssignedIdentityRepository _userAssignedIdentityRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly DomainInfrastructureConfig _infrastructureConfig;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly UserAssignedIdentity _identity;
    private readonly WebApp _sourceResource;
    private readonly KeyVault _targetResource;
    private readonly RoleAssignment _roleAssignment;
    private readonly string _roleDefinitionId;
    private readonly string _roleDefinitionName;
    private readonly ListRoleAssignmentsByIdentityQuery _query;
    private readonly ListRoleAssignmentsByIdentityQueryHandler _sut;

    public ListRoleAssignmentsByIdentityQueryHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _userAssignedIdentityRepository = Substitute.For<IUserAssignedIdentityRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();

        _infrastructureConfig = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _infrastructureConfig.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _identity = UserAssignedIdentity.Create(
            _resourceGroup.Id,
            new Name("uai-shared"),
            new Location(Location.LocationEnum.FranceCentral));
        _sourceResource = WebApp.Create(
            _resourceGroup.Id,
            new Name("web-shared"),
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
            new Name("kv-shared"),
            new Location(Location.LocationEnum.FranceCentral));

        var roleDefinition = AzureRoleDefinitionCatalog
            .GetForResourceType(AzureResourceTypes.KeyVault)[0];
        _roleDefinitionId = roleDefinition.Id;
        _roleDefinitionName = roleDefinition.Name;

        _sourceResource.AddRoleAssignment(
            _targetResource.Id,
            new ManagedIdentityType(ManagedIdentityType.IdentityTypeEnum.UserAssigned),
            _roleDefinitionId,
            _identity.Id);
        _roleAssignment = _sourceResource.RoleAssignments.Single();

        _query = new ListRoleAssignmentsByIdentityQuery(_identity.Id);
        _sut = new ListRoleAssignmentsByIdentityQueryHandler(
            _azureResourceRepository,
            _userAssignedIdentityRepository,
            _resourceGroupRepository,
            _accessService);
    }

    [Fact]
    public async Task Given_IdentityDoesNotExist_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _userAssignedIdentityRepository.GetByIdReadOnlyAsync(Arg.Any<Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((UserAssignedIdentity?)null);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.UserAssignedIdentity.NotFoundError(_query.IdentityId).Code);
        await _userAssignedIdentityRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        ConfigureIdentityAccess();
        _accessService.VerifyReadAccessAsync(_resourceGroup.InfraConfigId, Arg.Any<CancellationToken>())
            .Returns(Error.NotFound(code: "InfraConfig.NotFound", description: "Missing config"));

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.UserAssignedIdentity.NotFoundError(_query.IdentityId).Code);
        await _userAssignedIdentityRepository.Received(1)
            .GetByIdReadOnlyAsync(Arg.Any<Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.Received(1)
            .GetByIdReadOnlyAsync(_resourceGroup.Id, Arg.Any<CancellationToken>());
        await _userAssignedIdentityRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_NoRoleAssignments_When_Handle_Then_ReturnsEmptyListAsync()
    {
        // Arrange
        ConfigureIdentityAccess();
        _azureResourceRepository.GetRoleAssignmentsByIdentityIdAsync(_identity.Id, Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        await _userAssignedIdentityRepository.Received(1)
            .GetByIdReadOnlyAsync(Arg.Any<Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.Received(1)
            .GetByIdReadOnlyAsync(_resourceGroup.Id, Arg.Any<CancellationToken>());
        await _userAssignedIdentityRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_RoleAssignmentsExist_When_Handle_Then_ReturnsEnrichedResultsAsync()
    {
        // Arrange
        ConfigureIdentityAccess();
        _azureResourceRepository.GetRoleAssignmentsByIdentityIdAsync(_identity.Id, Arg.Any<CancellationToken>())
            .Returns([_roleAssignment]);
        _azureResourceRepository.GetByIdReadOnlyAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var resourceId = callInfo.Arg<AzureResourceId>();
                if (resourceId == _sourceResource.Id)
                {
                    return _sourceResource;
                }

                if (resourceId == _targetResource.Id)
                {
                    return _targetResource;
                }

                return null;
            });

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Id = _roleAssignment.Id,
            SourceResourceId = _sourceResource.Id,
            SourceResourceName = _sourceResource.Name.Value,
            SourceResourceType = nameof(WebApp),
            TargetResourceId = _targetResource.Id,
            TargetResourceName = _targetResource.Name.Value,
            TargetResourceType = nameof(KeyVault),
            RoleDefinitionId = _roleDefinitionId,
            RoleName = _roleDefinitionName,
        });
        await _userAssignedIdentityRepository.Received(1)
            .GetByIdReadOnlyAsync(Arg.Any<Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.Received(1)
            .GetByIdReadOnlyAsync(_resourceGroup.Id, Arg.Any<CancellationToken>());
        await _azureResourceRepository.Received(2)
            .GetByIdReadOnlyAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>());
        await _userAssignedIdentityRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>());
        await _azureResourceRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>());
    }

    private void ConfigureIdentityAccess()
    {
        _userAssignedIdentityRepository.GetByIdReadOnlyAsync(Arg.Any<Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_identity);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_resourceGroup.Id, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_resourceGroup.InfraConfigId, Arg.Any<CancellationToken>())
            .Returns(_infrastructureConfig);
    }
}
