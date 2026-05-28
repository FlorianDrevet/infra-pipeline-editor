import { Signal } from '@angular/core';
import { FormControl } from '@angular/forms';

import { TagRequest } from '../../../../shared/interfaces/infra-config.interface';
import { DsKeyValueItem } from '../../../../shared/components/ds';

export interface ConfigDetailTagsSection {
  readonly configTags: Signal<TagRequest[]>;
  readonly isEditing: Signal<boolean>;
  readonly editingTags: Signal<TagRequest[]>;
  readonly editingKvItems: Signal<DsKeyValueItem[]>;
  readonly errorKey: Signal<string>;
  readonly isSaving: Signal<boolean>;
  readonly tagNameControl: FormControl<string>;
  readonly tagValueControl: FormControl<string>;

  reset(): void;
  startEdit(): void;
  addTag(): void;
  removeTag(name: string): void;
  cancelEdit(): void;
  save(): Promise<void>;
  onItemsChange(items: DsKeyValueItem[]): void;
}