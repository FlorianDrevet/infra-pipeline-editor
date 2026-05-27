using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Configurations;

public sealed class DeleteBehaviorConfigurationTests
{
    [Fact]
    public void Given_AppSettingEntityModel_When_InspectingOptionalResourceReferenceDeleteBehaviors_Then_UsesSetNull()
    {
        // Arrange
        using var context = CreateRelationalContext();
        var entityType = context.Model.FindEntityType(typeof(AppSetting));

        // Act
        var sourceResourceForeignKey = FindForeignKey(entityType, nameof(AppSetting.SourceResourceId));
        var keyVaultForeignKey = FindForeignKey(entityType, nameof(AppSetting.KeyVaultResourceId));

        // Assert
        sourceResourceForeignKey.Should().NotBeNull();
        sourceResourceForeignKey!.DeleteBehavior.Should().Be(DeleteBehavior.SetNull);
        keyVaultForeignKey.Should().NotBeNull();
        keyVaultForeignKey!.DeleteBehavior.Should().Be(DeleteBehavior.SetNull);
    }

    private static ProjectDbContext CreateRelationalContext()
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=delete_behavior_tests;Username=test;Password=test")
            .Options;

        return new ProjectDbContext(options);
    }

    private static IReadOnlyForeignKey? FindForeignKey(IReadOnlyEntityType? entityType, params string[] propertyNames)
    {
        entityType.Should().NotBeNull();

        return entityType!
            .GetForeignKeys()
            .SingleOrDefault(foreignKey => foreignKey.Properties.Select(property => property.Name).SequenceEqual(propertyNames));
    }
}
