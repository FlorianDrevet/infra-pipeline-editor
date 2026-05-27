export interface MsalConfigInterface {
  clientId: string;
  authority: string;
  redirectUri: string;
  /** Optional additional API scopes to include in the token request. */
  apiScopes?: string[];
}

export interface EnvironmentInterface {
  production: boolean;
  api_url: string;
  /** Enable OpenTelemetry traces export to Aspire Dashboard via /otlp proxy. */
  otlpEnabled?: boolean;
  /** Application Insights connection string for production telemetry. */
  appInsightsConnectionString?: string;
  /** Optional human-readable environment name displayed in the status bar (e.g. "production", "preview"). */
  name?: string;
  /** Optional application version displayed in the status bar (build-time injection). */
  version?: string;
  msalConfig: MsalConfigInterface;
}
