using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate.Entities;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="ContainerAppEnvironmentEnvironmentSettings"/> entity.
/// </summary>
public sealed class ContainerAppEnvironmentEnvironmentSettingsConfiguration
    : IEntityTypeConfiguration<ContainerAppEnvironmentEnvironmentSettings>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ContainerAppEnvironmentEnvironmentSettings> builder)
    {
        builder.ToTable("ContainerAppEnvironmentEnvironmentSettings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<ContainerAppEnvironmentEnvironmentSettingsId>());

        builder.Property(x => x.ContainerAppEnvironmentId)
            .IsRequired()
            .HasConversion(new IdValueConverter<AzureResourceId>());

        builder.Property(x => x.EnvironmentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Sku)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(x => x.WorkloadProfileType)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(x => x.InternalLoadBalancerEnabled)
            .IsRequired(false);

        builder.Property(x => x.ZoneRedundancyEnabled)
            .IsRequired(false);

        builder.Property(x => x.LogAnalyticsWorkspaceId)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.HasIndex(x => new { x.ContainerAppEnvironmentId, x.EnvironmentName })
            .IsUnique();
    }
}
