import { EnvironmentInterface } from '../app/shared/interfaces/environment.interface';

/**
 * Aspire environment configuration.
 *
 * - api_url points to /api-proxy so Angular dev-server proxy.conf.js
 *   can relay requests to the real backend URL injected by Aspire.
 * - otlpEnabled activates the OpenTelemetry Web SDK and sends browser
 *   traces to /otlp/v1/traces (also proxied by the dev server to the
 *   Aspire Dashboard OTLP endpoint).
 */
export const environment: EnvironmentInterface = {
  production: false,
  api_url: '/api-proxy',
  otlpEnabled: true,
  msalConfig: {
    clientId: '24c34231-a984-43b3-8ac3-9278ebd067ef',
    authority: 'https://login.microsoftonline.com/common',
    redirectUri: 'http://localhost:4200',
  },
};

