import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';

import { LanguageService } from '../../shared/services/language.service';
import {
  BicepViewerTheme,
  BicepViewerThemeOption,
  UserPreferencesService,
} from '../../shared/services/user-preferences.service';
import { PersonalAccessTokenService } from '../../shared/services/personal-access-token.service';
import { SettingsComponent } from './settings.component';

describe('SettingsComponent', () => {
  let fixture: ComponentFixture<SettingsComponent>;
  let patServiceSpy: jasmine.SpyObj<PersonalAccessTokenService>;
  let userPreferencesServiceStub: UserPreferencesServiceStub;

  beforeEach(async () => {
    patServiceSpy = jasmine.createSpyObj<PersonalAccessTokenService>('PersonalAccessTokenService', ['getAll', 'revoke']);
    patServiceSpy.getAll.and.resolveTo([]);

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

  it('renders the Bicep viewer theme preference with the available options', () => {
    const preferenceRow = fixture.nativeElement.querySelector(
      '[data-preference="bicep-viewer-theme"]',
    ) as HTMLElement | null;

    expect(preferenceRow).not.toBeNull();
    expect(preferenceRow?.querySelectorAll('[data-bicep-theme-option]').length).toBe(3);
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
});

class UserPreferencesServiceStub {
  private readonly selectedTheme = signal<BicepViewerTheme>('azure-night');

  readonly bicepViewerTheme = this.selectedTheme.asReadonly();
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

  readonly setBicepViewerTheme = jasmine
    .createSpy('setBicepViewerTheme')
    .and.callFake((theme: BicepViewerTheme) => {
      this.selectedTheme.set(theme);
    });
}