import { Component, computed, ElementRef, inject, viewChild } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { DsButtonComponent, DsSelectComponent } from '../../../shared/components/ds';
import { MatChipsModule } from '@angular/material/chips';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import { RESOURCE_TYPE_OPTIONS } from '../enums/resource-type.enum';
import {
  buildNamingTemplateForm,
  computeFilteredResourceTypeOptions,
  extractNamingTemplateResult,
  formatPlaceholder,
  insertPlaceholderAtCursor,
  NAMING_TEMPLATE_PLACEHOLDERS,
} from '../../../shared/utils/naming-template-dialog.utils';

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
  templateUrl: './add-naming-template-dialog.component.html',
  styleUrl: './add-naming-template-dialog.component.scss',
})
export class AddNamingTemplateDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AddNamingTemplateDialogComponent>);
  private readonly data: AddNamingTemplateDialogData = inject(MAT_DIALOG_DATA);
  private readonly fb = inject(FormBuilder);

  private readonly templateInput = viewChild<ElementRef<HTMLInputElement>>('templateInput');

  protected readonly isResourceMode = this.data.mode === 'resource';
  protected readonly isEditMode = this.data.isEditMode;

  protected readonly placeholders = NAMING_TEMPLATE_PLACEHOLDERS;

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
