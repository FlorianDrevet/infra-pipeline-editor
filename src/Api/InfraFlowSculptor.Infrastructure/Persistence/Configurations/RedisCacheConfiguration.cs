using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.RedisCacheAggregate;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public class RedisCacheConfiguration : IEntityTypeConfiguration<RedisCache>
{
    public void Configure(EntityTypeBuilder<RedisCache> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("RedisCaches");

        builder.Property(r => r.Sku)
            .IsRequired()
            .HasConversion(new EnumValueConverter<RedisCacheSku, RedisCacheSku.Sku>());

        builder.Property(r => r.Capacity)
            .IsRequired();

        builder.Property(r => r.RedisVersion)
            .IsRequired();

        builder.Property(r => r.EnableNonSslPort)
            .IsRequired();

        builder.Property(r => r.MinimumTlsVersion)
            .IsRequired()
            .HasConversion(new EnumValueConverter<TlsVersion, TlsVersion.Version>());

        builder.Property(r => r.MaxMemoryPolicy)
            .IsRequired()
            .HasConversion(new EnumValueConverter<MaxMemoryPolicy, MaxMemoryPolicy.Policy>());
    }
}
