using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using InfraFlowSculptor.Domain.FunctionAppAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="FunctionApp"/> aggregate.</summary>
public sealed class FunctionAppConfiguration : IEntityTypeConfiguration<FunctionApp>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<FunctionApp> builder)
    {
        ConfigureTable(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<FunctionApp> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("FunctionApps");

        builder.Property(x => x.AppServicePlanId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(x => x.RuntimeStack)
            .IsRequired()
            .HasConversion(
                v => v.Value.ToString(),
                v => new FunctionAppRuntimeStack(
                    Enum.Parse<FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum>(v)));

        builder.Property(x => x.RuntimeVersion)
            .IsRequired();

        builder.Property(x => x.HttpsOnly);

        builder.Property(x => x.DeploymentMode)
            .IsRequired()
            .HasConversion(
                v => v.Value.ToString(),
                v => new DeploymentMode(
                    Enum.Parse<DeploymentMode.DeploymentModeType>(v)));

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
            .IsRequired(false);

        builder.Property(x => x.DockerfilePath)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.SourceCodePath)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.BuildCommand)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(x => x.ApplicationName)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.HasMany(x => x.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.FunctionAppId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
