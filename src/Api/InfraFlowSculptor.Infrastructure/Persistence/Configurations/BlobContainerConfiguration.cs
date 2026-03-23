using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public class BlobContainerConfiguration : IEntityTypeConfiguration<BlobContainer>
{
    public void Configure(EntityTypeBuilder<BlobContainer> builder)
    {
        builder.ToTable("BlobContainers");

        builder.HasKey(bc => bc.Id);

        builder.Property(bc => bc.Id)
            .HasConversion(new IdValueConverter<BlobContainerId>())
            .ValueGeneratedNever();

        builder.Property(bc => bc.StorageAccountId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(bc => bc.Name)
            .IsRequired();

        builder.Property(bc => bc.PublicAccess)
            .IsRequired()
            .HasConversion(new EnumValueConverter<BlobContainerPublicAccess, BlobContainerPublicAccess.AccessLevel>());
    }
}