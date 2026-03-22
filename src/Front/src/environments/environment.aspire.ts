import { EnvironmentInterface } from '../app/shared/interfaces/environment.interface';

/**
 * Aspire environment configuration.
 *
 * - api_url points to /api-proxy so Angular dev-server proxy.conf.js
 *   can relay requests to the real backend URL injected by Aspire.
 * - bicep_api_url points to /bicep-api-proxy for the Bicep Generator API.
 * - otlpEnabled activates the OpenTelemetry Web SDK and sends browser
 *   traces to /otlp/v1/traces (also proxied by the dev server to the
 *   Aspire Dashboard OTLP endpoint).
 */
export const environment: EnvironmentInterface = {
  production: false,
  api_url: '/api-proxy',
  bicep_api_url: '/bicep-api-proxy',
  otlpEnabled: true,
  msalConfig: {
    clientId: '24c34231-a984-43b3-8ac3-9278ebd067ef',
    authority: 'https://login.microsoftonline.com/cc625709-6696-4cf6-a330-7baf406f6a99',
    redirectUri: 'http://localhost:4200',
    apiScopes: ['api://4f7f2dbd-8a11-42c4-bedc-acbb127e9394/Configuration.Write'],
    bicepApiScopes: ['api://6960eaa6-7cc7-484e-9fc7-d53152006297/Generate'],
  },
};

