import { Injectable } from '@angular/core';
import { WebTracerProvider, BatchSpanProcessor } from '@opentelemetry/sdk-trace-web';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-http';
import { FetchInstrumentation } from '@opentelemetry/instrumentation-fetch';
import { registerInstrumentations } from '@opentelemetry/instrumentation';
import { resourceFromAttributes } from '@opentelemetry/resources';
import { ATTR_SERVICE_NAME } from '@opentelemetry/semantic-conventions';

/**
 * TelemetryService — initializes OpenTelemetry Web SDK for end-to-end tracing.
 *
 * When running under Aspire (`environment.otlpEnabled = true`), Angular dev
 * server proxies /otlp/* to the Aspire Dashboard OTLP HTTP endpoint, so no
 * CORS configuration is needed.
 *
 * The FetchInstrumentation automatically adds W3C `traceparent` headers to
 * every outgoing Fetch/XHR request, enabling the .NET backend to correlate
 * its spans with the browser-initiated span visible in the Aspire Dashboard.
 */
@Injectable({ providedIn: 'root' })
export class TelemetryService {
  private initialized = false;

  /**
   * Call once at application startup (e.g. in app.config.ts via APP_INITIALIZER).
   * Is a no-op when called more than once.
   *
   * @param serviceName  Resource name shown in the Aspire Dashboard traces view.
   */
  initialize(serviceName = 'angular-frontend'): void {
    if (this.initialized) return;
    this.initialized = true;

    const exporter = new OTLPTraceExporter({
      // Proxied by the Angular dev server to Aspire Dashboard OTLP endpoint.
      // See proxy.conf.js for the runtime rewrite.
      url: '/otlp/v1/traces',
    });

    const provider = new WebTracerProvider({
      resource: resourceFromAttributes({ [ATTR_SERVICE_NAME]: serviceName }),
      spanProcessors: [new BatchSpanProcessor(exporter)],
    });

    provider.register();

    registerInstrumentations({
      instrumentations: [
        new FetchInstrumentation({
          // Propagate traceparent to all origins so the .NET API can correlate spans.
          propagateTraceHeaderCorsUrls: [/.*/],
          clearTimingResources: true,
        }),
      ],
      tracerProvider: provider,
    });

    console.debug('[Telemetry] OpenTelemetry Web SDK initialized →', serviceName);
  }
}
