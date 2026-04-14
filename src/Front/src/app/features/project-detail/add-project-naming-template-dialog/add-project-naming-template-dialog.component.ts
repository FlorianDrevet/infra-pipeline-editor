import { Component, computed, ElementRef, inject, viewChild } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatOptionModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import { RESOURCE_TYPE_OPTIONS } from '../../config-detail/enums/resource-type.enum';

export type ProjectNamingTemplateDialogMode = 'default' | 'resource';

export interface AddProjectNamingTemplateDialogData {
  mode: ProjectNamingTemplateDialogMode;
  isEditMode: boolean;
  template?: string | null;
  resourceType?: string;
  availableResourceTypes?: string[];
}

export interface AddProjectNamingTemplateDialogResult {
  template: string;
  resourceType?: string;
}

@Component({
  selector: 'app-add-project-naming-template-dialog',
  standalone: true,
  imports: [
    MatButtonModule,
    MatChipsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatOptionModule,
    MatSelectModule,
    MatTooltipModule,
    ReactiveFormsModule,
    TranslateModule,
  ],
  templateUrl: './add-project-naming-template-dialog.component.html',
  styleUrl: './add-project-naming-template-dialog.component.scss',
})
export class AddProjectNamingTemplateDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AddProjectNamingTemplateDialogComponent>);
  private readonly data: AddProjectNamingTemplateDialogData = inject(MAT_DIALOG_DATA);
  private readonly fb = inject(FormBuilder);

  private readonly templateInput = viewChild<ElementRef<HTMLInputElement>>('templateInput');

  protected readonly isResourceMode = this.data.mode === 'resource';
  protected readonly isEditMode = this.data.isEditMode;

  protected readonly placeholders: readonly string[] = [
    'name',
    'prefix',
    'suffix',
    'env',
    'envShort',
    'resourceType',
    'resourceAbbr',
    'location',
  ];

  protected readonly dialogTitleKey = computed(() => {
    if (this.isResourceMode) {
      return this.isEditMode
        ? 'PROJECT_DETAIL.NAMING_TEMPLATES.RESOURCE_EDIT_DIALOG_TITLE'
        : 'PROJECT_DETAIL.NAMING_TEMPLATES.RESOURCE_ADD_DIALOG_TITLE';
    }
    return this.isEditMode
      ? 'PROJECT_DETAIL.NAMING_TEMPLATES.DEFAULT_EDIT_DIALOG_TITLE'
      : 'PROJECT_DETAIL.NAMING_TEMPLATES.DEFAULT_ADD_DIALOG_TITLE';
  });

  protected readonly submitKey = this.isEditMode
    ? 'PROJECT_DETAIL.NAMING_TEMPLATES.FORM.SAVE'
    : 'PROJECT_DETAIL.NAMING_TEMPLATES.FORM.ADD';

  protected readonly resourceTypeOptions = computed(() => {
    if (!this.isResourceMode || this.isEditMode) {
      return RESOURCE_TYPE_OPTIONS;
    }
    const allowedResourceTypes = new Set(this.data.availableResourceTypes ?? []);
    return RESOURCE_TYPE_OPTIONS.filter((option) => allowedResourceTypes.has(option.value));
  });

  protected readonly form = this.fb.group({
    resourceType: [
      this.data.resourceType ?? '',
      this.isResourceMode ? [Validators.required] : [],
    ],
    template: [
      this.data.template ?? '',
      [Validators.required, Validators.minLength(1), Validators.maxLength(500)],
    ],
  });

  protected formatPlaceholder(placeholder: string): string {
    return `{${placeholder}}`;
  }

  protected insertPlaceholder(placeholder: string): void {
    const token = this.formatPlaceholder(placeholder);
    const inputEl = this.templateInput()?.nativeElement;
    const ctrl = this.form.controls.template;
    const current = ctrl.value ?? '';

    if (inputEl) {
      const start = inputEl.selectionStart ?? current.length;
      const end = inputEl.selectionEnd ?? start;
      const newValue = current.slice(0, start) + token + current.slice(end);
      ctrl.setValue(newValue);
      const cursorPos = start + token.length;
      requestAnimationFrame(() => {
        inputEl.focus();
        inputEl.setSelectionRange(cursorPos, cursorPos);
      });
    } else {
      ctrl.setValue(current + token);
    }
  }

  protected onCancel(): void {
    this.dialogRef.close(null);
  }

  protected onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const values = this.form.getRawValue();
    const template = values.template?.trim();

    if (!template) {
      this.form.controls.template.setErrors({ required: true });
      return;
    }

    const result: AddProjectNamingTemplateDialogResult = { template };

    if (this.isResourceMode) {
      result.resourceType = values.resourceType ?? undefined;
    }

    this.dialogRef.close(result);
  }
}
