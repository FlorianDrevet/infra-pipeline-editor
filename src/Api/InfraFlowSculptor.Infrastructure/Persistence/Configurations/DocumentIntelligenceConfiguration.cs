using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

public class DocumentIntelligenceConfiguration : IEntityTypeConfiguration<DocumentIntelligence>
{
    public void Configure(EntityTypeBuilder<DocumentIntelligence> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("DocumentIntelligences");

        builder.Property(di => di.CustomSubDomainName)
            .IsRequired(false)
            .HasMaxLength(64);

        builder.HasMany(di => di.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.DocumentIntelligenceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(di => di.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
