using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Converters;
using InfraFlowSculptor.Infrastructure.Persistence.Configurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraFlowSculptor.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="PersonalAccessToken"/> aggregate.
/// </summary>
public sealed class PersonalAccessTokenConfiguration : IEntityTypeConfiguration<PersonalAccessToken>
{
    private const string TableName = "PersonalAccessTokens";
    private const int MaxNameLength = 100;
    private const int MaxTokenPrefixLength = 20;
    private const int MaxTokenHashLength = 64;

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PersonalAccessToken> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(t => t.Id);

        builder.ConfigureAggregateRootId<PersonalAccessToken, PersonalAccessTokenId>();

        builder.Property(t => t.UserId)
            .HasConversion(new IdValueConverter<UserId>())
            .IsRequired();

        builder.Property(t => t.Name)
            .HasMaxLength(MaxNameLength)
            .IsRequired();

        builder.Property(t => t.TokenHash)
            .HasConversion(new SingleValueConverter<TokenHash, string>())
            .HasMaxLength(MaxTokenHashLength)
            .IsRequired();

        builder.HasIndex(t => t.TokenHash)
            .IsUnique();

        builder.Property(t => t.TokenPrefix_)
            .HasColumnName("TokenPrefix")
            .HasMaxLength(MaxTokenPrefixLength)
            .IsRequired();

        builder.Property(t => t.ExpiresAt);
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.LastUsedAt);
        builder.Property(t => t.IsRevoked).IsRequired();

        builder.HasIndex(t => t.UserId);
    }
}
