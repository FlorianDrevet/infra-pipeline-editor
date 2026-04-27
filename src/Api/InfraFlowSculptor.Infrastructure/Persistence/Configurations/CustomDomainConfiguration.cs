using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="CustomDomain"/> entity.</summary>
public sealed class CustomDomainConfiguration : IEntityTypeConfiguration<CustomDomain>
{
    private const string TableName = "CustomDomains";

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CustomDomain> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(cd => cd.Id);

        builder.Property(cd => cd.Id)
            .HasConversion(new IdValueConverter<CustomDomainId>())
            .ValueGeneratedNever();

        builder.Property(cd => cd.ResourceId)
            .HasConversion(new IdValueConverter<AzureResourceId>())
            .IsRequired();

        builder.Property(cd => cd.EnvironmentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(cd => cd.DomainName)
            .IsRequired()
            .HasMaxLength(253);

        builder.Property(cd => cd.BindingType)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("SniEnabled");

        builder.HasIndex(cd => new { cd.ResourceId, cd.EnvironmentName, cd.DomainName })
            .IsUnique();
    }
}
