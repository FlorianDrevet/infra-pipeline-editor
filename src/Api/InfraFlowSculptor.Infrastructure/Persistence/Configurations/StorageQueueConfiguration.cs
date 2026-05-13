using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public class StorageQueueConfiguration : IEntityTypeConfiguration<StorageQueue>
{
    private const int StorageQueueNameMaxLength = 63;

    public void Configure(EntityTypeBuilder<StorageQueue> builder)
    {
        builder.ToTable("StorageQueues");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Id)
            .HasConversion(new IdValueConverter<StorageQueueId>())
            .ValueGeneratedNever();

        builder.Property(q => q.StorageAccountId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(q => q.Name)
            .IsRequired()
            .HasMaxLength(StorageQueueNameMaxLength);
    }
}
