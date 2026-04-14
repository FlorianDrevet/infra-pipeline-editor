using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

/// <summary>Strongly-typed identifier for a <see cref="Entites.RoleAssignment"/>.</summary>
public class RoleAssignmentId(Guid value) : Id<RoleAssignmentId>(value);
