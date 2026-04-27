using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="ContainerApp"/> aggregate.</summary>
public sealed class ContainerAppConfiguration : IEntityTypeConfiguration<ContainerApp>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ContainerApp> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("ContainerApps");

        builder.Property(x => x.ContainerAppEnvironmentId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(x => x.ContainerRegistryId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired(false);

        builder.Property(x => x.AcrAuthMode)
            .HasConversion(
                v => v == null ? null : v.Value.ToString(),
                v => string.IsNullOrWhiteSpace(v)
                    ? null
                    : new AcrAuthMode(
                        Enum.Parse<AcrAuthMode.AcrAuthModeType>(v)))
            .IsRequired(false);

        builder.Property(x => x.DockerImageName)
            .HasMaxLength(512)
            .IsRequired(false);

        builder.Property(x => x.DockerfilePath)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.ApplicationName)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.HasMany(x => x.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.ContainerAppId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
