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
    private const string DefaultDnsValidationStatus = "Pending";

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

        builder.Property(cd => cd.CertificateMode)
            .HasConversion(
                v => v.Value.ToString(),
                v => new CertificateMode(
                    Enum.Parse<CertificateMode.CertificateModeType>(v)))
            .IsRequired()
            .HasMaxLength(30)
            .HasDefaultValue(CertificateMode.ManagedCertificate);

        builder.Property(cd => cd.KeyVaultUrl)
            .HasMaxLength(500);

        builder.Property(cd => cd.ManagedIdentityResourceId)
            .HasMaxLength(500);

        builder.Property(cd => cd.CertificateName)
            .HasMaxLength(200);

        builder.Property(cd => cd.DnsValidationStatus)
            .HasConversion(
                v => v.Value.ToString(),
                v => new DnsValidationStatus(
                    Enum.Parse<DnsValidationStatus.DnsValidationStatusType>(v)))
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue(DnsValidationStatus.Pending);

        builder.HasIndex(cd => new { cd.ResourceId, cd.EnvironmentName, cd.DomainName })
            .IsUnique();
    }
}
