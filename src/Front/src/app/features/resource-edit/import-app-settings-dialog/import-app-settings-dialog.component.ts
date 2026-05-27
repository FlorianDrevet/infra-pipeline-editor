import { Component, inject, signal, computed } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { DsSpinnerComponent } from '../../../shared/components/ds/ds-spinner/ds-spinner.component';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { DsButtonComponent, DsCheckboxComponent, DsProgressBarComponent, DsSelectComponent, DsSelectOption, DsTextFieldComponent } from '../../../shared/components/ds';
import { AzureResourceResponse } from '../../../shared/interfaces/resource-group.interface';
import { AppSettingService } from '../../../shared/services/app-setting.service';
import { AppSettingResponse, OutputDefinitionResponse } from '../../../shared/interfaces/app-setting.interface';
import { ProjectPipelineVariableGroupResponse } from '../../../shared/interfaces/project.interface';
import { ProjectService } from '../../../shared/services/project.service';
import { RESOURCE_TYPE_ICONS } from '../../../shared/resource-metadata/resource-type.metadata';
import {
  buildImportAppSettingsAcceptAttribute,
  getImportAppSettingsModeDefinition,
  listImportAppSettingsModes,
  parseImportAppSettingsContent,
  resolveImportAppSettingsMode,
  resolveImportAppSettingsModeForFileName,
  type ImportAppSettingsMode,
} from './import-app-settings-parser';

// ─── Data contract ───────────────────────────────────────────────────────────

export interface ImportAppSettingsDialogData {
  resourceId: string;
  currentResourceName: string;
  siblingResources: AzureResourceResponse[];
  environments: { name: string }[];
  projectId: string;
  existingSettingNames: string[];
  resourceType: string;
  deploymentMode: string | null;
  runtimeStack: string | null;
}

// ─── Source types ────────────────────────────────────────────────────────────

type ImportSourceType = 'static' | 'output' | 'variableGroup' | 'skip';

// ─── Parsed entry ────────────────────────────────────────────────────────────

interface ImportEntry {
  key: string;
  rawValue: string;
  selected: boolean;
  sourceType: ImportSourceType;
  /** output mode */
  sourceResourceId: string | null;
  sourceOutputName: string | null;
  /** variable group mode */
  variableGroupId: string | null;
  pipelineVariableName: string;
  /** import result */
  status: 'pending' | 'importing' | 'success' | 'error' | 'duplicate';
  errorMessage: string;
}

