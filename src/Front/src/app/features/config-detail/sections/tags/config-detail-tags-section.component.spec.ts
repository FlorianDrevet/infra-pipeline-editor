import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { ConfigDetailTagsSectionComponent } from './config-detail-tags-section.component';

describe('ConfigDetailTagsSectionComponent', () => {
  let fixture: ComponentFixture<ConfigDetailTagsSectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConfigDetailTagsSectionComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfigDetailTagsSectionComponent);
  });

  it('shows the empty state when the config has no tags', () => {
    fixture.componentRef.setInput('canWrite', true);
    fixture.componentRef.setInput('section', createSection());
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.empty-state')).not.toBeNull();
  });

  it('delegates the edit action to the section controller', () => {
    const startEdit = jasmine.createSpy('startEdit');

    fixture.componentRef.setInput('canWrite', true);
    fixture.componentRef.setInput('section', createSection({ startEdit }));
    fixture.detectChanges();

    (fixture.nativeElement.querySelector('.template-action-btn--add') as HTMLButtonElement).click();

    expect(startEdit).toHaveBeenCalledOnceWith();
  });
});

function createSection(overrides: Partial<Record<string, unknown>> = {}): Record<string, unknown> {
  return {
    configTags: signal([]),
    isEditing: signal(false),
    editingTags: signal([]),
    errorKey: signal(''),
    isSaving: signal(false),
    tagNameControl: { value: '', reset: () => undefined },
    tagValueControl: { value: '', reset: () => undefined },
    reset: () => undefined,
    startEdit: () => undefined,
    addTag: () => undefined,
    removeTag: () => undefined,
    cancelEdit: () => undefined,
    save: async () => undefined,
    ...overrides,
  };
}