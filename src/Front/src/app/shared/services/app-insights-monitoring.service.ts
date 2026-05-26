import { ErrorHandler, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { ApplicationInsights } from '@microsoft/applicationinsights-web';
import { AngularPlugin } from '@microsoft/applicationinsights-angularplugin-js';

/**
 * AppInsightsMonitoringService — initializes Microsoft Application Insights for
 * production frontend telemetry (page views, exceptions, custom events, dependencies).
 *
 * Only activates when a valid connection string is provided to `initialize()`.
 * In local/Aspire dev, the OTel-based TelemetryService is used instead.
 */
@Injectable({ providedIn: 'root' })
export class AppInsightsMonitoringService {
  private appInsights: ApplicationInsights | null = null;
  private initialized = false;

  constructor(private readonly router: Router) {}

  /**
   * Initialize App Insights. Call once at app startup via APP_INITIALIZER.
   * No-op if already initialized or connectionString is empty.
   */
  initialize(connectionString: string): void {
    if (this.initialized) return;
    if (!connectionString) return;

    this.initialized = true;

    const angularPlugin = new AngularPlugin();

    this.appInsights = new ApplicationInsights({
      config: {
        connectionString,
        extensions: [angularPlugin],
        extensionConfig: {
          [angularPlugin.identifier]: {
            router: this.router,
          },
        },
        enableAutoRouteTracking: false, // handled by AngularPlugin
        enableCorsCorrelation: true,
        enableRequestHeaderTracking: true,
        enableResponseHeaderTracking: true,
        disableFetchTracking: false,
        enableAjaxPerfTracking: true,
        maxAjaxCallsPerView: -1,
      },
    });

    this.appInsights.loadAppInsights();
    this.appInsights.trackPageView();

    console.debug('[AppInsights] Application Insights initialized');
  }

  /**
   * Track a custom event.
   */
  trackEvent(name: string, properties?: Record<string, string>): void {
    this.appInsights?.trackEvent({ name }, properties);
  }

  /**
   * Track an exception manually.
   */
  trackException(error: Error, severityLevel?: number): void {
    this.appInsights?.trackException({ exception: error, severityLevel });
  }

  /**
   * Track a page view manually (e.g., for virtual navigations not caught by the router).
   */
  trackPageView(name?: string, uri?: string): void {
    this.appInsights?.trackPageView({ name, uri });
  }

  /**
   * Flush all queued telemetry (useful before page unload).
   */
  flush(): void {
    this.appInsights?.flush();
  }
}

/**
 * Global Angular ErrorHandler that reports uncaught exceptions to Application Insights.
 * Falls back to console.error when App Insights is not configured.
 */
@Injectable()
export class AppInsightsErrorHandler implements ErrorHandler {
  constructor(private readonly appInsightsService: AppInsightsMonitoringService) {}

  handleError(error: Error): void {
    this.appInsightsService.trackException(error);
    console.error(error);
  }
}
