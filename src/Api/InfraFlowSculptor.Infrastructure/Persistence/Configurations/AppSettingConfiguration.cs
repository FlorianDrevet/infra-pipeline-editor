using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
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

        builder.HasMany(s => s.EnvironmentValues)
            .WithOne()
            .HasForeignKey(ev => ev.AppSettingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.SourceResourceId)
            .HasConversion(
                v => v == null ? (Guid?)null : v.Value,
                v => v.HasValue ? new AzureResourceId(v.Value) : null)
            .IsRequired(false);

        builder.Property(s => s.SourceOutputName)
            .IsRequired(false)
            .HasMaxLength(128);

        builder.Property(s => s.KeyVaultResourceId)
            .HasConversion(
                v => v == null ? (Guid?)null : v.Value,
                v => v.HasValue ? new AzureResourceId(v.Value) : null)
            .IsRequired(false);

        builder.Property(s => s.SecretName)
            .IsRequired(false)
            .HasMaxLength(256);

        builder.Property(s => s.SecretValueAssignment)
            .HasConversion<string?>()
            .IsRequired(false)
            .HasMaxLength(64);

        builder.HasOne<AzureResource>()
            .WithMany()
            .HasForeignKey(s => s.SourceResourceId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AzureResource>()
            .WithMany()
            .HasForeignKey(s => s.KeyVaultResourceId)
            .IsRequired(false)
            // Audit DB-006 (2026-04-23): SetNull avoids silent data loss when the referenced
            // KeyVault is deleted. The mapping becomes invalid → surfaced as a domain error
            // at write/generation time instead of a silent cascade.
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(s => new { s.ResourceId, s.Name })
            .IsUnique();

        builder.Property(s => s.VariableGroupId)
            .HasConversion(
                v => v == null ? (Guid?)null : v.Value,
                v => v.HasValue ? new ProjectPipelineVariableGroupId(v.Value) : null)
            .IsRequired(false);

        builder.Property(s => s.PipelineVariableName)
            .IsRequired(false)
            .HasMaxLength(256);

        builder.HasOne<ProjectPipelineVariableGroup>()
            .WithMany()
            .HasForeignKey(s => s.VariableGroupId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
