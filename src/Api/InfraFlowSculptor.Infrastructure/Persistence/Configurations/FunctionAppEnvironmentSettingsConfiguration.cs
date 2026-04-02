using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.FunctionAppAggregate.Entities;
using InfraFlowSculptor.Domain.FunctionAppAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="FunctionAppEnvironmentSettings"/> entity.</summary>
public sealed class FunctionAppEnvironmentSettingsConfiguration
    : IEntityTypeConfiguration<FunctionAppEnvironmentSettings>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<FunctionAppEnvironmentSettings> builder)
    {
        builder.ToTable("FunctionAppEnvironmentSettings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<FunctionAppEnvironmentSettingsId>());

        builder.Property(x => x.FunctionAppId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(x => x.EnvironmentName)
            .IsRequired();

        builder.Property(x => x.HttpsOnly);

        builder.Property(x => x.MaxInstanceCount);

        builder.Property(x => x.DockerImageTag)
            .IsRequired(false);
    }
}
