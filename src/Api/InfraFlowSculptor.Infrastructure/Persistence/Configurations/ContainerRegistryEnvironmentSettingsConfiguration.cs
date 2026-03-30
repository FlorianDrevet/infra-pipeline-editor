using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ContainerRegistryAggregate.Entities;
using InfraFlowSculptor.Domain.ContainerRegistryAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="ContainerRegistryEnvironmentSettings"/> entity.
/// </summary>
public sealed class ContainerRegistryEnvironmentSettingsConfiguration
    : IEntityTypeConfiguration<ContainerRegistryEnvironmentSettings>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ContainerRegistryEnvironmentSettings> builder)
    {
        builder.ToTable("ContainerRegistryEnvironmentSettings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<ContainerRegistryEnvironmentSettingsId>());

        builder.Property(x => x.ContainerRegistryId)
            .IsRequired()
            .HasConversion(new IdValueConverter<AzureResourceId>());

        builder.Property(x => x.EnvironmentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Sku)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(x => x.AdminUserEnabled)
            .IsRequired(false);

        builder.Property(x => x.PublicNetworkAccess)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(x => x.ZoneRedundancy)
            .IsRequired(false);

        builder.HasIndex(x => new { x.ContainerRegistryId, x.EnvironmentName })
            .IsUnique();
    }
}
