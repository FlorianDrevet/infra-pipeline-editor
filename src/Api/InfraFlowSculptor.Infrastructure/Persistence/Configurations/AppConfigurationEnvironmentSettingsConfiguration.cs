using InfraFlowSculptor.Domain.AppConfigurationAggregate.Entities;
using InfraFlowSculptor.Domain.AppConfigurationAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="AppConfigurationEnvironmentSettings"/> entity.
/// </summary>
public sealed class AppConfigurationEnvironmentSettingsConfiguration
    : IEntityTypeConfiguration<AppConfigurationEnvironmentSettings>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AppConfigurationEnvironmentSettings> builder)
    {
        builder.ToTable("AppConfigurationEnvironmentSettings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<AppConfigurationEnvironmentSettingsId>());

        builder.Property(x => x.AppConfigurationId)
            .IsRequired()
            .HasConversion(new IdValueConverter<AzureResourceId>());

        builder.Property(x => x.EnvironmentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Sku)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(x => x.SoftDeleteRetentionInDays)
            .IsRequired(false);

        builder.Property(x => x.PurgeProtectionEnabled)
            .IsRequired(false);

        builder.Property(x => x.DisableLocalAuth)
            .IsRequired(false);

        builder.Property(x => x.PublicNetworkAccess)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.HasIndex(x => new { x.AppConfigurationId, x.EnvironmentName })
            .IsUnique();
    }
}
