using System.Text.Json.Serialization;

namespace InfraFlowSculptor.Mcp.Drafts;

/// <summary>Possible states of a project creation draft.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DraftStatus
{
    /// <summary>Draft needs more information before it can be created.</summary>
    RequiresClarification,

    /// <summary>Draft is complete and ready for creation.</summary>
    ReadyToCreate,
}
