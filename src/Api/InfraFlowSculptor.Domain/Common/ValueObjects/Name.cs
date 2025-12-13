using BicepGenerator.Domain.Common.Models;
using Shared.Domain.Domain.Models;
using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

public sealed class Name(string name): SingleValueObject<string>(name);