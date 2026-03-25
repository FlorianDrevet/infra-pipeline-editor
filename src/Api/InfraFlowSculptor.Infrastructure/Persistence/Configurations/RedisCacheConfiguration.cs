using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.RedisCacheAggregate;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public class RedisCacheConfiguration : IEntityTypeConfiguration<RedisCache>
{
    public void Configure(EntityTypeBuilder<RedisCache> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("RedisCaches");

        builder.Property(rc => rc.RedisVersion)
            .IsRequired(false);

        builder.Property(rc => rc.EnableNonSslPort)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(rc => rc.MinimumTlsVersion)
            .IsRequired(false)
            .HasConversion(new EnumValueConverter<TlsVersion, TlsVersion.Version>());

        builder.Property(rc => rc.DisableAccessKeyAuthentication)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(rc => rc.EnableAadAuth)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasMany(rc => rc.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.RedisCacheId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(rc => rc.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
