using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate.Entities;
using InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="KeyVaultEnvironmentSettings"/> entity.
/// </summary>
public sealed class KeyVaultEnvironmentSettingsConfiguration
    : IEntityTypeConfiguration<KeyVaultEnvironmentSettings>
{
    public void Configure(EntityTypeBuilder<KeyVaultEnvironmentSettings> builder)
    {
        builder.ToTable("KeyVaultEnvironmentSettings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<KeyVaultEnvironmentSettingsId>());

        builder.Property(x => x.KeyVaultId)
            .IsRequired()
            .HasConversion(new IdValueConverter<AzureResourceId>());

        builder.Property(x => x.EnvironmentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Sku)
            .IsRequired(false)
            .HasConversion(new EnumValueConverter<Sku, Sku.SkuEnum>());

        builder.HasIndex(x => new { x.KeyVaultId, x.EnvironmentName })
            .IsUnique();
    }
}