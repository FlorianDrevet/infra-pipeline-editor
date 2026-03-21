import { Injectable, inject, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

export type AppLanguage = 'fr' | 'en';

export interface LanguageOption {
  readonly code: AppLanguage;
  readonly labelKey: string;
}

@Injectable({
  providedIn: 'root',
})
export class LanguageService {
  private readonly translateService = inject(TranslateService);

  private readonly storageKey = 'infra-flow-sculptor.language';
  private readonly defaultLanguage: AppLanguage = 'fr';
  private readonly supportedLanguages = new Set<AppLanguage>(['fr', 'en']);
  private readonly selectedLanguage = signal<AppLanguage>(this.defaultLanguage);

  public readonly currentLanguage = this.selectedLanguage.asReadonly();
  public readonly availableLanguages: readonly LanguageOption[] = [
    { code: 'fr', labelKey: 'LANGUAGE.FRENCH_SHORT' },
    { code: 'en', labelKey: 'LANGUAGE.ENGLISH_SHORT' },
  ];

  public initialize(): void {
    this.translateService.addLangs(Array.from(this.supportedLanguages));
    this.translateService.setDefaultLang(this.defaultLanguage);
    this.setLanguage(this.resolveInitialLanguage());
  }

  public setLanguage(language: string): void {
    const normalizedLanguage = this.normalizeLanguage(language);

    this.selectedLanguage.set(normalizedLanguage);
    this.translateService.use(normalizedLanguage);

    this.persistLanguage(normalizedLanguage);
  }

  public translate(key: string, interpolateParams?: Record<string, unknown>): string {
    return this.translateService.instant(key, interpolateParams);
  }

  private resolveInitialLanguage(): AppLanguage {
    const persistedLanguage = this.readPersistedLanguage();

    if (persistedLanguage) {
      return persistedLanguage;
    }

    return this.normalizeLanguage(globalThis.navigator?.language ?? '');
  }

  private normalizeLanguage(language: string): AppLanguage {
    const normalizedLanguage = language.toLowerCase().split('-')[0] as AppLanguage;

    if (this.supportedLanguages.has(normalizedLanguage)) {
      return normalizedLanguage;
    }

    return this.defaultLanguage;
  }

  private persistLanguage(language: AppLanguage): void {
    try {
      globalThis.localStorage?.setItem(this.storageKey, language);
    } catch {
      // Access to localStorage can fail in private or restricted browser contexts.
    }
  }

  private readPersistedLanguage(): AppLanguage | null {
    try {
      const persistedLanguage = globalThis.localStorage?.getItem(this.storageKey);

      if (!persistedLanguage) {
        return null;
      }

      return this.normalizeLanguage(persistedLanguage);
    } catch {
      return null;
    }
  }
}
