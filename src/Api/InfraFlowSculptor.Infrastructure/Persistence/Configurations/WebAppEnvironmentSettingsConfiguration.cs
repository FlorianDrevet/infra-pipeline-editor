using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate.Entities;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="WebAppEnvironmentSettings"/> entity.</summary>
public class WebAppEnvironmentSettingsConfiguration
    : IEntityTypeConfiguration<WebAppEnvironmentSettings>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<WebAppEnvironmentSettings> builder)
    {
        builder.ToTable("WebAppEnvironmentSettings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<WebAppEnvironmentSettingsId>());

        builder.Property(x => x.WebAppId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(x => x.EnvironmentName)
            .IsRequired();

        builder.Property(x => x.AlwaysOn);

        builder.Property(x => x.HttpsOnly);

        builder.Property(x => x.RuntimeStack)
            .HasConversion(
                v => (object?)v != null ? v.Value.ToString() : null,
                v => v != null
                    ? new WebAppRuntimeStack(Enum.Parse<WebAppRuntimeStack.WebAppRuntimeStackEnum>(v))
                    : null);

        builder.Property(x => x.RuntimeVersion);

        builder.Property(x => x.DockerImageTag)
            .IsRequired(false);
    }
}
