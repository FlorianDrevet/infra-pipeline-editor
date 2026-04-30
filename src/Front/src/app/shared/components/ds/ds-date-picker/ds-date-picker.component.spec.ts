import { OverlayContainer } from '@angular/cdk/overlay';
import { signal, WritableSignal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { LanguageService } from '../../../services/language.service';
import { DsDatePickerComponent } from './ds-date-picker.component';

interface DsDatePickerComponentTestApi {
  viewDate: () => Date;
  viewMode: () => 'days' | 'months' | 'years';
}

const DATE_PICKER_TRANSLATIONS = {
  COMMON: {
    DATE_PICKER: {
      ARIA: {
        CLEAR: 'Clear date',
        PREVIOUS_YEAR: 'Previous year',
        OPEN_YEAR_SELECTION: 'Open year selection',
        NEXT_YEAR: 'Next year',
        PREVIOUS_MONTH: 'Previous month',
        OPEN_MONTH_SELECTION: 'Open month selection',
        NEXT_MONTH: 'Next month',
        PREVIOUS_YEAR_RANGE: 'Previous year range',
        RETURN_TO_DAYS: 'Return to calendar days',
        NEXT_YEAR_RANGE: 'Next year range',
        YEAR_SELECTION: 'Year selection',
        MONTH_SELECTION: 'Month selection',
        CHOOSE_YEAR: 'Choose year {{year}}',
        CHOOSE_MONTH: 'Choose month {{month}}',
      },
      HINTS: {
        SELECT_YEAR: 'Select year',
        SELECT_MONTH: 'Select month',
      },
    },
  },
} as const;

const DATE_PICKER_TRANSLATIONS_FR = {
  COMMON: {
    DATE_PICKER: {
      ARIA: {
        CLEAR: 'Effacer la date',
        PREVIOUS_YEAR: 'Annee precedente',
        OPEN_YEAR_SELECTION: 'Ouvrir la selection de l annee',
        NEXT_YEAR: 'Annee suivante',
        PREVIOUS_MONTH: 'Mois precedent',
        OPEN_MONTH_SELECTION: 'Ouvrir la selection du mois',
        NEXT_MONTH: 'Mois suivant',
        PREVIOUS_YEAR_RANGE: 'Plage d annees precedente',
        RETURN_TO_DAYS: 'Retour au calendrier',
        NEXT_YEAR_RANGE: 'Plage d annees suivante',
        YEAR_SELECTION: 'Selection de l annee',
        MONTH_SELECTION: 'Selection du mois',
        CHOOSE_YEAR: 'Choisir l annee {{year}}',
        CHOOSE_MONTH: 'Choisir le mois {{month}}',
      },
      HINTS: {
        SELECT_YEAR: 'Selectionner l annee',
        SELECT_MONTH: 'Selectionner le mois',
      },
    },
  },
} as const;

describe('DsDatePickerComponent', () => {
  let fixture: ComponentFixture<DsDatePickerComponent>;
  let component: DsDatePickerComponent;
  let overlayContainer: OverlayContainer;
  let overlayElement: HTMLElement;
  let translateService: TranslateService;
  let componentTestApi: DsDatePickerComponentTestApi;
  let currentLanguage: WritableSignal<'en' | 'fr'>;

  beforeEach(async () => {
    currentLanguage = signal<'en' | 'fr'>('en');

    await TestBed.configureTestingModule({
      imports: [DsDatePickerComponent, TranslateModule.forRoot()],
      providers: [
        {
          provide: LanguageService,
          useValue: {
            currentLanguage: currentLanguage.asReadonly(),
          },
        },
      ],
    }).compileComponents();

    overlayContainer = TestBed.inject(OverlayContainer);
    overlayElement = overlayContainer.getContainerElement();
    translateService = TestBed.inject(TranslateService);
    translateService.setFallbackLang('en');
    translateService.setTranslation('en', DATE_PICKER_TRANSLATIONS);
    translateService.setTranslation('fr', DATE_PICKER_TRANSLATIONS_FR);
    translateService.use('en');

    fixture = TestBed.createComponent(DsDatePickerComponent);
    component = fixture.componentInstance;
    componentTestApi = component as unknown as DsDatePickerComponentTestApi;
    component.writeValue('2032-06-18');
    fixture.detectChanges();
    await fixture.whenStable();
  });

  afterEach(() => {
    overlayContainer.ngOnDestroy();
  });

  it('renders year options when toggling year mode from the header year label', async () => {
    await openPicker();

    getOverlayButton('.ds-date-picker__year-label-button').click();
    fixture.detectChanges();
    await fixture.whenStable();

    const years = Array.from(overlayElement.querySelectorAll<HTMLButtonElement>('.ds-date-picker__year'));

    expect(years.length).toBe(12);
    expect(years.some((button) => button.textContent?.trim() === '2032')).toBeTrue();
  });

  it('updates the visible year and returns to day mode when a year is selected', async () => {
    await openPicker();

    getOverlayButton('.ds-date-picker__year-label-button').click();
    fixture.detectChanges();
    await fixture.whenStable();

    const yearButton = Array.from(overlayElement.querySelectorAll<HTMLButtonElement>('.ds-date-picker__year'))
      .find((button) => button.textContent?.trim() === '2035');

    expect(yearButton).toBeTruthy();

    yearButton?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(componentTestApi.viewDate().getFullYear()).toBe(2035);
    expect(componentTestApi.viewMode()).toBe('days');
  });

  it('renders month options when toggling month mode from the header month label', async () => {
    await openPicker();

    getOverlayButton('.ds-date-picker__month-label-button').click();
    fixture.detectChanges();
    await fixture.whenStable();

    const months = Array.from(overlayElement.querySelectorAll<HTMLButtonElement>('.ds-date-picker__month'));

    expect(months.length).toBe(12);
  });

  it('updates the visible month and returns to day mode when a month is selected', async () => {
    await openPicker();

    getOverlayButton('.ds-date-picker__month-label-button').click();
    fixture.detectChanges();
    await fixture.whenStable();

    const monthButtons = Array.from(overlayElement.querySelectorAll<HTMLButtonElement>('.ds-date-picker__month'));

    expect(monthButtons.length).toBe(12);

    monthButtons[8].click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(componentTestApi.viewDate().getMonth()).toBe(8);
    expect(componentTestApi.viewMode()).toBe('days');
  });

  it('updates the visible year from the day header year navigation', async () => {
    await openPicker();

    expect(getOverlayButton('.ds-date-picker__year-label-button').textContent?.trim()).toBe('2032');

    getOverlayButton('.ds-date-picker__year-nav-button--next').click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(getOverlayButton('.ds-date-picker__year-label-button').textContent?.trim()).toBe('2033');
    expect(componentTestApi.viewDate().getFullYear()).toBe(2033);
  });

  it('clamps next year navigation to the nearest allowed month in the target year', async () => {
    fixture.componentRef.setInput('max', '2033-04-30');
    component.writeValue('2032-05-01');
    fixture.detectChanges();
    await fixture.whenStable();

    await openPicker();

    const nextYearButton = getOverlayButton('.ds-date-picker__year-nav-button--next');

    expect(nextYearButton.disabled).toBeFalse();

    nextYearButton.click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(componentTestApi.viewDate().getFullYear()).toBe(2033);
    expect(componentTestApi.viewDate().getMonth()).toBe(3);
  });

  it('renders distinct aligned header rows for year and month navigation in day mode', async () => {
    await openPicker();

    const yearRow = overlayElement.querySelector<HTMLElement>('.ds-date-picker__header-row--year');
    const monthRow = overlayElement.querySelector<HTMLElement>('.ds-date-picker__header-row--month');

    expect(yearRow).withContext('year navigation row should exist').not.toBeNull();
    expect(monthRow).withContext('month navigation row should exist').not.toBeNull();
    expect(yearRow?.querySelector('.ds-date-picker__year-nav-button--previous')).not.toBeNull();
    expect(yearRow?.querySelector('.ds-date-picker__year-label-button')).not.toBeNull();
    expect(yearRow?.querySelector('.ds-date-picker__year-nav-button--next')).not.toBeNull();
    expect(monthRow?.querySelector('.ds-date-picker__month-nav-button--previous')).not.toBeNull();
    expect(monthRow?.querySelector('.ds-date-picker__month-label-button')).not.toBeNull();
    expect(monthRow?.querySelector('.ds-date-picker__month-nav-button--next')).not.toBeNull();
  });

  it('disables next year navigation when the target month is after the configured max date', async () => {
    fixture.componentRef.setInput('max', '2032-12-31');
    fixture.detectChanges();

    await openPicker();

    expect(getOverlayButton('.ds-date-picker__year-nav-button--next').disabled).toBeTrue();
  });

  it('disables impossible months in month selection mode when min and max constrain the current year', async () => {
    fixture.componentRef.setInput('min', '2032-03-01');
    fixture.componentRef.setInput('max', '2032-10-31');
    component.writeValue('2032-06-18');
    fixture.detectChanges();
    await fixture.whenStable();

    await openPicker();

    getOverlayButton('.ds-date-picker__month-label-button').click();
    fixture.detectChanges();
    await fixture.whenStable();

    const monthButtons = Array.from(overlayElement.querySelectorAll<HTMLButtonElement>('.ds-date-picker__month'));

    expect(monthButtons[0].disabled).toBeTrue();
    expect(monthButtons[2].disabled).toBeFalse();
    expect(monthButtons[9].disabled).toBeFalse();
    expect(monthButtons[10].disabled).toBeTrue();
  });

  it('renders translated internal copy and date labels using the active app language', async () => {
    currentLanguage.set('fr');
    translateService.use('fr');
    fixture.detectChanges();
    await fixture.whenStable();

    await openPicker();

    expect(overlayElement.querySelector('.ds-date-picker__month-label')?.textContent?.trim().toLowerCase()).toContain('juin');

    getOverlayButton('.ds-date-picker__year-label-button').click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(overlayElement.querySelector('.ds-date-picker__header-hint')?.textContent?.trim()).toBe('Selectionner l annee');
  });

  async function openPicker(): Promise<void> {
    const host = fixture.nativeElement as HTMLElement;
    const trigger = host.querySelector<HTMLButtonElement>('.ds-date-picker__trigger');
    expect(trigger).withContext('date picker trigger should exist').not.toBeNull();

    trigger?.click();
    fixture.detectChanges();
    await fixture.whenStable();
  }

  function getOverlayButton(selector: string): HTMLButtonElement {
    const button = overlayElement.querySelector<HTMLButtonElement>(selector);
    expect(button).withContext(`overlay button ${selector} should exist`).not.toBeNull();
    return button as HTMLButtonElement;
  }
});