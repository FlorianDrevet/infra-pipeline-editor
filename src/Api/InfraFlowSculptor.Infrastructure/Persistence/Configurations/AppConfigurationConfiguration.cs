using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.Common.BaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="AppConfiguration"/> aggregate root (TPT).
/// </summary>
public class AppConfigurationConfiguration : IEntityTypeConfiguration<AppConfiguration>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AppConfiguration> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("AppConfigurations");

        builder.HasMany(ac => ac.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.AppConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(ac => ac.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
