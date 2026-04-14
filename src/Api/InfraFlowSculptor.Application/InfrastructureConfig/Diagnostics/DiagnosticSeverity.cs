namespace InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics;

/// <summary>Severity levels for configuration diagnostics.</summary>
public enum DiagnosticSeverity
{
    /// <summary>Informational finding — no action required.</summary>
    Info,

    /// <summary>Potential issue that should be reviewed.</summary>
    Warning,

    /// <summary>Critical issue that will likely cause a deployment failure.</summary>
    Error
}
