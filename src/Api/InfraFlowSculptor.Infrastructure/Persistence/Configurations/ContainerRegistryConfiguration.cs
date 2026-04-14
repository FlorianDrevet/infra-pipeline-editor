using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.ContainerRegistryAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="ContainerRegistry"/> aggregate root (TPT).
/// </summary>
public sealed class ContainerRegistryConfiguration : IEntityTypeConfiguration<ContainerRegistry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ContainerRegistry> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("ContainerRegistries");

        builder.HasMany(x => x.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.ContainerRegistryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
