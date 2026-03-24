using InfraFlowSculptor.Domain.ApplicationInsightsAggregate;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="ApplicationInsights"/> aggregate.</summary>
public sealed class ApplicationInsightsEfConfiguration : IEntityTypeConfiguration<ApplicationInsights>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ApplicationInsights> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("ApplicationInsights");

        builder.Property(x => x.LogAnalyticsWorkspaceId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.HasMany(x => x.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.ApplicationInsightsId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
