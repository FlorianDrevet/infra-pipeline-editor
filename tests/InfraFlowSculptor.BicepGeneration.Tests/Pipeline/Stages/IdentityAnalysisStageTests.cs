using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Pipeline.Stages;

public sealed class IdentityAnalysisStageTests
{
    private readonly IdentityAnalysisStage _sut = new();

    [Fact]
    public void Given_Stage_When_CheckOrder_Then_Returns100()
    {
        _sut.Order.Should().Be(100);
    }

    [Fact]
    public void Given_NoRoleAssignments_When_Execute_Then_IdentityResultIsEmpty()
    {
        // Arrange
        var context = CreateContext([], []);

        // Act
        _sut.Execute(context);

        // Assert
        context.Identity.SystemIdentityResources.Should().BeEmpty();
        context.Identity.UserIdentityResources.Should().BeEmpty();
        context.Identity.MixedIdentityArmTypes.Should().BeEmpty();
    }

    [Fact]
    public void Given_SystemAssignedRoleAssignment_When_Execute_Then_SystemIdentityResourcesPopulated()
    {
        // Arrange
        var resources = new[]
        {
            CreateResource("my-webapp", "Microsoft.Web/sites"),
        };
        var roleAssignments = new[]
        {
            CreateRoleAssignment("my-webapp", "Microsoft.Web/sites", "SystemAssigned"),
        };
        var context = CreateContext(resources, roleAssignments);

        // Act
        _sut.Execute(context);

        // Assert
        context.Identity.SystemIdentityResources.Should().Contain(("my-webapp", "Microsoft.Web/sites"));
        context.Identity.UserIdentityResources.Should().BeEmpty();
    }

    [Fact]
    public void Given_UserAssignedRoleAssignment_When_Execute_Then_UserIdentityResourcesPopulated()
    {
        // Arrange
        var resources = new[]
        {
            CreateResource("my-webapp", "Microsoft.Web/sites"),
        };
        var roleAssignments = new[]
        {
            CreateRoleAssignment("my-webapp", "Microsoft.Web/sites", "UserAssigned", "myUai"),
        };
        var context = CreateContext(resources, roleAssignments);

        // Act
        _sut.Execute(context);

        // Assert
        context.Identity.UserIdentityResources.Should().ContainKey(("my-webapp", "Microsoft.Web/sites"));
        context.Identity.SystemIdentityResources.Should().BeEmpty();
    }

    [Fact]
    public void Given_ResourceWithAssignedUaiButNoRoleAssignment_When_Execute_Then_UserIdentityIncluded()
    {
        // Arrange
        var resources = new[]
        {
            CreateResource("my-webapp", "Microsoft.Web/sites", assignedUaiName: "shared-identity"),
        };
        var context = CreateContext(resources, []);

        // Act
        _sut.Execute(context);

        // Assert
        context.Identity.UserIdentityResources.Should().ContainKey(("my-webapp", "Microsoft.Web/sites"));
    }

    [Fact]
    public void Given_SameArmTypeWithBothIdentityKinds_When_Execute_Then_MixedIdentityArmTypesPopulated()
    {
        // Arrange — two WebApps, one with system, one with user identity
        var resources = new[]
        {
            CreateResource("webapp-a", "Microsoft.Web/sites"),
            CreateResource("webapp-b", "Microsoft.Web/sites"),
        };
        var roleAssignments = new[]
        {
            CreateRoleAssignment("webapp-a", "Microsoft.Web/sites", "SystemAssigned"),
            CreateRoleAssignment("webapp-b", "Microsoft.Web/sites", "UserAssigned", "myUai"),
        };
        var context = CreateContext(resources, roleAssignments);

        // Act
        _sut.Execute(context);

        // Assert
        context.Identity.MixedIdentityArmTypes.Should().Contain("Microsoft.Web/sites");
    }

    [Fact]
    public void Given_SameArmTypeWithUniformIdentityKind_When_Execute_Then_MixedIdentityArmTypesEmpty()
    {
        // Arrange — two WebApps, both with system identity
        var resources = new[]
        {
            CreateResource("webapp-a", "Microsoft.Web/sites"),
            CreateResource("webapp-b", "Microsoft.Web/sites"),
        };
        var roleAssignments = new[]
        {
            CreateRoleAssignment("webapp-a", "Microsoft.Web/sites", "SystemAssigned"),
            CreateRoleAssignment("webapp-b", "Microsoft.Web/sites", "SystemAssigned"),
        };
        var context = CreateContext(resources, roleAssignments);

        // Act
        _sut.Execute(context);

        // Assert
        context.Identity.MixedIdentityArmTypes.Should().BeEmpty();
    }

    [Fact]
    public void Given_UserAssignedIdentityResource_When_Execute_Then_ExcludedFromMixedAnalysis()
    {
        // Arrange — UAI resource type is always excluded from identity kind analysis
        var resources = new[]
        {
            CreateResource("my-uai", "Microsoft.ManagedIdentity/userAssignedIdentities"),
        };
        var roleAssignments = new[]
        {
            CreateRoleAssignment("my-uai", "Microsoft.ManagedIdentity/userAssignedIdentities", "SystemAssigned"),
        };
        var context = CreateContext(resources, roleAssignments);

        // Act
        _sut.Execute(context);

        // Assert
        context.Identity.MixedIdentityArmTypes.Should().BeEmpty();
    }

    // ── Helpers ──

    private static BicepGenerationContext CreateContext(
        ResourceDefinition[] resources,
        RoleAssignmentDefinition[] roleAssignments) =>
        new()
        {
            Request = new GenerationRequest
            {
                Resources = resources,
                RoleAssignments = roleAssignments,
            },
        };

    private static ResourceDefinition CreateResource(
        string name, string type, string? assignedUaiName = null) =>
        new()
        {
            ResourceId = Guid.NewGuid(),
            Name = name,
            Type = type,
            AssignedUserAssignedIdentityName = assignedUaiName,
        };

    private static RoleAssignmentDefinition CreateRoleAssignment(
        string sourceName, string sourceType, string identityType, string? uaiName = null) =>
        new()
        {
            SourceResourceName = sourceName,
            SourceResourceType = sourceType,
            ManagedIdentityType = identityType,
            UserAssignedIdentityName = uaiName,
            TargetResourceName = "target",
            TargetResourceType = "Microsoft.KeyVault/vaults",
            RoleDefinitionId = "00000000-0000-0000-0000-000000000000",
        };
}
