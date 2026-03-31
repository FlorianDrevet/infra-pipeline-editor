using InfraFlowSculptor.Domain.AppConfigurationAggregate.Entities;
using InfraFlowSculptor.Domain.AppConfigurationAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="AppConfigurationKey"/> entity.</summary>
public sealed class AppConfigurationKeyConfiguration : IEntityTypeConfiguration<AppConfigurationKey>
{
    public void Configure(EntityTypeBuilder<AppConfigurationKey> builder)
    {
        builder.ToTable("AppConfigurationKeys");

        builder.HasKey(k => k.Id);

        builder.Property(k => k.Id)
            .HasConversion(new IdValueConverter<AppConfigurationKeyId>())
            .ValueGeneratedNever();

        builder.Property(k => k.AppConfigurationId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(k => k.Key)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(k => k.Label)
            .IsRequired(false)
            .HasMaxLength(128);

        builder.HasMany(k => k.EnvironmentValues)
            .WithOne()
            .HasForeignKey(ev => ev.AppConfigurationKeyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(k => k.EnvironmentValues)
            .HasField("_environmentValues")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(k => k.SourceResourceId)
            .HasConversion(
                v => v == null ? (Guid?)null : v.Value,
                v => v.HasValue ? new AzureResourceId(v.Value) : null)
            .IsRequired(false);

        builder.Property(k => k.SourceOutputName)
            .IsRequired(false)
            .HasMaxLength(128);

        builder.HasOne<AzureResource>()
            .WithMany()
            .HasForeignKey(k => k.SourceResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(k => k.KeyVaultResourceId)
            .HasConversion(
                v => v == null ? (Guid?)null : v.Value,
                v => v.HasValue ? new AzureResourceId(v.Value) : null)
            .IsRequired(false);

        builder.Property(k => k.SecretName)
            .IsRequired(false)
            .HasMaxLength(256);

        builder.Property(k => k.SecretValueAssignment)
            .HasConversion<string?>()
            .IsRequired(false)
            .HasMaxLength(64);

        builder.HasOne<AzureResource>()
            .WithMany()
            .HasForeignKey(k => k.KeyVaultResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(k => new { k.AppConfigurationId, k.Key })
            .IsUnique();

        builder.Property(k => k.VariableGroupId)
            .HasConversion(
                v => v == null ? (Guid?)null : v.Value,
                v => v.HasValue ? new ProjectPipelineVariableGroupId(v.Value) : null)
            .IsRequired(false);

        builder.Property(k => k.PipelineVariableName)
            .IsRequired(false)
            .HasMaxLength(256);

        builder.HasOne<ProjectPipelineVariableGroup>()
            .WithMany()
            .HasForeignKey(k => k.VariableGroupId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
