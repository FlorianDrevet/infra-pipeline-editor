import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DsPageHeaderComponent } from './ds-page-header.component';

describe('DsPageHeaderComponent', () => {
  let fixture: ComponentFixture<DsPageHeaderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsPageHeaderComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsPageHeaderComponent);
  });

  it('renders the icon and title inside the same title row when a subtitle is present', () => {
    fixture.componentRef.setInput('title', 'Projects');
    fixture.componentRef.setInput('icon', 'folder_open');
    fixture.componentRef.setInput('subtitle', 'Track active infrastructure projects');
    fixture.detectChanges();

    const titleRow = getTitleRow();
    const rowIcon = titleRow?.querySelector('.ds-page-header__icon') as HTMLElement | null;
    const rowTitle = titleRow?.querySelector('.ds-page-header__title') as HTMLElement | null;
    const subtitle = fixture.nativeElement.querySelector('.ds-page-header__subtitle') as HTMLElement | null;

    expect(titleRow).withContext('title row should exist').not.toBeNull();
    expect(rowIcon).withContext('icon should be part of the title row').not.toBeNull();
    expect(rowTitle?.textContent?.trim()).toBe('Projects');
    expect(subtitle).withContext('subtitle should still render').not.toBeNull();
    expect(titleRow?.contains(subtitle)).withContext('subtitle should stay outside the title row').toBeFalse();
  });

  function getTitleRow(): HTMLElement | null {
    return fixture.nativeElement.querySelector('.ds-page-header__title-row') as HTMLElement | null;
  }
});