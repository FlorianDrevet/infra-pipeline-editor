import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideExperimentalZonelessChangeDetection,
} from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';

import { routes } from './app-routing';
import { environment } from '../environments/environment';
import { TelemetryService } from './shared/services/telemetry.service';
import { LanguageService } from './shared/services/language.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideExperimentalZonelessChangeDetection(),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(),
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


