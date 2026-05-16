import { computed, effect, Injectable, signal } from '@angular/core';

export type BicepViewerTheme = 'azure-night' | 'graphite-frost' | 'sand-dusk';
export type AppTheme = 'dark' | 'light';

export interface BicepViewerThemeOption {
  readonly value: BicepViewerTheme;
  readonly labelKey: string;
  readonly descriptionKey: string;
  readonly previewColors: readonly [string, string, string];
}

export interface AppThemeOption {
  readonly value: AppTheme;
  readonly labelKey: string;
  readonly descriptionKey: string;
  readonly icon: string;
}

interface UserPreferencesState {
  readonly bicepViewerTheme: BicepViewerTheme;
  readonly appTheme: AppTheme;
}

const USER_PREFERENCES_STORAGE_KEY = 'infra-flow-sculptor.user-preferences';
const DEFAULT_BICEP_VIEWER_THEME: BicepViewerTheme = 'azure-night';
const DEFAULT_APP_THEME: AppTheme = 'dark';
const BICEP_VIEWER_THEMES: ReadonlySet<BicepViewerTheme> = new Set<BicepViewerTheme>([
  'azure-night',
  'graphite-frost',
  'sand-dusk',
]);
const APP_THEMES: ReadonlySet<AppTheme> = new Set<AppTheme>(['dark', 'light']);
const DEFAULT_USER_PREFERENCES: UserPreferencesState = {
  bicepViewerTheme: DEFAULT_BICEP_VIEWER_THEME,
  appTheme: DEFAULT_APP_THEME,
};
const BICEP_VIEWER_THEME_OPTIONS: readonly BicepViewerThemeOption[] = [
  {
    value: 'azure-night',
    labelKey: 'SETTINGS.PREFERENCES.BICEP_THEME_OPTIONS.AZURE_NIGHT.LABEL',
    descriptionKey: 'SETTINGS.PREFERENCES.BICEP_THEME_OPTIONS.AZURE_NIGHT.DESCRIPTION',
    previewColors: ['#adc8ff', '#89bcc9', '#d8c490'],
  },
  {
    value: 'graphite-frost',
    labelKey: 'SETTINGS.PREFERENCES.BICEP_THEME_OPTIONS.GRAPHITE_FROST.LABEL',
    descriptionKey: 'SETTINGS.PREFERENCES.BICEP_THEME_OPTIONS.GRAPHITE_FROST.DESCRIPTION',
    previewColors: ['#c7d2fe', '#bfdbfe', '#e2e8f0'],
  },
  {
    value: 'sand-dusk',
    labelKey: 'SETTINGS.PREFERENCES.BICEP_THEME_OPTIONS.SAND_DUSK.LABEL',
    descriptionKey: 'SETTINGS.PREFERENCES.BICEP_THEME_OPTIONS.SAND_DUSK.DESCRIPTION',
    previewColors: ['#fbbf24', '#f59e0b', '#fcd34d'],
  },
];

const APP_THEME_OPTIONS: readonly AppThemeOption[] = [
  {
    value: 'dark',
    labelKey: 'SETTINGS.PREFERENCES.APP_THEME_OPTIONS.DARK.LABEL',
    descriptionKey: 'SETTINGS.PREFERENCES.APP_THEME_OPTIONS.DARK.DESCRIPTION',
    icon: 'dark_mode',
  },
  {
    value: 'light',
    labelKey: 'SETTINGS.PREFERENCES.APP_THEME_OPTIONS.LIGHT.LABEL',
    descriptionKey: 'SETTINGS.PREFERENCES.APP_THEME_OPTIONS.LIGHT.DESCRIPTION',
    icon: 'light_mode',
  },
];

@Injectable({
  providedIn: 'root',
})
export class UserPreferencesService {
  private readonly selectedPreferences = signal<UserPreferencesState>(this.resolveInitialPreferences());

  public readonly preferences = this.selectedPreferences.asReadonly();
  public readonly bicepViewerTheme = computed(() => this.preferences().bicepViewerTheme);
  public readonly appTheme = computed(() => this.preferences().appTheme);
  public readonly bicepViewerThemeOptions = BICEP_VIEWER_THEME_OPTIONS;
  public readonly appThemeOptions = APP_THEME_OPTIONS;

  constructor() {
    effect(() => {
      const theme = this.appTheme();
      document.documentElement.dataset['theme'] = theme;

      const colorSchemeMeta = document.querySelector('meta[name="color-scheme"]');
      if (colorSchemeMeta) {
        colorSchemeMeta.setAttribute('content', theme);
      }
    });
  }

  public setBicepViewerTheme(theme: BicepViewerTheme): void {
    const nextPreferences: UserPreferencesState = {
      ...this.selectedPreferences(),
      bicepViewerTheme: theme,
    };

    this.selectedPreferences.set(nextPreferences);
    this.persistPreferences(nextPreferences);
  }

  public setAppTheme(theme: AppTheme): void {
    const nextPreferences: UserPreferencesState = {
      ...this.selectedPreferences(),
      appTheme: theme,
    };

    this.selectedPreferences.set(nextPreferences);
    this.persistPreferences(nextPreferences);
  }

  private resolveInitialPreferences(): UserPreferencesState {
    try {
      const persistedPreferences = globalThis.localStorage?.getItem(USER_PREFERENCES_STORAGE_KEY);

      if (!persistedPreferences) {
        return DEFAULT_USER_PREFERENCES;
      }

      const parsedPreferences = JSON.parse(persistedPreferences) as Partial<UserPreferencesState>;

      return {
        bicepViewerTheme: this.normalizeBicepViewerTheme(parsedPreferences.bicepViewerTheme),
        appTheme: this.normalizeAppTheme(parsedPreferences.appTheme),
      };
    } catch {
      return DEFAULT_USER_PREFERENCES;
    }
  }

  private normalizeBicepViewerTheme(theme: string | undefined): BicepViewerTheme {
    if (theme && BICEP_VIEWER_THEMES.has(theme as BicepViewerTheme)) {
      return theme as BicepViewerTheme;
    }

    return DEFAULT_BICEP_VIEWER_THEME;
  }

  private normalizeAppTheme(theme: string | undefined): AppTheme {
    if (theme && APP_THEMES.has(theme as AppTheme)) {
      return theme as AppTheme;
    }

    return DEFAULT_APP_THEME;
  }

  private persistPreferences(preferences: UserPreferencesState): void {
    try {
      globalThis.localStorage?.setItem(USER_PREFERENCES_STORAGE_KEY, JSON.stringify(preferences));
    } catch {
      // Access to localStorage can fail in restricted browser contexts.
    }
  }
}