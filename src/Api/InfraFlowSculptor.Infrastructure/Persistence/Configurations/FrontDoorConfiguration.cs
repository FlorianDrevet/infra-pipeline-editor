using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.FrontDoorAggregate;
using InfraFlowSculptor.Domain.FrontDoorAggregate.Entities;
using InfraFlowSculptor.Domain.FrontDoorAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="FrontDoor"/> aggregate.</summary>
public class FrontDoorConfiguration : IEntityTypeConfiguration<FrontDoor>
{
    public void Configure(EntityTypeBuilder<FrontDoor> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("FrontDoors");

        builder.Property(f => f.WafPolicyEnabled)
            .IsRequired();

        builder.HasMany(f => f.Origins)
            .WithOne()
            .HasForeignKey(o => o.FrontDoorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(f => f.Origins)
            .HasField("_origins")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(f => f.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.FrontDoorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(f => f.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>EF Core configuration for the <see cref="FrontDoorOrigin"/> entity.</summary>
public class FrontDoorOriginConfiguration : IEntityTypeConfiguration<FrontDoorOrigin>
{
    public void Configure(EntityTypeBuilder<FrontDoorOrigin> builder)
    {
        builder.ToTable("FrontDoorOrigins");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(new IdValueConverter<FrontDoorOriginId>());

        builder.Property(o => o.FrontDoorId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(o => o.TargetResourceId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(o => o.HostName)
            .HasMaxLength(260);

        builder.Property(o => o.Weight).IsRequired();
        builder.Property(o => o.Priority).IsRequired();
    }
}

/// <summary>EF Core configuration for the <see cref="FrontDoorEnvironmentSettings"/> entity.</summary>
public class FrontDoorEnvironmentSettingsConfiguration : IEntityTypeConfiguration<FrontDoorEnvironmentSettings>
{
    public void Configure(EntityTypeBuilder<FrontDoorEnvironmentSettings> builder)
    {
        builder.ToTable("FrontDoorEnvironmentSettings");

        builder.HasKey(es => es.Id);

        builder.Property(es => es.Id)
            .HasConversion(new IdValueConverter<FrontDoorEnvironmentSettingsId>());

        builder.Property(es => es.FrontDoorId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(es => es.EnvironmentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(es => es.Sku)
            .HasConversion(new EnumValueConverter<FrontDoorSku, FrontDoorSku.Sku>())
            .IsRequired();

        builder.HasIndex(es => new { es.FrontDoorId, es.EnvironmentName })
            .IsUnique();
    }
}
