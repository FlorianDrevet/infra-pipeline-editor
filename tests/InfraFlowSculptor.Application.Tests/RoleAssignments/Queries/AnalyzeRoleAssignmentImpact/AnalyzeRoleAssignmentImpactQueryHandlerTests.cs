using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Application.RoleAssignments.Queries.AnalyzeRoleAssignmentImpact;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.RoleAssignments.Queries.AnalyzeRoleAssignmentImpact;

public sealed class AnalyzeRoleAssignmentImpactQueryHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IRoleAssignmentImpactAnalyzer _impactAnalyzer;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly WebApp _sourceResource;
    private readonly AnalyzeRoleAssignmentImpactQuery _query;
    private readonly AnalyzeRoleAssignmentImpactQueryHandler _sut;

    public AnalyzeRoleAssignmentImpactQueryHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _impactAnalyzer = Substitute.For<IRoleAssignmentImpactAnalyzer>();

        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-rbac"),
            _config.Id,
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

        var targetResource = KeyVault.Create(
            _resourceGroup.Id,
            new Name("kv-rbac"),
            new Location(Location.LocationEnum.FranceCentral));
        var roleDefinition = AzureRoleDefinitionCatalog.GetForResourceType(nameof(KeyVault))[0];
        _sourceResource.AddRoleAssignment(
            targetResource.Id,
            new ManagedIdentityType(ManagedIdentityType.IdentityTypeEnum.SystemAssigned),
            roleDefinition.Id);

        var roleAssignment = _sourceResource.RoleAssignments.Single();
        _query = new AnalyzeRoleAssignmentImpactQuery(_sourceResource.Id, roleAssignment.Id);
        _sut = new AnalyzeRoleAssignmentImpactQueryHandler(
            _azureResourceRepository,
            _resourceGroupRepository,
            _accessService,
            _impactAnalyzer);
    }

    [Fact]
    public async Task Given_ReadAccessDenied_When_Handle_Then_ReturnsAuthErrorUsingReadOnlyLookupsAsync()
    {
        // Arrange
        _azureResourceRepository
            .GetByIdWithRoleAssignmentsAndAppSettingsReadOnlyAsync(_sourceResource.Id, Arg.Any<CancellationToken>())
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
            .GetByIdWithRoleAssignmentsAndAppSettingsReadOnlyAsync(_sourceResource.Id, Arg.Any<CancellationToken>());
        await _resourceGroupRepository.Received(1)
            .GetByIdReadOnlyAsync(_sourceResource.ResourceGroupId, Arg.Any<CancellationToken>());
        await _impactAnalyzer.DidNotReceive()
            .AnalyzeAsync(Arg.Any<InfraFlowSculptor.Domain.Common.BaseModels.AzureResource>(), Arg.Any<InfraFlowSculptor.Domain.Common.BaseModels.Entites.RoleAssignment>(), Arg.Any<CancellationToken>());
        await _azureResourceRepository.DidNotReceive()
            .GetByIdWithRoleAssignmentsAndAppSettingsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<InfraFlowSculptor.Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ReadAccessGranted_When_Handle_Then_AnalyzesImpactUsingReadOnlyLookupsAsync()
    {
        // Arrange
        var roleAssignment = _sourceResource.RoleAssignments.Single();
        var expectedImpact = new RoleAssignmentImpactItem(
            "LastRoleToTarget",
            _sourceResource.Id.Value,
            _sourceResource.Name.Value,
            nameof(WebApp),
            roleAssignment.TargetResourceId.Value,
            "kv-rbac",
            nameof(KeyVault),
            "Impact detected",
            "Critical");

        _azureResourceRepository
            .GetByIdWithRoleAssignmentsAndAppSettingsReadOnlyAsync(_sourceResource.Id, Arg.Any<CancellationToken>())
            .Returns(_sourceResource);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_sourceResource.ResourceGroupId, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _impactAnalyzer.AnalyzeAsync(_sourceResource, roleAssignment, Arg.Any<CancellationToken>())
            .Returns([expectedImpact]);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.HasImpact.Should().BeTrue();
        result.Value.Impacts.Should().ContainSingle().Which.Should().Be(expectedImpact);
        await _azureResourceRepository.Received(1)
            .GetByIdWithRoleAssignmentsAndAppSettingsReadOnlyAsync(_sourceResource.Id, Arg.Any<CancellationToken>());
        await _resourceGroupRepository.Received(1)
            .GetByIdReadOnlyAsync(_sourceResource.ResourceGroupId, Arg.Any<CancellationToken>());
        await _impactAnalyzer.Received(1)
            .AnalyzeAsync(_sourceResource, roleAssignment, Arg.Any<CancellationToken>());
        await _azureResourceRepository.DidNotReceive()
            .GetByIdWithRoleAssignmentsAndAppSettingsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<InfraFlowSculptor.Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>());
    }
}