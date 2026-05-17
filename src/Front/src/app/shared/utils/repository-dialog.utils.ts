import {
  AbstractControl,
  FormArray,
  FormBuilder,
  FormControl,
  FormGroup,
  Validators,
} from '@angular/forms';
import { DsSelectOption } from '../components/ds';
import { RepositoryContentKind } from '../interfaces/project-repository.interface';

export const PROVIDER_OPTIONS: DsSelectOption[] = [
  { value: 'AzureDevOps', label: 'Azure DevOps' },
  { value: 'GitHub', label: 'GitHub' },
  { value: 'GitLab', label: 'GitLab' },
  { value: 'Bitbucket', label: 'Bitbucket' },
];

export const CONTENT_KINDS: ReadonlyArray<RepositoryContentKind> = [
  'Infrastructure',
  'ApplicationCode',
];

export interface RepositoryFormExisting {
  alias?: string;
  providerType?: string;
  repositoryUrl?: string;
  defaultBranch?: string;
  contentKinds?: RepositoryContentKind[];
}

export type RepositoryFormGroup = FormGroup<{
  alias: FormControl<string>;
  providerType: FormControl<string>;
  repositoryUrl: FormControl<string>;
  defaultBranch: FormControl<string>;
  contentKinds: FormArray<FormControl<boolean>>;
}>;

export function buildRepositoryForm(
  fb: FormBuilder,
  isEditMode: boolean,
  existing: RepositoryFormExisting | undefined,
  lockedKinds: ReadonlyArray<RepositoryContentKind>,
): RepositoryFormGroup {
  return fb.group({
    alias: new FormControl<string>(
      { value: existing?.alias ?? '', disabled: isEditMode },
      {
        nonNullable: true,
        validators: [Validators.required, Validators.pattern(/^[a-z0-9-]+$/)],
      }
    ),
    providerType: new FormControl<string>(
      existing?.providerType ?? 'AzureDevOps',
      { nonNullable: true, validators: [Validators.required] }
    ),
    repositoryUrl: new FormControl<string>(
      existing?.repositoryUrl ?? '',
      { nonNullable: true, validators: [Validators.required] }
    ),
    defaultBranch: new FormControl<string>(
      existing?.defaultBranch ?? 'main',
      { nonNullable: true, validators: [Validators.required] }
    ),
    contentKinds: fb.array<FormControl<boolean>>(
      CONTENT_KINDS.map((kind) => {
        const isLocked = lockedKinds.includes(kind);
        const initialChecked = isLocked
          ? true
          : (existing?.contentKinds?.includes(kind) ?? false);
        return new FormControl<boolean>(
          { value: initialChecked, disabled: isLocked },
          { nonNullable: true }
        );
      }),
      [atLeastOneChecked()]
    ),
  }) as RepositoryFormGroup;
}

export function getSelectedContentKinds(
  rawContentKinds: boolean[],
  lockedKinds: ReadonlyArray<RepositoryContentKind>,
): RepositoryContentKind[] {
  return CONTENT_KINDS.filter(
    (kind, idx) => rawContentKinds[idx] || lockedKinds.includes(kind)
  );
}

export function atLeastOneChecked() {
  return (control: AbstractControl) => {
    const arr = control as FormArray<FormControl<boolean>>;
    const any = arr.controls.some((c) => c.value === true);
    return any ? null : { required: true };
  };
}
