import { OverlayContainer } from '@angular/cdk/overlay';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DsDatePickerComponent } from './ds-date-picker.component';

describe('DsDatePickerComponent', () => {
  let fixture: ComponentFixture<DsDatePickerComponent>;
  let component: DsDatePickerComponent;
  let overlayContainer: OverlayContainer;
  let overlayElement: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsDatePickerComponent],
    }).compileComponents();

    overlayContainer = TestBed.inject(OverlayContainer);
    overlayElement = overlayContainer.getContainerElement();

    fixture = TestBed.createComponent(DsDatePickerComponent);
    component = fixture.componentInstance;
    component.writeValue('2032-06-18');
    fixture.detectChanges();
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

    expect((component as any).viewDate().getFullYear()).toBe(2035);
    expect((component as any).viewMode()).toBe('days');
  });

  it('updates the visible year from the day header year navigation', async () => {
    await openPicker();

    expect(getOverlayButton('.ds-date-picker__year-label-button').textContent?.trim()).toBe('2032');

    getOverlayButton('.ds-date-picker__year-nav-button--next').click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(getOverlayButton('.ds-date-picker__year-label-button').textContent?.trim()).toBe('2033');
    expect((component as any).viewDate().getFullYear()).toBe(2033);
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
    expect(monthRow?.querySelector('.ds-date-picker__month-label')).not.toBeNull();
    expect(monthRow?.querySelector('.ds-date-picker__month-nav-button--next')).not.toBeNull();
  });

  it('disables next year navigation when the target month is after the configured max date', async () => {
    fixture.componentRef.setInput('max', '2032-12-31');
    fixture.detectChanges();

    await openPicker();

    expect(getOverlayButton('.ds-date-picker__year-nav-button--next').disabled).toBeTrue();
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