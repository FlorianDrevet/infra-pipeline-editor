using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate.Entities;
using InfraFlowSculptor.Domain.ContainerAppAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="ContainerAppEnvironmentSettings"/> entity.
/// </summary>
public sealed class ContainerAppEnvironmentSettingsConfiguration
    : IEntityTypeConfiguration<ContainerAppEnvironmentSettings>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ContainerAppEnvironmentSettings> builder)
    {
        builder.ToTable("ContainerAppEnvironmentSettings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<ContainerAppEnvironmentSettingsId>());

        builder.Property(x => x.ContainerAppId)
            .IsRequired()
            .HasConversion(new IdValueConverter<AzureResourceId>());

        builder.Property(x => x.EnvironmentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CpuCores)
            .IsRequired(false)
            .HasMaxLength(10);

        builder.Property(x => x.MemoryGi)
            .IsRequired(false)
            .HasMaxLength(10);

        builder.Property(x => x.MinReplicas)
            .IsRequired(false);

        builder.Property(x => x.MaxReplicas)
            .IsRequired(false);

        builder.Property(x => x.IngressEnabled)
            .IsRequired(false);

        builder.Property(x => x.IngressTargetPort)
            .IsRequired(false);

        builder.Property(x => x.IngressExternal)
            .IsRequired(false);

        builder.Property(x => x.TransportMethod)
            .IsRequired(false)
            .HasMaxLength(20);

        builder.Property(x => x.ReadinessProbePath)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(x => x.ReadinessProbePort)
            .IsRequired(false);

        builder.Property(x => x.LivenessProbePath)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(x => x.LivenessProbePort)
            .IsRequired(false);

        builder.Property(x => x.StartupProbePath)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(x => x.StartupProbePort)
            .IsRequired(false);

        builder.HasIndex(x => new { x.ContainerAppId, x.EnvironmentName })
            .IsUnique();
    }
}
