using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.RedisCacheAggregate.Entities;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="RedisCacheEnvironmentSettings"/> entity.
/// </summary>
public sealed class RedisCacheEnvironmentSettingsConfiguration
    : IEntityTypeConfiguration<RedisCacheEnvironmentSettings>
{
    public void Configure(EntityTypeBuilder<RedisCacheEnvironmentSettings> builder)
    {
        builder.ToTable("RedisCacheEnvironmentSettings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<RedisCacheEnvironmentSettingsId>());

        builder.Property(x => x.RedisCacheId)
            .IsRequired()
            .HasConversion(new IdValueConverter<AzureResourceId>());

        builder.Property(x => x.EnvironmentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Sku)
            .IsRequired(false)
#pragma warning disable CS8620 // Nullability mismatch — EF Core handles null conversion internally
            .HasConversion(new EnumValueConverter<RedisCacheSku, RedisCacheSku.Sku>());
#pragma warning restore CS8620

        builder.Property(x => x.Capacity)
            .IsRequired(false);

        builder.Property(x => x.MaxMemoryPolicy)
            .IsRequired(false)
#pragma warning disable CS8620 // Nullability mismatch — EF Core handles null conversion internally
            .HasConversion(new EnumValueConverter<MaxMemoryPolicy, MaxMemoryPolicy.Policy>());
#pragma warning restore CS8620

        builder.HasIndex(x => new { x.RedisCacheId, x.EnvironmentName })
            .IsUnique();
    }
}