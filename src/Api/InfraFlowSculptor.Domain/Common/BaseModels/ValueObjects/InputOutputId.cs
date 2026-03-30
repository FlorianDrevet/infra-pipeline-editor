using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

/// <summary>Strongly-typed identifier for an <see cref="Entites.InputOutputLink"/>.</summary>
public class InputOutputId(Guid value) : Id<InputOutputId>(value);