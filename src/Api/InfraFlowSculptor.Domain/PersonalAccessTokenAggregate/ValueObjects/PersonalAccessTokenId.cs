using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="PersonalAccessToken"/>.</summary>
public sealed class PersonalAccessTokenId(Guid value) : Id<PersonalAccessTokenId>(value);
