import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideZonelessChangeDetection,
} from '@angular/core';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { provideRouter, withViewTransitions } from '@angular/router';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';

import { routes } from './app-routing';
import { environment } from '../environments/environment';
import { TelemetryService } from './shared/services/telemetry.service';
import { LanguageService } from './shared/services/language.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideRouter(routes, withViewTransitions()),
    provideAnimationsAsync(),
    provideHttpClient(withFetch()),
    provideClientHydration(withEventReplay()),
    provideTranslateService({
      lang: 'fr',
      fallbackLang: 'fr',
    }),
    ...provideTranslateHttpLoader({
      prefix: '/i18n/',
      suffix: '.json',
    }),
    provideAppInitializer(() => {
      inject(LanguageService).initialize();
    }),
    ...(environment.otlpEnabled
      ? [
          provideAppInitializer(() => {
            inject(TelemetryService).initialize();
          }),
        ]
      : []),
  ],
};


