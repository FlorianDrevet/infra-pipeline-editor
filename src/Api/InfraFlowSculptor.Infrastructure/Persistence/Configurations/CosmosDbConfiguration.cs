using InfraFlowSculptor.Domain.CosmosDbAggregate;
using InfraFlowSculptor.Domain.Common.BaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="CosmosDb"/> aggregate root (TPT).
/// </summary>
public class CosmosDbConfiguration : IEntityTypeConfiguration<CosmosDb>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CosmosDb> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("CosmosDbAccounts");

        builder.HasMany(c => c.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.CosmosDbId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
