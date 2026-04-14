import { ResourceDiagnosticResponse } from './bicep-generator.interface';

export interface ConfigDiagnosticsResponse {
  diagnostics: ResourceDiagnosticResponse[];
}

// Re-export from bicep-generator for convenience
export type { ResourceDiagnosticResponse } from './bicep-generator.interface';
