import { Component, computed, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatOptionModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { TranslateModule } from '@ngx-translate/core';
import { RESOURCE_TYPE_OPTIONS } from '../enums/resource-type.enum';

export type NamingTemplateDialogMode = 'default' | 'resource';

export interface AddNamingTemplateDialogData {
  mode: NamingTemplateDialogMode;
  isEditMode: boolean;
  template?: string | null;
  resourceType?: string;
  availableResourceTypes?: string[];
}

export interface AddNamingTemplateDialogResult {
  template: string;
  resourceType?: string;
}

@Component({
  selector: 'app-add-naming-template-dialog',
  standalone: true,
  imports: [
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatOptionModule,
    MatSelectModule,
    ReactiveFormsModule,
    TranslateModule,
  ],
  templateUrl: './add-naming-template-dialog.component.html',
  styleUrl: './add-naming-template-dialog.component.scss',
})
export class AddNamingTemplateDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AddNamingTemplateDialogComponent>);
  private readonly data: AddNamingTemplateDialogData = inject(MAT_DIALOG_DATA);
  private readonly fb = inject(FormBuilder);

  protected readonly isResourceMode = this.data.mode === 'resource';
  protected readonly isEditMode = this.data.isEditMode;

  protected readonly dialogTitleKey = computed(() => {
    if (this.isResourceMode) {
      return this.isEditMode
        ? 'CONFIG_DETAIL.NAMING_TEMPLATES.RESOURCE_EDIT_DIALOG_TITLE'
        : 'CONFIG_DETAIL.NAMING_TEMPLATES.RESOURCE_ADD_DIALOG_TITLE';
    }

    return this.isEditMode
      ? 'CONFIG_DETAIL.NAMING_TEMPLATES.DEFAULT_EDIT_DIALOG_TITLE'
      : 'CONFIG_DETAIL.NAMING_TEMPLATES.DEFAULT_ADD_DIALOG_TITLE';
  });

  protected readonly submitKey = this.isEditMode
    ? 'CONFIG_DETAIL.NAMING_TEMPLATES.FORM.SAVE'
    : 'CONFIG_DETAIL.NAMING_TEMPLATES.FORM.ADD';

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

    const result: AddNamingTemplateDialogResult = { template };

    if (this.isResourceMode) {
      result.resourceType = values.resourceType ?? undefined;
    }

    this.dialogRef.close(result);
  }
}
