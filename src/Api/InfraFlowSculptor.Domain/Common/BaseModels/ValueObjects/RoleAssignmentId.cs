using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

public class RoleAssignmentId(Guid value) : Id<RoleAssignmentId>(value);
