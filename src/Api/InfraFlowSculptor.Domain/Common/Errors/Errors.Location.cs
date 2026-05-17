using ErrorOr;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors related to location parsing and validation.</summary>
    public static class Location
    {
        private const string InvalidLocationCode = "Location.Invalid";

        /// <summary>Returns an error when a location string cannot be parsed into a valid location enum value.</summary>
        public static Error InvalidLocation(string raw) =>
            Error.Validation(code: InvalidLocationCode, description: $"Invalid location '{raw}'.");
    }
}
