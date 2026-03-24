using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="WebApp"/> aggregate.</summary>
public class WebAppConfiguration : IEntityTypeConfiguration<WebApp>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<WebApp> builder)
    {
        ConfigureTable(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<WebApp> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("WebApps");

        builder.Property(x => x.AppServicePlanId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(x => x.RuntimeStack)
            .IsRequired()
            .HasConversion(
                v => v.Value.ToString(),
                v => new WebAppRuntimeStack(
                    Enum.Parse<WebAppRuntimeStack.WebAppRuntimeStackEnum>(v)));

        builder.Property(x => x.RuntimeVersion)
            .IsRequired();

        builder.Property(x => x.AlwaysOn);

        builder.Property(x => x.HttpsOnly);

        builder.HasMany(x => x.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.WebAppId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
