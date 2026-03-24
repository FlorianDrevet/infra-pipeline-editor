using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.SqlServerAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="SqlServer"/> aggregate.</summary>
public class SqlServerConfiguration : IEntityTypeConfiguration<SqlServer>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SqlServer> builder)
    {
        ConfigureTable(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<SqlServer> builder)
    {
        builder.HasBaseType<AzureResource>()
            .ToTable("SqlServers");

        builder.Property(x => x.Version)
            .IsRequired()
            .HasConversion(
                v => v.Value.ToString(),
                v => new Domain.SqlServerAggregate.ValueObjects.SqlServerVersion(
                    Enum.Parse<Domain.SqlServerAggregate.ValueObjects.SqlServerVersion.SqlServerVersionEnum>(v)));

        builder.Property(x => x.AdministratorLogin)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasMany(x => x.EnvironmentSettings)
            .WithOne()
            .HasForeignKey(es => es.SqlServerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.EnvironmentSettings)
            .HasField("_environmentSettings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
