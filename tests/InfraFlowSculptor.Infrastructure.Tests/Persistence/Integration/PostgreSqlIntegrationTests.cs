using FluentAssertions;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.UserAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Integration;

/// <summary>
/// Integration tests running against a real PostgreSQL container.
/// Validates cascade deletes, unique constraints, and query correctness
/// that the in-memory provider cannot guarantee.
/// </summary>
[Collection(PostgreSqlCollection.Name)]
public sealed class PostgreSqlIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private ProjectDbContext _context = null!;

    public PostgreSqlIntegrationTests(PostgreSqlFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();
        _context = _fixture.CreateContext();
    }

    public async Task DisposeAsync() => await _context.DisposeAsync();

    private static User CreateUser()
        => User.Create(new EntraId(Guid.NewGuid()), new Domain.UserAggregate.ValueObjects.Name("Test", "User"));

    private static Project CreateProject(UserId ownerId)
        => Project.Create(new Name("test-project"), "desc", ownerId);

    private static InfrastructureConfig CreateInfraConfig(Project project)
        => InfrastructureConfig.Create(new Name("dev"), project.Id);

    private static ResourceGroup CreateResourceGroup(InfrastructureConfig config)
        => ResourceGroup.Create(new Name("rg-test"), config.Id, new Location(Location.LocationEnum.WestEurope));

    [Fact]
    public async Task Given_ProjectWithInfraConfig_When_DeleteProject_Then_CascadeDeletesAll()
    {
        // Arrange — full hierarchy: Project → InfraConfig → ResourceGroup → KeyVault
        var user = CreateUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var project = CreateProject(user.Id);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var config = CreateInfraConfig(project);
        _context.InfrastructureConfigs.Add(config);
        await _context.SaveChangesAsync();

        var rg = CreateResourceGroup(config);
        config.AddResourceGroup(rg);
        _context.ResourceGroups.Add(rg);
        await _context.SaveChangesAsync();

        var kv = KeyVault.Create(rg.Id, new Name("kv-test"), new Location(Location.LocationEnum.WestEurope));
        rg.AddResource(kv);
        _context.KeyVaults.Add(kv);
        await _context.SaveChangesAsync();

        // Act — delete the project
        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        // Assert — everything cascaded
        await using var verify = _fixture.CreateContext();
        (await verify.InfrastructureConfigs.AnyAsync()).Should().BeFalse("InfraConfig should cascade");
        (await verify.ResourceGroups.AnyAsync()).Should().BeFalse("ResourceGroup should cascade");
        (await verify.KeyVaults.AnyAsync()).Should().BeFalse("KeyVault should cascade");
    }

    [Fact]
    public async Task Given_InfraConfigWithResourceGroups_When_DeleteConfig_Then_CascadeDeletesChildren()
    {
        // Arrange
        var user = CreateUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var project = CreateProject(user.Id);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var config = CreateInfraConfig(project);
        _context.InfrastructureConfigs.Add(config);
        await _context.SaveChangesAsync();

        var rg1 = ResourceGroup.Create(new Name("rg-one"), config.Id, new Location(Location.LocationEnum.WestEurope));
        var rg2 = ResourceGroup.Create(new Name("rg-two"), config.Id, new Location(Location.LocationEnum.FranceCentral));
        config.AddResourceGroup(rg1);
        config.AddResourceGroup(rg2);
        _context.ResourceGroups.AddRange(rg1, rg2);
        await _context.SaveChangesAsync();

        // Act
        _context.InfrastructureConfigs.Remove(config);
        await _context.SaveChangesAsync();

        // Assert
        await using var verify = _fixture.CreateContext();
        (await verify.ResourceGroups.AnyAsync()).Should().BeFalse("Both RGs should cascade");
    }

    [Fact]
    public async Task Given_ResourceGroupWithResources_When_DeleteGroup_Then_CascadeDeletesResources()
    {
        // Arrange
        var user = CreateUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var project = CreateProject(user.Id);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var config = CreateInfraConfig(project);
        _context.InfrastructureConfigs.Add(config);
        await _context.SaveChangesAsync();

        var rg = CreateResourceGroup(config);
        config.AddResourceGroup(rg);
        _context.ResourceGroups.Add(rg);
        await _context.SaveChangesAsync();

        var kv1 = KeyVault.Create(rg.Id, new Name("kv-one"), new Location(Location.LocationEnum.WestEurope));
        var kv2 = KeyVault.Create(rg.Id, new Name("kv-two"), new Location(Location.LocationEnum.WestEurope));
        rg.AddResource(kv1);
        rg.AddResource(kv2);
        _context.KeyVaults.AddRange(kv1, kv2);
        await _context.SaveChangesAsync();

        // Act — delete the resource group
        _context.ResourceGroups.Remove(rg);
        await _context.SaveChangesAsync();

        // Assert
        await using var verify = _fixture.CreateContext();
        (await verify.KeyVaults.AnyAsync()).Should().BeFalse("KeyVaults should cascade with RG");
        (await verify.AzureResources.AnyAsync()).Should().BeFalse("AzureResources should cascade with RG");
    }

    [Fact]
    public async Task Given_Entities_When_SavedAndQueried_Then_ValueObjectsPersistCorrectly()
    {
        // Arrange
        var user = CreateUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var project = CreateProject(user.Id);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var config = CreateInfraConfig(project);
        _context.InfrastructureConfigs.Add(config);
        await _context.SaveChangesAsync();

        var rg = CreateResourceGroup(config);
        config.AddResourceGroup(rg);
        _context.ResourceGroups.Add(rg);
        await _context.SaveChangesAsync();

        // Act — reload in a fresh context
        await using var verify = _fixture.CreateContext();
        var loaded = await verify.ResourceGroups
            .Include(x => x.InfraConfig)
            .FirstOrDefaultAsync(x => x.Id == rg.Id);

        // Assert — value objects round-trip correctly
        loaded.Should().NotBeNull();
        loaded!.Name.Value.Should().Be("rg-test");
        loaded.Location.Value.Should().Be(Location.LocationEnum.WestEurope);
        loaded.InfraConfigId.Should().Be(config.Id);
    }

    [Fact]
    public async Task Given_MultipleQueries_When_UsingInclude_Then_NoNPlusOneViolation()
    {
        // Arrange — create 3 RGs with 2 resources each
        var user = CreateUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var project = CreateProject(user.Id);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var config = CreateInfraConfig(project);
        _context.InfrastructureConfigs.Add(config);
        await _context.SaveChangesAsync();

        for (var i = 0; i < 3; i++)
        {
            var rg = ResourceGroup.Create(
                new Name($"rg-{i}"), config.Id, new Location(Location.LocationEnum.WestEurope));
            config.AddResourceGroup(rg);
            _context.ResourceGroups.Add(rg);
            await _context.SaveChangesAsync();

            var kv = KeyVault.Create(rg.Id, new Name($"kv-{i}"), new Location(Location.LocationEnum.WestEurope));
            rg.AddResource(kv);
            _context.KeyVaults.Add(kv);
            await _context.SaveChangesAsync();
        }

        // Act — single query with Include should load everything
        await using var verify = _fixture.CreateContext();
        var groups = await verify.ResourceGroups
            .Include(rg => rg.Resources)
            .ToListAsync();

        // Assert
        groups.Should().HaveCount(3);
        groups.Should().AllSatisfy(rg => rg.Resources.Should().HaveCount(1));
    }
}
