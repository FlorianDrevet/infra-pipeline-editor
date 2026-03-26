import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';
import { ProjectService } from '../../../shared/services/project.service';
import {
  ProjectResourceResponse,
  AddCrossConfigReferenceRequest,
} from '../../../shared/interfaces/cross-config-reference.interface';
import { RESOURCE_TYPE_ICONS } from '../enums/resource-type.enum';

export interface AddCrossConfigReferenceDialogData {
  configId: string;
  projectId: string;
  existingReferenceResourceIds: string[];
}

interface ResourceGroup {
  configName: string;
  resources: ProjectResourceResponse[];
}

@Component({
  selector: 'app-add-cross-config-reference-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    TranslateModule,
  ],
  templateUrl: './add-cross-config-reference-dialog.component.html',
  styleUrl: './add-cross-config-reference-dialog.component.scss',
})
export class AddCrossConfigReferenceDialogComponent implements OnInit {
  private readonly dialogRef = inject(MatDialogRef<AddCrossConfigReferenceDialogComponent>);
  private readonly data: AddCrossConfigReferenceDialogData = inject(MAT_DIALOG_DATA);
  private readonly projectService = inject(ProjectService);
  private readonly fb = inject(FormBuilder);

  protected readonly resourceTypeIcons = RESOURCE_TYPE_ICONS;
  protected readonly isLoading = signal(false);
  protected readonly errorKey = signal('');
  protected readonly step = signal<1 | 2>(1);
  protected readonly searchQuery = signal('');
  protected readonly selectedResource = signal<ProjectResourceResponse | null>(null);
  protected readonly allResources = signal<ProjectResourceResponse[]>([]);

  protected readonly form = this.fb.group({
    alias: ['', [Validators.required, Validators.pattern(/^[a-zA-Z][a-zA-Z0-9]*$/)]],
    purpose: [''],
  });

  protected readonly filteredGroups = computed<ResourceGroup[]>(() => {
    const resources = this.allResources();
    const query = this.searchQuery().toLowerCase();
    const configId = this.data.configId;
    const existingIds = new Set(this.data.existingReferenceResourceIds);

    const available = resources.filter(
      (r) => r.configId !== configId && !existingIds.has(r.resourceId)
    );

    const filtered = query
      ? available.filter(
          (r) =>
            r.resourceName.toLowerCase().includes(query) ||
            r.resourceType.toLowerCase().includes(query) ||
            r.configName.toLowerCase().includes(query)
        )
      : available;

    const groupMap = new Map<string, ProjectResourceResponse[]>();
    for (const r of filtered) {
      const existing = groupMap.get(r.configName);
      if (existing) {
        existing.push(r);
      } else {
        groupMap.set(r.configName, [r]);
      }
    }

    return Array.from(groupMap.entries()).map(([configName, res]) => ({
      configName,
      resources: res,
    }));
  });

  protected readonly hasNoResources = computed(() => {
    const resources = this.allResources();
    const configId = this.data.configId;
    const existingIds = new Set(this.data.existingReferenceResourceIds);
    return resources.filter((r) => r.configId !== configId && !existingIds.has(r.resourceId)).length === 0;
  });

  async ngOnInit(): Promise<void> {
    await this.loadResources();
  }

  private async loadResources(): Promise<void> {
    this.isLoading.set(true);
    this.errorKey.set('');
    try {
      const resources = await this.projectService.getProjectResources(this.data.projectId);
      this.allResources.set(resources);
    } catch {
      this.errorKey.set('CONFIG_DETAIL.CROSS_CONFIG_REFS.ADD_DIALOG_ERROR');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected onSearchInput(event: Event): void {
    this.searchQuery.set((event.target as HTMLInputElement).value);
  }

  protected selectResource(resource: ProjectResourceResponse): void {
    this.selectedResource.set(resource);
    const suggestedAlias = this.generateAlias(resource);
    this.form.patchValue({ alias: suggestedAlias });
    this.step.set(2);
  }

  protected goBack(): void {
    this.step.set(1);
  }

  protected onConfirm(): void {
    if (this.form.invalid) return;
    const resource = this.selectedResource();
    if (!resource) return;

    const result: AddCrossConfigReferenceRequest = {
      targetResourceId: resource.resourceId,
      alias: this.form.value.alias!,
      purpose: this.form.value.purpose || undefined,
    };
    this.dialogRef.close(result);
  }

  protected onCancel(): void {
    this.dialogRef.close();
  }

  private generateAlias(resource: ProjectResourceResponse): string {
    const configPrefix = this.toCamelCase(resource.configName);
    const resourceName = this.toCamelCase(resource.resourceName);
    return configPrefix + resourceName.charAt(0).toUpperCase() + resourceName.slice(1);
  }

  private toCamelCase(str: string): string {
    return str
      .replace(/[^a-zA-Z0-9]+(.)/g, (_, char: string) => char.toUpperCase())
      .replace(/^[A-Z]/, (c) => c.toLowerCase())
      .replace(/[^a-zA-Z0-9]/g, '');
  }
}
