using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Persistence.Configurations;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public class StorageTableConfiguration : IEntityTypeConfiguration<StorageTable>
{
    public void Configure(EntityTypeBuilder<StorageTable> builder)
    {
        builder.ToTable("StorageTables");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(new IdValueConverter<StorageTableId>())
            .ValueGeneratedNever();

        builder.Property(t => t.StorageAccountId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(t => t.Name)
            .IsRequired();
    }
}
