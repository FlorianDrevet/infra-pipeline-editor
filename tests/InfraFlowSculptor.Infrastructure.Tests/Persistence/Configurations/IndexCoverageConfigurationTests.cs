using FluentAssertions;
using InfraFlowSculptor.Domain.AppConfigurationAggregate.Entities;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Configurations;

public sealed class IndexCoverageConfigurationTests
{
    [Fact]
    public void Given_AzureResourceEntityModel_When_InspectingKeyIndexes_Then_HasResourceGroupAndResourceTypeIndexes()
    {
        // Arrange
        using var context = CreateRelationalContext();

        // Act
        var entityType = GetEntityType<AzureResource>(context);

        // Assert
        FindIndex(entityType, nameof(AzureResource.ResourceGroupId)).Should().NotBeNull();
        FindIndex(entityType, nameof(AzureResource.ResourceType)).Should().NotBeNull();
    }

    [Fact]
    public void Given_ResourceGroupAndInfrastructureConfigModels_When_InspectingForeignKeyIndexes_Then_HavePrimaryFilterCoverage()
    {
        // Arrange
        using var context = CreateRelationalContext();

        // Act
        var resourceGroupEntityType = GetEntityType<ResourceGroup>(context);
        var infrastructureConfigEntityType = GetEntityType<InfrastructureConfig>(context);

        // Assert
        FindIndex(resourceGroupEntityType, nameof(ResourceGroup.InfraConfigId)).Should().NotBeNull();
        FindIndex(infrastructureConfigEntityType, nameof(InfrastructureConfig.ProjectId)).Should().NotBeNull();
    }

    [Fact]
    public void Given_AppSettingAndAppConfigurationKeyModels_When_InspectingOptionalResourceReferenceIndexes_Then_ConventionIndexesExist()
    {
        // Arrange
        using var context = CreateRelationalContext();

        // Act
        var appSettingEntityType = GetEntityType<AppSetting>(context);
        var appConfigurationKeyEntityType = GetEntityType<AppConfigurationKey>(context);

        // Assert
        FindIndex(appSettingEntityType, nameof(AppSetting.SourceResourceId)).Should().NotBeNull();
        FindIndex(appSettingEntityType, nameof(AppSetting.KeyVaultResourceId)).Should().NotBeNull();
        FindIndex(appConfigurationKeyEntityType, nameof(AppConfigurationKey.SourceResourceId)).Should().NotBeNull();
        FindIndex(appConfigurationKeyEntityType, nameof(AppConfigurationKey.KeyVaultResourceId)).Should().NotBeNull();
    }

    [Fact]
    public void Given_RoleAssignmentEntityModel_When_InspectingSourceTargetLookupCoverage_Then_HasUniqueCompositeIndex()
    {
        // Arrange
        using var context = CreateRelationalContext();

        // Act
        var entityType = GetEntityType<RoleAssignment>(context);
        var index = FindIndex(
            entityType,
            nameof(RoleAssignment.SourceResourceId),
            nameof(RoleAssignment.TargetResourceId),
            nameof(RoleAssignment.UserAssignedIdentityId),
            nameof(RoleAssignment.RoleDefinitionId));

        // Assert
        index.Should().NotBeNull();
        index!.IsUnique.Should().BeTrue();
    }

    private static IReadOnlyEntityType GetEntityType<TEntity>(ProjectDbContext context)
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));
        entityType.Should().NotBeNull();
        return entityType!;
    }

    private static ProjectDbContext CreateRelationalContext()
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=index_coverage_tests;Username=test;Password=test")
            .Options;

        return new ProjectDbContext(options);
    }

    private static IReadOnlyIndex? FindIndex(IReadOnlyEntityType entityType, params string[] propertyNames)
    {
        return entityType.GetIndexes()
            .SingleOrDefault(index => index.Properties.Select(property => property.Name).SequenceEqual(propertyNames));
    }
}
