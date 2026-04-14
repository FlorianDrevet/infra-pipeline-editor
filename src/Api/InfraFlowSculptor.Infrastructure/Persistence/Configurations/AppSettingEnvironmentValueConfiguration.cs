using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="AppSettingEnvironmentValue"/> entity.</summary>
public sealed class AppSettingEnvironmentValueConfiguration
    : IEntityTypeConfiguration<AppSettingEnvironmentValue>
{
    public void Configure(EntityTypeBuilder<AppSettingEnvironmentValue> builder)
    {
        builder.ToTable("AppSettingEnvironmentValues");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<AppSettingEnvironmentValueId>())
            .ValueGeneratedNever();

        builder.Property(x => x.AppSettingId)
            .HasConversion(new IdValueConverter<AppSettingId>())
            .IsRequired();

        builder.Property(x => x.EnvironmentName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(4000);

        builder.HasIndex(x => new { x.AppSettingId, x.EnvironmentName })
            .IsUnique();
    }
}
