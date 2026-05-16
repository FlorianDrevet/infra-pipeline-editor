import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DsSectionHeaderComponent } from './ds-section-header.component';

describe('DsSectionHeaderComponent', () => {
  let fixture: ComponentFixture<DsSectionHeaderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsSectionHeaderComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsSectionHeaderComponent);
  });

  it('renders the icon and title inside the same title row when a subtitle is present', () => {
    fixture.componentRef.setInput('title', 'Environment settings');
    fixture.componentRef.setInput('icon', 'settings');
    fixture.componentRef.setInput('subtitle', 'Configure defaults for all environments');
    fixture.detectChanges();

    const titleRow = getTitleRow();
    const rowIcon = titleRow?.querySelector('.ds-section-header__icon') as HTMLElement | null;
    const rowTitle = titleRow?.querySelector('.ds-section-header__title') as HTMLElement | null;
    const subtitle = fixture.nativeElement.querySelector('.ds-section-header__subtitle') as HTMLElement | null;

    expect(titleRow).withContext('title row should exist').not.toBeNull();
    expect(rowIcon).withContext('icon should be part of the title row').not.toBeNull();
    expect(rowTitle?.textContent?.trim()).toBe('Environment settings');
    expect(subtitle).withContext('subtitle should still render').not.toBeNull();
    expect(titleRow?.contains(subtitle)).withContext('subtitle should stay outside the title row').toBeFalse();
  });

  function getTitleRow(): HTMLElement | null {
    return fixture.nativeElement.querySelector('.ds-section-header__title-row') as HTMLElement | null;
  }
});