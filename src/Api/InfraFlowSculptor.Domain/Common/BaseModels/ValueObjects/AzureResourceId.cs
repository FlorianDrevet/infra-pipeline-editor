using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

public class AzureResourceId(Guid value): Id<AzureResourceId>(value);