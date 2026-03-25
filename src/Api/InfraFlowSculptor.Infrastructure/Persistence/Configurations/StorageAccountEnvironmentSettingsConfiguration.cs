using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="StorageAccountEnvironmentSettings"/> entity.
/// </summary>
public sealed class StorageAccountEnvironmentSettingsConfiguration
    : IEntityTypeConfiguration<StorageAccountEnvironmentSettings>
{
    public void Configure(EntityTypeBuilder<StorageAccountEnvironmentSettings> builder)
    {
        builder.ToTable("StorageAccountEnvironmentSettings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<StorageAccountEnvironmentSettingsId>());

        builder.Property(x => x.StorageAccountId)
            .IsRequired()
            .HasConversion(new IdValueConverter<AzureResourceId>());

        builder.Property(x => x.EnvironmentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Sku)
            .IsRequired(false)
            .HasConversion(new EnumValueConverter<StorageAccountSku, StorageAccountSku.Sku>());

        builder.HasIndex(x => new { x.StorageAccountId, x.EnvironmentName })
            .IsUnique();
    }
}