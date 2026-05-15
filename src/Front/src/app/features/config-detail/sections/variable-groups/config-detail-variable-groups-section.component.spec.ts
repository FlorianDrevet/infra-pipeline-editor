import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { ConfigDetailVariableGroupsSectionComponent } from './config-detail-variable-groups-section.component';

describe('ConfigDetailVariableGroupsSectionComponent', () => {
  let fixture: ComponentFixture<ConfigDetailVariableGroupsSectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConfigDetailVariableGroupsSectionComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfigDetailVariableGroupsSectionComponent);
  });

  it('shows the empty state when there are no variable groups', () => {
    fixture.componentRef.setInput('canWrite', true);
    fixture.componentRef.setInput('section', createSection({ isLoaded: signal(true) }));
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.empty-state')).not.toBeNull();
  });

  it('delegates the add action to the section controller', () => {
    const openAddDialog = jasmine.createSpy('openAddDialog');

    fixture.componentRef.setInput('canWrite', true);
    fixture.componentRef.setInput('section', createSection({ openAddDialog }));
    fixture.detectChanges();

    (fixture.nativeElement.querySelector('.add-rg-btn') as HTMLButtonElement).click();

    expect(openAddDialog).toHaveBeenCalledOnceWith();
  });
});

function createSection(overrides: Partial<Record<string, unknown>> = {}): Record<string, unknown> {
  return {
    configVariableGroups: signal([]),
    isLoading: signal(false),
    errorKey: signal(''),
    isLoaded: signal(false),
    reset: () => undefined,
    load: async () => undefined,
    openAddDialog: () => undefined,
    openRemoveDialog: () => undefined,
    ...overrides,
  };
}