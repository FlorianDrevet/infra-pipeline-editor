using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

public class InputOutputId(Guid value): Id<InputOutputId>(value);