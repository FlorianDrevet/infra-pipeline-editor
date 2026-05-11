import { Component, inject, signal, computed } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { DsButtonComponent, DsSelectComponent, DsSelectOption, DsTextFieldComponent } from '../../../shared/components/ds';
import { AzureResourceResponse } from '../../../shared/interfaces/resource-group.interface';
import { AppSettingService } from '../../../shared/services/app-setting.service';
import { AppSettingResponse, OutputDefinitionResponse } from '../../../shared/interfaces/app-setting.interface';
import { ProjectPipelineVariableGroupResponse } from '../../../shared/interfaces/project.interface';
import { ProjectService } from '../../../shared/services/project.service';
import { RESOURCE_TYPE_ICONS } from '../../config-detail/enums/resource-type.enum';

// ─── Data contract ───────────────────────────────────────────────────────────

export interface ImportAppSettingsDialogData {
  resourceId: string;
  currentResourceName: string;
  siblingResources: AzureResourceResponse[];
  environments: { name: string }[];
  projectId: string;
  existingSettingNames: string[];
}

// ─── Source types ────────────────────────────────────────────────────────────

type ImportSourceType = 'static' | 'output' | 'variableGroup' | 'skip';

// ─── Parsed entry ────────────────────────────────────────────────────────────

interface ImportEntry {
  key: string;
  jsonValue: string;
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

// ─── Helpers ─────────────────────────────────────────────────────────────────

function flattenJson(obj: unknown, prefix: string = ''): Record<string, string> {
  const result: Record<string, string> = {};
  if (obj === null || obj === undefined) return result;
  if (typeof obj !== 'object' || Array.isArray(obj)) {
    result[prefix] = String(obj);
    return result;
  }
  for (const [k, v] of Object.entries(obj as Record<string, unknown>)) {
    const fullKey = prefix ? `${prefix}__${k}` : k;
    if (v !== null && typeof v === 'object' && !Array.isArray(v)) {
      Object.assign(result, flattenJson(v, fullKey));
    } else {
      result[fullKey] = v === null || v === undefined ? '' : String(v);
    }
  }
  return result;
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
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatCheckboxModule,
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

  // ─── Step: 'upload' → 'board' → 'importing' ───
  protected readonly step = signal<'upload' | 'board' | 'importing'>('upload');

  // ─── Upload step ───
  protected readonly jsonInput = signal('');
  protected readonly parseError = signal('');
  protected readonly fileName = signal('');

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

  // ─── Computed ───

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

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.fileName.set(file.name);
    const reader = new FileReader();
    reader.onload = () => {
      this.jsonInput.set(reader.result as string);
      this.parseError.set('');
    };
    reader.readAsText(file);
  }

  protected onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    const file = event.dataTransfer?.files[0];
    if (!file) return;
    this.fileName.set(file.name);
    const reader = new FileReader();
    reader.onload = () => {
      this.jsonInput.set(reader.result as string);
      this.parseError.set('');
    };
    reader.readAsText(file);
  }

  protected onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
  }

  // ─── Parse JSON ────────────────────────────────────────────────────────────

  protected parseAndProceed(): void {
    const raw = this.jsonInput().trim();
    if (!raw) {
      this.parseError.set('RESOURCE_EDIT.IMPORT_APP_SETTINGS.ERROR_EMPTY');
      return;
    }

    let parsed: unknown;
    try {
      parsed = JSON.parse(raw);
    } catch {
      this.parseError.set('RESOURCE_EDIT.IMPORT_APP_SETTINGS.ERROR_INVALID_JSON');
      return;
    }

    if (typeof parsed !== 'object' || parsed === null || Array.isArray(parsed)) {
      this.parseError.set('RESOURCE_EDIT.IMPORT_APP_SETTINGS.ERROR_NOT_OBJECT');
      return;
    }

    const flat = flattenJson(parsed);
    const existingNames = new Set(this.data.existingSettingNames.map(n => n.toUpperCase()));

    const importEntries: ImportEntry[] = Object.entries(flat).map(([key, value]) => ({
      key,
      jsonValue: value,
      selected: !existingNames.has(key.toUpperCase()),
      sourceType: 'static' as ImportSourceType,
      sourceResourceId: null,
      sourceOutputName: null,
      variableGroupId: null,
      pipelineVariableName: key,
      status: existingNames.has(key.toUpperCase()) ? 'duplicate' as const : 'pending' as const,
      errorMessage: '',
    }));

    this.entries.set(importEntries);
    this.step.set('board');
    this.loadVariableGroups();
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

  protected onSourceTypeChange(index: number, sourceType: string): void {
    const typedSource = sourceType as ImportSourceType;
    this.entries.update(list =>
      list.map((e, i) => i === index ? {
        ...e,
        sourceType: typedSource,
        sourceResourceId: null,
        sourceOutputName: null,
        variableGroupId: null,
        selected: typedSource !== 'skip' ? e.selected : false,
      } : e)
    );
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
          envValues[env] = entry.jsonValue;
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
