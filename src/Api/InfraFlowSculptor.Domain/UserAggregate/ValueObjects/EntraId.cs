using Shared.Domain.Domain.Models;
using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

public sealed class EntraId(Guid entraId) : SingleValueObject<Guid>(entraId);