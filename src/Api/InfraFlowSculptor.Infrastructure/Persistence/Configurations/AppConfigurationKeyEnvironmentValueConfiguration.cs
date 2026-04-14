using InfraFlowSculptor.Domain.AppConfigurationAggregate.Entities;
using InfraFlowSculptor.Domain.AppConfigurationAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="AppConfigurationKeyEnvironmentValue"/> entity.</summary>
public sealed class AppConfigurationKeyEnvironmentValueConfiguration
    : IEntityTypeConfiguration<AppConfigurationKeyEnvironmentValue>
{
    public void Configure(EntityTypeBuilder<AppConfigurationKeyEnvironmentValue> builder)
    {
        builder.ToTable("AppConfigurationKeyEnvironmentValues");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<AppConfigurationKeyEnvironmentValueId>())
            .ValueGeneratedNever();

        builder.Property(x => x.AppConfigurationKeyId)
            .HasConversion(new IdValueConverter<AppConfigurationKeyId>())
            .IsRequired();

        builder.Property(x => x.EnvironmentName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(4000);

        builder.HasIndex(x => new { x.AppConfigurationKeyId, x.EnvironmentName })
            .IsUnique();
    }
}
