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
  msalConfig: MsalConfigInterface;
}
