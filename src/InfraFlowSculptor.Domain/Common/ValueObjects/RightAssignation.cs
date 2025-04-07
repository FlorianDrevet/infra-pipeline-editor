using InfraFlowSculptor.BicepDirector.Enums;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

public class RightAssignation: ValueObject
{
    public RightKind RightKind { get; protected set; }
    public string RightName { get; protected set; }
    public Guid ResourceId { get; protected set; }

    private RightAssignation()
    {
    }

    public RightAssignation(RightKind rightKind, string rightName, Guid resourceId)
    {
        RightKind = rightKind;
        RightName = rightName;
        ResourceId = resourceId;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return RightName;
        yield return RightKind;
        yield return ResourceId;
    }
}