import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';

import { LanguageService } from '../../shared/services/language.service';
import {
  AppTheme,
  AppThemeOption,
  BicepViewerTheme,
  BicepViewerThemeOption,
  UserPreferencesService,
} from '../../shared/services/user-preferences.service';
import { PersonalAccessTokenResponse } from '../../shared/interfaces/personal-access-token.interface';
import { PersonalAccessTokenService } from '../../shared/services/personal-access-token.service';
import { SettingsComponent } from './settings.component';

const ACTIVE_TOKEN: PersonalAccessTokenResponse = {
  id: 'pat-01',
  name: 'Terraform Bot',
  tokenPrefix: 'ifs_pat',
  createdAt: '2026-05-01T12:00:00.000Z',
  lastUsedAt: '2026-05-12T08:00:00.000Z',
  expiresAt: '2026-06-01T12:00:00.000Z',
  isRevoked: false,
};

describe('SettingsComponent', () => {
  let fixture: ComponentFixture<SettingsComponent>;
  let patServiceSpy: jasmine.SpyObj<PersonalAccessTokenService>;
  let userPreferencesServiceStub: UserPreferencesServiceStub;

  beforeEach(async () => {
    patServiceSpy = jasmine.createSpyObj<PersonalAccessTokenService>('PersonalAccessTokenService', ['getAll', 'revoke']);
    patServiceSpy.getAll.and.resolveTo([ACTIVE_TOKEN]);

    userPreferencesServiceStub = new UserPreferencesServiceStub();

    await TestBed.configureTestingModule({
      imports: [SettingsComponent, TranslateModule.forRoot()],
      providers: [
        {
          provide: PersonalAccessTokenService,
          useValue: patServiceSpy,
        },
        {
          provide: MatDialog,
          useValue: jasmine.createSpyObj<MatDialog>('MatDialog', ['open']),
        },
        {
          provide: LanguageService,
          useValue: {
            currentLanguage: signal<'fr' | 'en'>('fr').asReadonly(),
            setLanguage: jasmine.createSpy('setLanguage'),
          } satisfies Pick<LanguageService, 'currentLanguage' | 'setLanguage'>,
        },
        {
          provide: UserPreferencesService,
          useValue: userPreferencesServiceStub,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SettingsComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  });

  it('renders a Bicep preview block for each available viewer theme', () => {
    const preferenceRow = fixture.nativeElement.querySelector(
      '[data-preference="bicep-viewer-theme"]',
    ) as HTMLElement | null;
    const previewBlocks = fixture.nativeElement.querySelectorAll('[data-bicep-theme-preview]');

    expect(preferenceRow).not.toBeNull();
    expect(preferenceRow?.querySelectorAll('[data-bicep-theme-option]').length).toBe(3);
    expect(previewBlocks.length).toBe(3);
    expect(
      preferenceRow?.querySelector('[data-bicep-theme-preview="azure-night"] .theme-option__code-line'),
    ).not.toBeNull();
    expect(
      preferenceRow?.querySelector('[data-bicep-theme-option="azure-night"]')?.classList.contains('theme-option--active'),
    ).toBeTrue();
  });

  it('delegates Bicep viewer theme changes through the preferences service', () => {
    const graphiteButton = fixture.nativeElement.querySelector(
      '[data-bicep-theme-option="graphite-frost"]',
    ) as HTMLButtonElement;

    graphiteButton.click();
    fixture.detectChanges();

    expect(userPreferencesServiceStub.setBicepViewerTheme).toHaveBeenCalledOnceWith('graphite-frost');
    expect(graphiteButton.classList.contains('theme-option--active')).toBeTrue();
  });

  it('renders the PAT list with the design-system table instead of legacy ifs-table markup', () => {
    const dsTable = fixture.nativeElement.querySelector('app-ds-table');
    const legacyHeader = fixture.nativeElement.querySelector('.ifs-table__header');

    expect(dsTable).not.toBeNull();
    expect(legacyHeader).toBeNull();
    expect(dsTable?.textContent).toContain('Terraform Bot');
  });
});

class UserPreferencesServiceStub {
  private readonly selectedTheme = signal<BicepViewerTheme>('azure-night');
  private readonly selectedAppTheme = signal<AppTheme>('dark');

  readonly bicepViewerTheme = this.selectedTheme.asReadonly();
  readonly appTheme = this.selectedAppTheme.asReadonly();
  readonly bicepViewerThemeOptions: readonly BicepViewerThemeOption[] = [
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
  readonly appThemeOptions: readonly AppThemeOption[] = [
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

  readonly setBicepViewerTheme = jasmine
    .createSpy('setBicepViewerTheme')
    .and.callFake((theme: BicepViewerTheme) => {
      this.selectedTheme.set(theme);
    });

  readonly setAppTheme = jasmine
    .createSpy('setAppTheme')
    .and.callFake((theme: AppTheme) => {
      this.selectedAppTheme.set(theme);
    });
}