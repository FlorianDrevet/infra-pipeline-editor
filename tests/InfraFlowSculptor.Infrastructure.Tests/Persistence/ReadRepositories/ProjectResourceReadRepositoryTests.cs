using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectResources;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Infrastructure.Persistence;
using InfraFlowSculptor.Infrastructure.Persistence.ReadRepositories;
using InfraFlowSculptor.Infrastructure.Tests.Persistence.Repositories;
using Xunit;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.ReadRepositories;

public sealed class ProjectResourceReadRepositoryTests : IDisposable
{
    private readonly ProjectDbContext _context;
    private readonly ProjectResourceReadRepository _sut;

    public ProjectResourceReadRepositoryTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _sut = new ProjectResourceReadRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Given_ProjectResourcesAcrossConfigs_When_GetByProjectIdAsync_Then_ReturnsFlatProjectedResourcesAsync()
    {
        // Arrange
        var projectId = ProjectId.CreateUnique();
        var otherProjectId = ProjectId.CreateUnique();
        var devConfig = InfrastructureConfig.Create(new Name("dev"), projectId);
        var prodConfig = InfrastructureConfig.Create(new Name("prod"), projectId);
        var otherConfig = InfrastructureConfig.Create(new Name("other"), otherProjectId);
        var devResourceGroup = ResourceGroup.Create(new Name("rg-dev"), devConfig.Id, new Location(Location.LocationEnum.WestEurope));
        var prodResourceGroup = ResourceGroup.Create(new Name("rg-prod"), prodConfig.Id, new Location(Location.LocationEnum.FranceCentral));
        var otherResourceGroup = ResourceGroup.Create(new Name("rg-other"), otherConfig.Id, new Location(Location.LocationEnum.WestEurope));
        var keyVault = KeyVault.Create(devResourceGroup.Id, new Name("kv-dev"), new Location(Location.LocationEnum.WestEurope));
        var containerApp = ContainerApp.Create(
            prodResourceGroup.Id,
            new Name("ca-prod"),
            new Location(Location.LocationEnum.FranceCentral),
            AzureResourceId.CreateUnique(),
            containerRegistryId: null,
            acrAuthMode: null);
        var unrelated = KeyVault.Create(otherResourceGroup.Id, new Name("kv-other"), new Location(Location.LocationEnum.WestEurope));
        _context.InfrastructureConfigs.AddRange(devConfig, prodConfig, otherConfig);
        _context.ResourceGroups.AddRange(devResourceGroup, prodResourceGroup, otherResourceGroup);
        _context.KeyVaults.AddRange(keyVault, unrelated);
        _context.ContainerApps.Add(containerApp);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByProjectIdAsync(projectId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(new[]
        {
            new ProjectResourceResult(
                keyVault.Id.Value,
                "kv-dev",
                "KeyVault",
                "rg-dev",
                devConfig.Id.Value,
                "dev"),
            new ProjectResourceResult(
                containerApp.Id.Value,
                "ca-prod",
                "ContainerApp",
                "rg-prod",
                prodConfig.Id.Value,
                "prod")
        });
    }
}