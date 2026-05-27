using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerRegistries.Queries.CheckAcrPullAccess;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerRegistryAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAssignedIdentityAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.ContainerRegistries.Queries.CheckAcrPullAccess;

public sealed class CheckAcrPullAccessQueryHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly UserAssignedIdentity _identity;
    private readonly ContainerRegistry _registry;
    private readonly WebApp _sourceResource;
    private readonly CheckAcrPullAccessQuery _query;
    private readonly CheckAcrPullAccessQueryHandler _sut;

    public CheckAcrPullAccessQueryHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();

        var config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        var resourceGroup = DomainResourceGroup.Create(
            new Name("rg-rbac"),
            config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _identity = UserAssignedIdentity.Create(
            resourceGroup.Id,
            new Name("uai-acr"),
            new Location(Location.LocationEnum.FranceCentral));
        _registry = ContainerRegistry.Create(
            resourceGroup.Id,
            new Name("acrshared"),
            new Location(Location.LocationEnum.FranceCentral));
        _sourceResource = WebApp.Create(
            resourceGroup.Id,
            new Name("web-rbac"),
            new Location(Location.LocationEnum.FranceCentral),
            AzureResourceId.CreateUnique(),
            new WebAppRuntimeStack(WebAppRuntimeStack.WebAppRuntimeStackEnum.DotNet),
            "8.0",
            alwaysOn: true,
            httpsOnly: true,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Code),
            containerRegistryId: _registry.Id,
            acrAuthMode: new AcrAuthMode(AcrAuthMode.AcrAuthModeType.ManagedIdentity),
            dockerImageName: null);
        _sourceResource.AddRoleAssignment(
            _registry.Id,
            new ManagedIdentityType(ManagedIdentityType.IdentityTypeEnum.UserAssigned),
            AzureRoleDefinitionCatalog.AcrPull,
            _identity.Id);

        _query = new CheckAcrPullAccessQuery(
            _sourceResource.Id,
            _registry.Id,
            AcrAuthMode.AcrAuthModeType.ManagedIdentity.ToString());
        _sut = new CheckAcrPullAccessQueryHandler(_azureResourceRepository);
    }

    [Fact]
    public async Task Given_UserAssignedAcrPullAssignment_When_Handle_Then_UsesDetachedRoleAssignmentLookupsAsync()
    {
        // Arrange
        _azureResourceRepository.GetByIdWithRoleAssignmentsReadOnlyAsync(_sourceResource.Id, Arg.Any<CancellationToken>())
            .Returns(_sourceResource);
        _azureResourceRepository.GetByIdReadOnlyAsync(_identity.Id, Arg.Any<CancellationToken>())
            .Returns(_identity);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.HasAccess.Should().BeTrue();
        result.Value.AssignedUserAssignedIdentityName.Should().Be(_identity.Name.Value);
        await _azureResourceRepository.Received(1)
            .GetByIdWithRoleAssignmentsReadOnlyAsync(_sourceResource.Id, Arg.Any<CancellationToken>());
        await _azureResourceRepository.Received(1)
            .GetByIdReadOnlyAsync(_identity.Id, Arg.Any<CancellationToken>());
        await _azureResourceRepository.DidNotReceive()
            .GetByIdWithRoleAssignmentsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>());
        await _azureResourceRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_SpecificAcrPullIdentity_When_IdentityHasAcrPullRole_Then_ReturnsAccessWithSelectedIdentityAsync()
    {
        // Arrange
        var config = DomainInfrastructureConfig.Create(new Name("test-config"), ProjectId.CreateUnique());
        var rg = DomainResourceGroup.Create(
            new Name("rg-test"),
            config.Id,
            new Location(Location.LocationEnum.FranceCentral));

        var selectedUai = UserAssignedIdentity.Create(
            rg.Id,
            new Name("uai-selected"),
            new Location(Location.LocationEnum.FranceCentral));

        var testRegistry = ContainerRegistry.Create(
            rg.Id,
            new Name("acrtest"),
            new Location(Location.LocationEnum.FranceCentral));

        var testSourceResource = WebApp.Create(
            rg.Id,
            new Name("web-test"),
            new Location(Location.LocationEnum.FranceCentral),
            AzureResourceId.CreateUnique(),
            new WebAppRuntimeStack(WebAppRuntimeStack.WebAppRuntimeStackEnum.DotNet),
            "8.0",
            alwaysOn: true,
            httpsOnly: true,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Code),
            containerRegistryId: testRegistry.Id,
            acrAuthMode: new AcrAuthMode(AcrAuthMode.AcrAuthModeType.ManagedIdentity),
            dockerImageName: null);

        testSourceResource.AddRoleAssignment(
            testRegistry.Id,
            new ManagedIdentityType(ManagedIdentityType.IdentityTypeEnum.UserAssigned),
            AzureRoleDefinitionCatalog.AcrPull,
            selectedUai.Id);

        var queryWithSpecificIdentity = new CheckAcrPullAccessQuery(
            testSourceResource.Id,
            testRegistry.Id,
            AcrAuthMode.AcrAuthModeType.ManagedIdentity.ToString(),
            selectedUai.Id);

        _azureResourceRepository.GetByIdWithRoleAssignmentsReadOnlyAsync(testSourceResource.Id, Arg.Any<CancellationToken>())
            .Returns(testSourceResource);
        _azureResourceRepository.GetByIdReadOnlyAsync(selectedUai.Id, Arg.Any<CancellationToken>())
            .Returns(selectedUai);

        // Act
        var result = await _sut.Handle(queryWithSpecificIdentity, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.HasAccess.Should().BeTrue();
        result.Value.AssignedUserAssignedIdentityId.Should().Be(selectedUai.Id.Value.ToString());
        result.Value.AssignedUserAssignedIdentityName.Should().Be(selectedUai.Name.Value);
        result.Value.HasUserAssignedIdentity.Should().BeTrue();
    }

    [Fact]
    public async Task Given_SpecificAcrPullIdentity_When_IdentityMissingAcrPullRole_Then_ReturnsMissingAccessWithSelectedIdentityAsync()
    {
        // Arrange - Create a source resource that has a role assignment for _identity, but we'll check a different UAI
        var selectedUai = UserAssignedIdentity.Create(
            _identity.ResourceGroupId,
            new Name("uai-without-access"),
            new Location(Location.LocationEnum.FranceCentral));

        // _sourceResource from constructor already has AcrPull role for _identity, but NOT for selectedUai
        var queryWithSpecificIdentity = new CheckAcrPullAccessQuery(
            _sourceResource.Id,
            _registry.Id,
            AcrAuthMode.AcrAuthModeType.ManagedIdentity.ToString(),
            selectedUai.Id);

        _azureResourceRepository.GetByIdWithRoleAssignmentsReadOnlyAsync(_sourceResource.Id, Arg.Any<CancellationToken>())
            .Returns(_sourceResource);
        _azureResourceRepository.GetRoleAssignmentsByIdentityIdAsync(selectedUai.Id, Arg.Any<CancellationToken>())
            .Returns([]);
        _azureResourceRepository.GetByIdReadOnlyAsync(selectedUai.Id, Arg.Any<CancellationToken>())
            .Returns(selectedUai);

        // Act
        var result = await _sut.Handle(queryWithSpecificIdentity, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.HasAccess.Should().BeFalse();
        result.Value.MissingRoleDefinitionId.Should().Be(AzureRoleDefinitionCatalog.AcrPull);
        result.Value.MissingRoleName.Should().Be("AcrPull");
        result.Value.AssignedUserAssignedIdentityId.Should().Be(selectedUai.Id.Value.ToString());
        result.Value.AssignedUserAssignedIdentityName.Should().Be(selectedUai.Name.Value);
        result.Value.HasUserAssignedIdentity.Should().BeTrue();
    }

    [Fact]
    public async Task Given_AdminCredentials_When_AcrPullIdentitySpecified_Then_IgnoresIdentityAndReturnsAccessAsync()
    {
        // Arrange
        var selectedUai = UserAssignedIdentity.Create(
            _identity.ResourceGroupId,
            new Name("uai-ignored"),
            new Location(Location.LocationEnum.FranceCentral));

        var queryAdminWithIdentity = new CheckAcrPullAccessQuery(
            _sourceResource.Id,
            _registry.Id,
            AcrAuthMode.AcrAuthModeType.AdminCredentials.ToString(),
            selectedUai.Id);

        // Act
        var result = await _sut.Handle(queryAdminWithIdentity, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.HasAccess.Should().BeTrue();
        result.Value.AssignedUserAssignedIdentityId.Should().BeNull();
        result.Value.AssignedUserAssignedIdentityName.Should().BeNull();
        result.Value.HasUserAssignedIdentity.Should().BeFalse();
        result.Value.AcrAuthMode.Should().Be(AcrAuthMode.AcrAuthModeType.AdminCredentials.ToString());
    }

    [Fact]
    public async Task Given_NoAcrPullIdentity_When_Handle_Then_UsesBackwardCompatibleBehaviorAsync()
    {
        // Arrange - No AcrPullIdentityId specified, should find any UAI with AcrPull (backward compat)
        _azureResourceRepository.GetByIdWithRoleAssignmentsReadOnlyAsync(_sourceResource.Id, Arg.Any<CancellationToken>())
            .Returns(_sourceResource);
        _azureResourceRepository.GetByIdReadOnlyAsync(_identity.Id, Arg.Any<CancellationToken>())
            .Returns(_identity);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.HasAccess.Should().BeTrue();
        result.Value.AssignedUserAssignedIdentityId.Should().Be(_identity.Id.Value.ToString());
        result.Value.AssignedUserAssignedIdentityName.Should().Be(_identity.Name.Value);
    }
}
