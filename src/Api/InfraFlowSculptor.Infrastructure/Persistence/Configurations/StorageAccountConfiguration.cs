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

        builder.HasMany(sa => sa.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.StorageAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(sa => sa.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}