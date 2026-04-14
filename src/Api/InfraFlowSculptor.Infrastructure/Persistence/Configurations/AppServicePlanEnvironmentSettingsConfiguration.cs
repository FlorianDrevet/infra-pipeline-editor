using InfraFlowSculptor.Domain.AppServicePlanAggregate.Entities;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="AppServicePlanEnvironmentSettings"/> entity.</summary>
public class AppServicePlanEnvironmentSettingsConfiguration
    : IEntityTypeConfiguration<AppServicePlanEnvironmentSettings>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AppServicePlanEnvironmentSettings> builder)
    {
        builder.ToTable("AppServicePlanEnvironmentSettings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(new IdValueConverter<AppServicePlanEnvironmentSettingsId>());

        builder.Property(x => x.AppServicePlanId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(x => x.EnvironmentName)
            .IsRequired();

        builder.Property(x => x.Sku)
            .HasConversion(
                v => (object?)v != null ? v.Value.ToString() : null,
                v => v != null
                    ? new AppServicePlanSku(Enum.Parse<AppServicePlanSku.AppServicePlanSkuEnum>(v))
                    : null);

        builder.Property(x => x.Capacity);
    }
}
