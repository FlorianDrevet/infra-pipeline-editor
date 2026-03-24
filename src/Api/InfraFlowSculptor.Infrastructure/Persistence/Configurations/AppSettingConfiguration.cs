using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="AppSetting"/> entity.</summary>
public sealed class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.ToTable("AppSettings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(new IdValueConverter<AppSettingId>())
            .ValueGeneratedNever();

        builder.Property(s => s.ResourceId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.StaticValue)
            .IsRequired(false)
            .HasMaxLength(4000);

        builder.Property(s => s.SourceResourceId)
            .HasConversion(
                v => v == null ? (Guid?)null : v.Value,
                v => v.HasValue ? new AzureResourceId(v.Value) : null)
            .IsRequired(false);

        builder.Property(s => s.SourceOutputName)
            .IsRequired(false)
            .HasMaxLength(128);

        builder.HasOne<AzureResource>()
            .WithMany()
            .HasForeignKey(s => s.SourceResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.ResourceId, s.Name })
            .IsUnique();
    }
}
