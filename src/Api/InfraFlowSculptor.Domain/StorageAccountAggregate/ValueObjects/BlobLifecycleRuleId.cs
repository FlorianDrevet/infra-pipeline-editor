using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

public sealed class BlobLifecycleRuleId(Guid value) : Id<BlobLifecycleRuleId>(value);
