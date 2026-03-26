using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.StorageAccountAggregate;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public class StorageAccountConfiguration : IEntityTypeConfiguration<StorageAccount>
{
    public void Configure(EntityTypeBuilder<StorageAccount> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("StorageAccounts");

        builder.Property(s => s.Kind)
            .IsRequired()
            .HasConversion(new EnumValueConverter<StorageAccountKind, StorageAccountKind.Kind>());

        builder.Property(s => s.AccessTier)
            .IsRequired()
            .HasConversion(new EnumValueConverter<StorageAccessTier, StorageAccessTier.Tier>());

        builder.Property(s => s.AllowBlobPublicAccess)
            .IsRequired();

        builder.Property(s => s.EnableHttpsTrafficOnly)
            .IsRequired();

        builder.Property(s => s.MinimumTlsVersion)
            .IsRequired()
            .HasConversion(new EnumValueConverter<StorageAccountTlsVersion, StorageAccountTlsVersion.Version>());

        builder.HasMany(s => s.BlobContainers)
            .WithOne()
            .HasForeignKey(bc => bc.StorageAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.BlobContainers)
            .HasField("_blobContainers")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(s => s.Queues)
            .WithOne()
            .HasForeignKey(q => q.StorageAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Queues)
            .HasField("_queues")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(s => s.Tables)
            .WithOne()
            .HasForeignKey(t => t.StorageAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Tables)
            .HasField("_tables")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(s => s.AllCorsRules)
            .WithOne()
            .HasForeignKey(cr => cr.StorageAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.AllCorsRules)
            .HasField("_corsRules")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(sa => sa.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.StorageAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(sa => sa.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}