using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Extensions;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public class KeyVaultConfiguration : IEntityTypeConfiguration<KeyVault>
{
    public void Configure(EntityTypeBuilder<KeyVault> builder)
    {
        ConfigureUsersTable(builder);
    }

    private static void ConfigureUsersTable(EntityTypeBuilder<KeyVault> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("KeyVaults");

        builder.Property(kv => kv.EnableRbacAuthorization)
            .IsRequired();

        builder.Property(kv => kv.EnabledForDeployment)
            .IsRequired();

        builder.Property(kv => kv.EnabledForDiskEncryption)
            .IsRequired();

        builder.Property(kv => kv.EnabledForTemplateDeployment)
            .IsRequired();

        builder.Property(kv => kv.EnablePurgeProtection)
            .IsRequired();

        builder.Property(kv => kv.EnableSoftDelete)
            .IsRequired();

        builder.HasMany(kv => kv.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.KeyVaultId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(kv => kv.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}