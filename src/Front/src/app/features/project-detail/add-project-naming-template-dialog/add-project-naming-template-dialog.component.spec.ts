import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';

import {
  AddProjectNamingTemplateDialogComponent,
  AddProjectNamingTemplateDialogData,
} from './add-project-naming-template-dialog.component';

describe('AddProjectNamingTemplateDialogComponent', () => {
  let fixture: ComponentFixture<AddProjectNamingTemplateDialogComponent>;
  let component: AddProjectNamingTemplateDialogComponent;
  let componentTestApi: { form: { controls: { template: { value: string | null } } } };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddProjectNamingTemplateDialogComponent, TranslateModule.forRoot()],
      providers: [
        {
          provide: MAT_DIALOG_DATA,
          useValue: {
            mode: 'default',
            isEditMode: false,
          } satisfies AddProjectNamingTemplateDialogData,
        },
        {
          provide: MatDialogRef,
          useValue: jasmine.createSpyObj<MatDialogRef<AddProjectNamingTemplateDialogComponent>>('MatDialogRef', ['close']),
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AddProjectNamingTemplateDialogComponent);
    component = fixture.componentInstance;
    componentTestApi = component as unknown as { form: { controls: { template: { value: string | null } } } };
    fixture.detectChanges();
  });

  it('inserts a placeholder when the chip is activated from the keyboard', () => {
    getFirstPlaceholderChip().dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', bubbles: true, cancelable: true }));
    fixture.detectChanges();

    expect(componentTestApi.form.controls.template.value).toBe('{name}');
  });

  function getFirstPlaceholderChip(): HTMLElement {
    return fixture.nativeElement.querySelector('.placeholder-chip') as HTMLElement;
  }
});