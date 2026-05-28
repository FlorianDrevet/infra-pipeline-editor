import { computed, inject, signal } from '@angular/core';
import { FormControl } from '@angular/forms';

import { TagRequest } from '../../../../shared/interfaces/infra-config.interface';
import { DsKeyValueItem } from '../../../../shared/components/ds';
import { InfraConfigService } from '../../../../shared/services/infra-config.service';
import { ConfigDetailTagsSection } from './config-detail-tags-section.interface';

interface ConfigDetailTagsSectionControllerDependencies {
  getConfigId(): string | null;
  getConfigTags(): TagRequest[];
  updateConfigTags(tags: TagRequest[]): void;
}

export function createConfigDetailTagsSectionController(
  dependencies: ConfigDetailTagsSectionControllerDependencies,
): ConfigDetailTagsSection {
  const infraConfigService = inject(InfraConfigService);

  const isEditing = signal(false);
  const editingTags = signal<TagRequest[]>([]);
  const errorKey = signal('');
  const isSaving = signal(false);
  const tagNameControl = new FormControl('', { nonNullable: true });
  const tagValueControl = new FormControl('', { nonNullable: true });
  const configTags = computed(() => dependencies.getConfigTags());

  const reset = (): void => {
    isEditing.set(false);
    editingTags.set([]);
    errorKey.set('');
    isSaving.set(false);
    tagNameControl.reset();
    tagValueControl.reset();
  };

  const startEdit = (): void => {
    editingTags.set(configTags().map((tag) => ({ name: tag.name, value: tag.value })));
    tagNameControl.reset();
    tagValueControl.reset();
    errorKey.set('');
    isEditing.set(true);
  };

  const addTag = (): void => {
    const name = tagNameControl.value.trim();
    const value = tagValueControl.value.trim();
    if (!name) {
      return;
    }

    editingTags.update((currentTags) => [
      ...currentTags.filter((tag) => tag.name !== name),
      { name, value },
    ]);
    tagNameControl.reset();
    tagValueControl.reset();
  };

  const removeTag = (name: string): void => {
    editingTags.update((currentTags) => currentTags.filter((tag) => tag.name !== name));
  };

  const cancelEdit = (): void => {
    isEditing.set(false);
    editingTags.set([]);
    errorKey.set('');
  };

  const save = async (): Promise<void> => {
    const configId = dependencies.getConfigId();
    if (!configId || isSaving()) {
      return;
    }

    isSaving.set(true);
    errorKey.set('');
    try {
      await infraConfigService.setTags(configId, { tags: editingTags() });
      dependencies.updateConfigTags(editingTags());
      isEditing.set(false);
      editingTags.set([]);
    } catch {
      errorKey.set('CONFIG_DETAIL.TAGS.SAVE_ERROR');
    } finally {
      isSaving.set(false);
    }
  };

  const onItemsChange = (items: DsKeyValueItem[]): void => {
    editingTags.set(items.map(item => ({ name: item.key, value: item.value })));
  };

  const editingKvItems = computed<DsKeyValueItem[]>(() =>
    editingTags().map(t => ({ key: t.name, value: t.value })),
  );

  return {
    configTags,
    isEditing,
    editingTags,
    editingKvItems,
    errorKey,
    isSaving,
    tagNameControl,
    tagValueControl,
    reset,
    startEdit,
    addTag,
    removeTag,
    cancelEdit,
    save,
    onItemsChange,
  };
}