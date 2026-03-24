/**
 * proxy.conf.js — Angular Dev Server proxy for Aspire local orchestration.
 *
 * When running via `npm run start:aspire`, Aspire injects:
 *   services__infraflowsculptor-api__http__0   → HTTP URL of the main API
 *   services__infraflowsculptor-api__https__0  → HTTPS URL of the main API
 *   OTEL_EXPORTER_OTLP_ENDPOINT                → Aspire Dashboard OTLP endpoint
 *
 * The Angular dev server proxies:
 *   /api-proxy/*  → main API  (removes /api-proxy prefix)
 *   /otlp/*       → Aspire Dashboard OTLP endpoint  (removes /otlp prefix)
 *
 * This avoids CORS issues: the browser only talks to localhost:4200 and the
 * Node.js dev server relays to the real backend URLs.
 */

const apiUrl =
  process.env['services__infraflowsculptor-api__https__0'] ||
  process.env['services__infraflowsculptor-api__http__0'] ||
  'http://localhost:5257';

const otlpUrl = process.env['OTEL_EXPORTER_OTLP_ENDPOINT'] || null;

console.log(`[Aspire Proxy] Main API → ${apiUrl}`);
if (otlpUrl) {
  console.log(`[Aspire Proxy] OTLP     → ${otlpUrl}`);
} else {
  console.log('[Aspire Proxy] OTLP not configured (OTEL_EXPORTER_OTLP_ENDPOINT not set)');
}

const proxies = [
  {
    context: ['/api-proxy'],
    target: apiUrl,
    secure: false,
    changeOrigin: true,
    pathRewrite: { '^/api-proxy': '' },
    logLevel: 'info',
  },
];

if (otlpUrl) {
  proxies.push({
    context: ['/otlp'],
    target: otlpUrl,
    secure: false,
    changeOrigin: true,
    pathRewrite: { '^/otlp': '' },
    logLevel: 'info',
  });
}

module.exports = proxies;