@Component({
  selector: 'app-import-app-settings-dialog',
  standalone: true,
  imports: [
    TranslateModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    DsSpinnerComponent,
    DsProgressBarComponent,
    DsCheckboxComponent,
    MatTooltipModule,
    DsButtonComponent,
    DsSelectComponent,
    DsTextFieldComponent,
  ],
  templateUrl: './import-app-settings-dialog.component.html',
  styleUrl: './import-app-settings-dialog.component.scss',
})
export class ImportAppSettingsDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<ImportAppSettingsDialogComponent>);
  protected readonly data: ImportAppSettingsDialogData = inject(MAT_DIALOG_DATA);
  private readonly appSettingService = inject(AppSettingService);
  private readonly projectService = inject(ProjectService);
  private readonly translate = inject(TranslateService);

  protected readonly resourceTypeIcons = RESOURCE_TYPE_ICONS;
  private readonly recommendedImportMode = resolveImportAppSettingsMode({
    resourceType: this.data.resourceType,
    deploymentMode: this.data.deploymentMode,
    runtimeStack: this.data.runtimeStack,
  });

  // ─── Step: 'upload' → 'board' → 'importing' ───
  protected readonly step = signal<'upload' | 'board' | 'importing'>('upload');

  // ─── Upload step ───
  protected readonly jsonInput = signal('');
  protected readonly parseError = signal('');
  protected readonly fileName = signal('');
  protected readonly importMode = signal<ImportAppSettingsMode>(this.recommendedImportMode);
  protected readonly acceptAttribute = buildImportAppSettingsAcceptAttribute();

  // ─── Board step ───
  protected readonly entries = signal<ImportEntry[]>([]);
  protected readonly searchFilter = signal('');

  // ─── Output caches ───
  private readonly outputCache = new Map<string, OutputDefinitionResponse[]>();
  protected readonly outputsLoading = signal<Set<string>>(new Set());

  // ─── Variable groups ───
  protected readonly vgOptions = signal<ProjectPipelineVariableGroupResponse[]>([]);
  protected readonly vgLoaded = signal(false);

  // ─── Import progress ───
  protected readonly importProgress = signal(0);
  protected readonly importTotal = signal(0);
  protected readonly importedSettings = signal<AppSettingResponse[]>([]);

  // ─── Source type select options ───
  protected readonly sourceTypeOptions: DsSelectOption[] = [
    { value: 'static', label: this.translate.instant('RESOURCE_EDIT.IMPORT_APP_SETTINGS.SOURCE_STATIC') },
    { value: 'output', label: this.translate.instant('RESOURCE_EDIT.IMPORT_APP_SETTINGS.SOURCE_OUTPUT') },
    { value: 'variableGroup', label: this.translate.instant('RESOURCE_EDIT.IMPORT_APP_SETTINGS.SOURCE_VARIABLE_GROUP') },
    { value: 'skip', label: this.translate.instant('RESOURCE_EDIT.IMPORT_APP_SETTINGS.SOURCE_SKIP') },
  ];

  protected readonly importModeOptions: DsSelectOption[] = listImportAppSettingsModes().map((definition) => ({
    value: definition.mode,
    label: this.translate.instant(definition.labelKey),
  }));

  // ─── Computed ───

  protected readonly selectedImportModeDefinition = computed(() =>
    getImportAppSettingsModeDefinition(this.importMode())
  );

  protected readonly recommendedImportModeDefinition = getImportAppSettingsModeDefinition(this.recommendedImportMode);

  protected readonly applicationProfileLabel = computed(() => {
    if (this.data.resourceType === 'ContainerApp' || this.data.deploymentMode === 'Container') {
      return 'Container';
    }

    if (this.data.runtimeStack) {
      return `${this.data.resourceType} / ${this.data.runtimeStack}`;
    }

    return this.data.resourceType;
  });

  protected readonly selectedModeExamples = computed(() =>
    this.selectedImportModeDefinition().exampleFileNames.join(', ')
  );

  protected readonly filteredEntries = computed(() => {
    const filter = this.searchFilter().toLowerCase().trim();
    const all = this.entries();
    if (!filter) return all;
    return all.filter(e => e.key.toLowerCase().includes(filter));
  });

  protected readonly selectedCount = computed(() =>
    this.entries().filter(e => e.selected && e.sourceType !== 'skip').length
  );

  protected readonly totalCount = computed(() => this.entries().length);

  protected readonly allSelected = computed(() =>
    this.entries().length > 0 && this.entries().every(e => e.selected)
  );

  protected readonly canImport = computed(() =>
    this.selectedCount() > 0 && this.step() === 'board'
  );

  protected readonly importFinished = computed(() =>
    this.step() === 'importing' && this.importProgress() >= this.importTotal()
  );

  protected readonly successCount = computed(() =>
    this.entries().filter(e => e.status === 'success').length
  );

  protected readonly errorCount = computed(() =>
    this.entries().filter(e => e.status === 'error').length
  );

  protected readonly duplicateCount = computed(() =>
    this.entries().filter(e => e.status === 'duplicate').length
  );

  protected readonly vgSelectOptions = computed<DsSelectOption[]>(() =>
    this.vgOptions().map(vg => ({ value: vg.id, label: vg.groupName }))
  );

  protected resourceSelectOptions = computed<DsSelectOption[]>(() =>
    this.data.siblingResources.map(r => ({
      value: r.id,
      label: r.name,
      icon: this.resourceTypeIcons[r.resourceType] ?? 'widgets',
    }))
  );

  // ─── File handling ─────────────────────────────────────────────────────────

    protected async onFileSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
      if (!file) {
        return;
      }

      this.prepareImportedFile(file);
      await this.readImportedFile(file);
  }

    protected async onDrop(event: DragEvent): Promise<void> {
    event.preventDefault();
    event.stopPropagation();
    const file = event.dataTransfer?.files[0];
      if (!file) {
        return;
      }

      this.prepareImportedFile(file);
      await this.readImportedFile(file);
  }

  protected onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
  }

    protected onImportModeChange(mode: ImportAppSettingsMode): void {
      this.importMode.set(mode);
    this.parseError.set('');
  }

  // ─── Parse JSON ────────────────────────────────────────────────────────────

  protected parseAndProceed(): void {
    const raw = this.jsonInput().trim();
    if (!raw) {
      this.parseError.set('RESOURCE_EDIT.IMPORT_APP_SETTINGS.ERROR_EMPTY');
      return;
    }

    let parsedEntries: Array<{ key: string; value: string }>;
    try {
      parsedEntries = parseImportAppSettingsContent(raw, this.importMode());
    } catch (error) {
      this.parseError.set(this.resolveParseErrorKey(error));
      return;
    }

    if (parsedEntries.length === 0) {
      this.parseError.set('RESOURCE_EDIT.IMPORT_APP_SETTINGS.ERROR_NO_VALUES');
      return;
    }

    const existingNames = new Set(this.data.existingSettingNames.map(n => n.toUpperCase()));

    const importEntries: ImportEntry[] = parsedEntries.map(({ key, value }) => ({
      key,
      rawValue: value,
      selected: !existingNames.has(key.toUpperCase()),
      sourceType: 'static',
      sourceResourceId: null,
      sourceOutputName: null,
      variableGroupId: null,
      pipelineVariableName: key,
      status: existingNames.has(key.toUpperCase()) ? 'duplicate' : 'pending',
      errorMessage: '',
    }));

    this.entries.set(importEntries);
    this.step.set('board');
    this.loadVariableGroups();
  }

  private resolveParseErrorKey(error: unknown): string {
    if (error instanceof Error && error.message === 'Root content must be an object.') {
      return 'RESOURCE_EDIT.IMPORT_APP_SETTINGS.ERROR_NOT_OBJECT';
    }

    return this.importMode() === 'dotnetJson' || this.importMode() === 'functionAppJson'
      ? 'RESOURCE_EDIT.IMPORT_APP_SETTINGS.ERROR_INVALID_JSON'
      : 'RESOURCE_EDIT.IMPORT_APP_SETTINGS.ERROR_INVALID_FORMAT';
  }

  // ─── Variable group loading ────────────────────────────────────────────────

  private async loadVariableGroups(): Promise<void> {
    if (this.vgLoaded()) return;
    try {
      const groups = await this.projectService.getPipelineVariableGroups(this.data.projectId);
      this.vgOptions.set(groups);
      this.vgLoaded.set(true);
    } catch {
      // non-blocking
    }
  }

  // ─── Output loading ───────────────────────────────────────────────────────

  protected async loadOutputsForResource(resourceId: string): Promise<OutputDefinitionResponse[]> {
    if (this.outputCache.has(resourceId)) {
      return this.outputCache.get(resourceId)!;
    }

    this.outputsLoading.update(s => new Set(s).add(resourceId));
    try {
      const resp = await this.appSettingService.getAvailableOutputs(resourceId);
      const outputs = resp.outputs ?? [];
      this.outputCache.set(resourceId, outputs);
      return outputs;
    } catch {
      return [];
    } finally {
      this.outputsLoading.update(s => {
        const next = new Set(s);
        next.delete(resourceId);
        return next;
      });
    }
  }

  protected getOutputOptionsForResource(resourceId: string): DsSelectOption[] {
    const cached = this.outputCache.get(resourceId);
    if (!cached) return [];
    return cached.map(o => ({ value: o.name, label: o.name }));
  }

  // ─── Board interactions ────────────────────────────────────────────────────

  protected toggleAll(checked: boolean): void {
    this.entries.update(list =>
      list.map(e => e.status === 'duplicate' ? e : { ...e, selected: checked })
    );
  }

  protected toggleEntry(index: number): void {
    this.entries.update(list =>
      list.map((e, i) => i === index ? { ...e, selected: !e.selected } : e)
    );
  }

  protected onSourceTypeChange(index: number, sourceType: ImportSourceType): void {
    this.entries.update(list =>
      list.map((e, i) => i === index ? {
        ...e,
        sourceType,
        sourceResourceId: null,
        sourceOutputName: null,
        variableGroupId: null,
        selected: sourceType === 'skip' ? false : e.selected,
      } : e)
    );
  }

  private prepareImportedFile(file: File): void {
    this.fileName.set(file.name);
    this.importMode.set(resolveImportAppSettingsModeForFileName(file.name, this.importMode()));
  }

  private async readImportedFile(file: File): Promise<void> {
    this.jsonInput.set(await file.text());
    this.parseError.set('');
  }

  protected async onSourceResourceChange(index: number, resourceId: string): Promise<void> {
    this.entries.update(list =>
      list.map((e, i) => i === index ? { ...e, sourceResourceId: resourceId, sourceOutputName: null } : e)
    );
    await this.loadOutputsForResource(resourceId);
  }

  protected onSourceOutputChange(index: number, outputName: string): void {
    this.entries.update(list =>
      list.map((e, i) => i === index ? { ...e, sourceOutputName: outputName } : e)
    );
  }

  protected onVariableGroupChange(index: number, vgId: string): void {
    this.entries.update(list =>
      list.map((e, i) => i === index ? { ...e, variableGroupId: vgId } : e)
    );
  }

  protected onPipelineVariableNameChange(index: number, name: string): void {
    this.entries.update(list =>
      list.map((e, i) => i === index ? { ...e, pipelineVariableName: name } : e)
    );
  }

  protected onKeyChange(index: number, key: string): void {
    this.entries.update(list =>
      list.map((e, i) => i === index ? { ...e, key } : e)
    );
  }

  // ─── Bulk apply source type ────────────────────────────────────────────────

  protected applySourceTypeToAll(sourceType: string): void {
    const typedSource = sourceType as ImportSourceType;
    this.entries.update(list =>
      list.map(e => e.status === 'duplicate' ? e : {
        ...e,
        sourceType: typedSource,
        sourceResourceId: null,
        sourceOutputName: null,
        variableGroupId: null,
        selected: typedSource !== 'skip',
      })
    );
  }

  // ─── Import execution ─────────────────────────────────────────────────────

  protected async startImport(): Promise<void> {
    const toImport = this.entries().filter(e => e.selected && e.sourceType !== 'skip' && e.status !== 'duplicate');
    if (toImport.length === 0) return;

    this.step.set('importing');
    this.importTotal.set(toImport.length);
    this.importProgress.set(0);

    const envNames = this.data.environments.map(e => e.name);

    for (const entry of toImport) {
      // Mark importing
      this.entries.update(list =>
        list.map(e => e.key === entry.key ? { ...e, status: 'importing' as const } : e)
      );

      try {
        const request = this.buildRequest(entry, envNames);
        const result = await this.appSettingService.add(this.data.resourceId, request);
        this.importedSettings.update(s => [...s, result]);

        this.entries.update(list =>
          list.map(e => e.key === entry.key ? { ...e, status: 'success' as const } : e)
        );
      } catch (err: unknown) {
        const message = err instanceof Error ? err.message : 'Unknown error';
        this.entries.update(list =>
          list.map(e => e.key === entry.key ? { ...e, status: 'error' as const, errorMessage: message } : e)
        );
      }

      this.importProgress.update(p => p + 1);
    }
  }

  private buildRequest(entry: ImportEntry, envNames: string[]): {
    name: string;
    environmentValues?: Record<string, string> | null;
    sourceResourceId?: string | null;
    sourceOutputName?: string | null;
    variableGroupId?: string;
    pipelineVariableName?: string;
  } {
    switch (entry.sourceType) {
      case 'output':
        return {
          name: entry.key,
          sourceResourceId: entry.sourceResourceId,
          sourceOutputName: entry.sourceOutputName,
        };

      case 'variableGroup':
        return {
          name: entry.key,
          variableGroupId: entry.variableGroupId ?? undefined,
          pipelineVariableName: entry.pipelineVariableName || entry.key,
        };

      case 'static':
      default: {
        const envValues: Record<string, string> = {};
        for (const env of envNames) {
          envValues[env] = entry.rawValue;
        }
        return {
          name: entry.key,
          environmentValues: envValues,
        };
      }
    }
  }

  // ─── Close ─────────────────────────────────────────────────────────────────

  protected close(): void {
    this.dialogRef.close(this.importedSettings().length > 0 ? this.importedSettings() : undefined);
  }

  protected cancel(): void {
    this.dialogRef.close();
  }

  // ─── Back to board ─────────────────────────────────────────────────────────

  protected backToUpload(): void {
    this.step.set('upload');
    this.entries.set([]);
    this.parseError.set('');
  }
}
