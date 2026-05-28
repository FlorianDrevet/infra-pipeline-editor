using ErrorOr;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.NetworkingProfileAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors related to the NetworkingProfile aggregate.</summary>
    public static class NetworkingProfile
    {
        /// <summary>Returned when a NetworkingProfile with the specified identifier does not exist.</summary>
        public static Error NotFound(NetworkingProfileId id) => Error.NotFound(
            code: "NetworkingProfile.NotFound",
            description: $"A networking profile with the given id '{id}' does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );

        /// <summary>Returned when no networking profile exists for the specified infra config.</summary>
        public static Error NotFoundForInfraConfig(InfrastructureConfigId infraConfigId) => Error.NotFound(
            code: "NetworkingProfile.NotFoundForInfraConfig",
            description: $"No networking profile exists for infrastructure configuration '{infraConfigId}'.",
            metadata: new Dictionary<string, object> { { "InfraConfigId", infraConfigId.ToString() } }
        );

        /// <summary>Returned when a networking profile already exists for the specified infra config.</summary>
        public static Error AlreadyExists(InfrastructureConfigId infraConfigId) => Error.Conflict(
            code: "NetworkingProfile.AlreadyExists",
            description: $"A networking profile already exists for infrastructure configuration '{infraConfigId}'.",
            metadata: new Dictionary<string, object> { { "InfraConfigId", infraConfigId.ToString() } }
        );
    }
}
