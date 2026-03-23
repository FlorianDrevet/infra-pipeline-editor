using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

public class InputOutputId(Guid value): Id<InputOutputId>(value);