import { ElementRef } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { DsSelectOption } from '../components/ds';

export const NAMING_TEMPLATE_PLACEHOLDERS: readonly string[] = [
  'name',
  'prefix',
  'suffix',
  'env',
  'envShort',
  'resourceType',
  'resourceAbbr',
  'location',
];

export interface NamingTemplateFormData {
  resourceType?: string;
  template?: string | null;
}

export type NamingTemplateFormGroup = FormGroup<{
  resourceType: FormControl<string | null>;
  template: FormControl<string | null>;
}>;

export function buildNamingTemplateForm(
  fb: FormBuilder,
  isResourceMode: boolean,
  data: NamingTemplateFormData,
): NamingTemplateFormGroup {
  return fb.group({
    resourceType: [
      data.resourceType ?? '',
      isResourceMode ? [Validators.required] : [],
    ],
    template: [
      data.template ?? '',
      [Validators.required, Validators.minLength(1), Validators.maxLength(500)],
    ],
  });
}

export function formatPlaceholder(placeholder: string): string {
  return `{${placeholder}}`;
}

export function insertPlaceholderAtCursor(
  placeholder: string,
  templateCtrl: FormControl<string | null>,
  inputEl: ElementRef<HTMLInputElement> | undefined,
): void {
  const token = formatPlaceholder(placeholder);
  const current = templateCtrl.value ?? '';

  if (inputEl) {
    const el = inputEl.nativeElement;
    const start = el.selectionStart ?? current.length;
    const end = el.selectionEnd ?? start;
    const newValue = current.slice(0, start) + token + current.slice(end);
    templateCtrl.setValue(newValue);
    const cursorPos = start + token.length;
    requestAnimationFrame(() => {
      el.focus();
      el.setSelectionRange(cursorPos, cursorPos);
    });
  } else {
    templateCtrl.setValue(current + token);
  }
}

export interface NamingTemplateDialogResult {
  template: string;
  resourceType?: string;
}

export function extractNamingTemplateResult(
  form: NamingTemplateFormGroup,
  isResourceMode: boolean,
): NamingTemplateDialogResult | null {
  if (form.invalid) {
    form.markAllAsTouched();
    return null;
  }

  const values = form.getRawValue();
  const template = values.template?.trim();

  if (!template) {
    form.controls.template.setErrors({ required: true });
    return null;
  }

  const result: NamingTemplateDialogResult = { template };

  if (isResourceMode) {
    result.resourceType = values.resourceType ?? undefined;
  }

  return result;
}

export function computeFilteredResourceTypeOptions(
  allOptions: DsSelectOption[],
  isResourceMode: boolean,
  isEditMode: boolean,
  availableResourceTypes: string[] | undefined,
): DsSelectOption[] {
  if (!isResourceMode || isEditMode) {
    return allOptions;
  }
  const allowed = new Set(availableResourceTypes ?? []);
  return allOptions.filter((option) => typeof option.value === 'string' && allowed.has(option.value));
}
