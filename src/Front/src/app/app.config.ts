import { APP_INITIALIZER, ApplicationConfig, provideExperimentalZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

import { routes } from './app-routing';
import { environment } from '../environments/environment';
import { TelemetryService } from './shared/services/telemetry.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideExperimentalZonelessChangeDetection(),
    provideRouter(routes),
    provideAnimationsAsync(),
    ...(environment.otlpEnabled
      ? [
          {
            provide: APP_INITIALIZER,
            useFactory: (telemetry: TelemetryService) => () => telemetry.initialize(),
            deps: [TelemetryService],
            multi: true,
          },
        ]
      : []),
  ],
};


