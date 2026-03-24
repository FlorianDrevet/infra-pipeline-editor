using InfraFlowSculptor.Domain.AppServicePlanAggregate;
using InfraFlowSculptor.Domain.Common.BaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="AppServicePlan"/> aggregate.</summary>
public class AppServicePlanConfiguration : IEntityTypeConfiguration<AppServicePlan>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AppServicePlan> builder)
    {
        ConfigureTable(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<AppServicePlan> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("AppServicePlans");

        builder.Property(x => x.OsType)
            .IsRequired()
            .HasConversion(
                v => v.Value.ToString(),
                v => new Domain.AppServicePlanAggregate.ValueObjects.AppServicePlanOsType(
                    Enum.Parse<Domain.AppServicePlanAggregate.ValueObjects.AppServicePlanOsType.AppServicePlanOsTypeEnum>(v)));

        builder.HasMany(x => x.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.AppServicePlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
