using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="ContainerAppEnvironment"/> aggregate root (TPT).
/// </summary>
public sealed class ContainerAppEnvironmentConfiguration : IEntityTypeConfiguration<ContainerAppEnvironment>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ContainerAppEnvironment> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("ContainerAppEnvironments");

        builder.Property(x => x.LogAnalyticsWorkspaceId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired(false);

        builder.HasMany(x => x.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.ContainerAppEnvironmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
