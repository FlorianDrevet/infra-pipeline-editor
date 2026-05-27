import { Component, computed, ElementRef, inject, viewChild } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { DsButtonComponent, DsSelectComponent } from '../../../shared/components/ds';
import { MatChipsModule } from '@angular/material/chips';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import { RESOURCE_TYPE_OPTIONS } from '../../../shared/resource-metadata/resource-type.metadata';
import {
  buildNamingTemplateForm,
  computeFilteredResourceTypeOptions,
  extractNamingTemplateResult,
  formatPlaceholder,
  insertPlaceholderAtCursor,
  NAMING_TEMPLATE_PLACEHOLDERS,
} from '../../../shared/utils/naming-template-dialog.utils';

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
    MatChipsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatTooltipModule,
    ReactiveFormsModule,
    TranslateModule,
    DsButtonComponent,
    DsSelectComponent,
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

  protected readonly placeholders = NAMING_TEMPLATE_PLACEHOLDERS;

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

  protected readonly resourceTypeOptions = computed(() =>
    computeFilteredResourceTypeOptions(
      RESOURCE_TYPE_OPTIONS, this.isResourceMode, this.isEditMode, this.data.availableResourceTypes
    )
  );

  protected readonly form = buildNamingTemplateForm(this.fb, this.isResourceMode, this.data);

  protected formatPlaceholder(placeholder: string): string {
    return formatPlaceholder(placeholder);
  }

  protected insertPlaceholder(placeholder: string): void {
    insertPlaceholderAtCursor(placeholder, this.form.controls.template, this.templateInput());
  }

  protected onCancel(): void {
    this.dialogRef.close(null);
  }

  protected onSubmit(): void {
    const result = extractNamingTemplateResult(this.form, this.isResourceMode);
    if (result) {
      this.dialogRef.close(result);
    }
  }
}
