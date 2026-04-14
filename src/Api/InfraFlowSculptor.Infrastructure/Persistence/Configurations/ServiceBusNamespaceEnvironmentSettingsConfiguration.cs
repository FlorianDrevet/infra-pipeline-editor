using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.Entities;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="ServiceBusNamespaceEnvironmentSettings"/> entity.
/// </summary>
public sealed class ServiceBusNamespaceEnvironmentSettingsConfiguration
    : IEntityTypeConfiguration<ServiceBusNamespaceEnvironmentSettings>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ServiceBusNamespaceEnvironmentSettings> builder)
    {
        builder.ToTable("ServiceBusNamespaceEnvironmentSettings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<ServiceBusNamespaceEnvironmentSettingsId>());

        builder.Property(x => x.ServiceBusNamespaceId)
            .IsRequired()
            .HasConversion(new IdValueConverter<AzureResourceId>());

        builder.Property(x => x.EnvironmentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Sku)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(x => x.Capacity)
            .IsRequired(false);

        builder.Property(x => x.ZoneRedundant)
            .IsRequired(false);

        builder.Property(x => x.DisableLocalAuth)
            .IsRequired(false);

        builder.Property(x => x.MinimumTlsVersion)
            .IsRequired(false)
            .HasMaxLength(10);

        builder.HasIndex(x => new { x.ServiceBusNamespaceId, x.EnvironmentName })
            .IsUnique();
    }
}
