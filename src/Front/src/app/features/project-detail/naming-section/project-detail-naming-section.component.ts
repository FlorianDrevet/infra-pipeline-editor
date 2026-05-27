import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';

import { signal } from '@angular/core';
import {
  ResourceNamingTemplateResponse,
  SetResourceAbbreviationOverrideRequest,
} from '../../../shared/interfaces/infra-config.interface';
import { ProjectResponse } from '../../../shared/interfaces/project.interface';
import { ProjectService } from '../../../shared/services/project.service';
import { RESOURCE_TYPE_OPTIONS, RESOURCE_TYPE_ABBREVIATIONS, RESOURCE_TYPE_ICONS } from '../../../shared/resource-metadata/resource-type.metadata';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import {
  EditAbbreviationDialogComponent,
  EditAbbreviationDialogData,
  EditAbbreviationDialogResult,
} from '../../../shared/components/edit-abbreviation-dialog/edit-abbreviation-dialog.component';
import {
  AddProjectNamingTemplateDialogComponent,
  AddProjectNamingTemplateDialogData,
  AddProjectNamingTemplateDialogResult,
} from '../add-project-naming-template-dialog/add-project-naming-template-dialog.component';

@Component({
  selector: 'app-project-detail-naming-section',
  standalone: true,
  imports: [
    MatDialogModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    TranslateModule,
  ],
  templateUrl: './project-detail-naming-section.component.html',
  styleUrl: './project-detail-naming-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectDetailNamingSectionComponent {
  private readonly projectService = inject(ProjectService);
  private readonly dialog = inject(MatDialog);

  readonly project = input.required<ProjectResponse | null>();
  readonly canWrite = input.required<boolean>();
  readonly projectChange = output<ProjectResponse>();

  protected readonly namingActionKey = signal<string | null>(null);
  protected readonly namingErrorKey = signal('');
  protected readonly resourceTypeOptions = RESOURCE_TYPE_OPTIONS;

  protected readonly canAddResourceNamingTemplate = computed(() => {
    const configuredTypes = new Set((this.project()?.resourceNamingTemplates ?? []).map((item) => item.resourceType));
    return this.resourceTypeOptions.some((option) => !configuredTypes.has(option.value));
  });

  protected readonly abbreviationDisplayItems = computed(() => {
    const proj = this.project();
    if (!proj) return [];

    const overrides = proj.resourceAbbreviations ?? [];
    const overrideMap = new Map(overrides.map((override) => [override.resourceType, override.abbreviation]));

    const usedTypes: string[] = proj.usedResourceTypes ?? [];
    const sortedUsedTypes = [...usedTypes].sort((left, right) => left.localeCompare(right));

    return sortedUsedTypes
      .map((resourceType) => {
        const defaultAbbr = RESOURCE_TYPE_ABBREVIATIONS[resourceType] ?? resourceType.toLowerCase();
        const customAbbr = overrideMap.get(resourceType);
        return {
          resourceType,
          icon: RESOURCE_TYPE_ICONS[resourceType] ?? 'widgets',
          defaultAbbreviation: defaultAbbr,
          customAbbreviation: customAbbr ?? null,
          effectiveAbbreviation: customAbbr ?? defaultAbbr,
          isCustomized: !!customAbbr,
        };
      });
  });

  protected isNamingActionActive(actionKey: string): boolean {
    return this.namingActionKey() === actionKey;
  }

  protected isResourceNamingTemplateBusy(resourceType: string): boolean {
    const actionKey = this.namingActionKey();
    return actionKey === `resource:${resourceType}` || actionKey === `resource-remove:${resourceType}`;
  }

  protected isAbbreviationBusy(resourceType: string): boolean {
    const key = this.namingActionKey();
    return key === `abbr:${resourceType}` || key === `abbr-remove:${resourceType}`;
  }

  protected openDefaultNamingTemplateDialog(): void {
    const project = this.project();
    if (!project || !this.canWrite()) return;

    const dialogRef = this.dialog.open(AddProjectNamingTemplateDialogComponent, {
      data: {
        mode: 'default',
        isEditMode: !!project.defaultNamingTemplate,
        template: project.defaultNamingTemplate,
      } satisfies AddProjectNamingTemplateDialogData,
      width: '460px',
    });

    dialogRef.afterClosed().subscribe(async (result: AddProjectNamingTemplateDialogResult | null) => {
      if (!result) return;
      await this.saveDefaultNamingTemplate(project.id, result.template);
    });
  }

  protected openResourceNamingTemplateDialog(existing?: ResourceNamingTemplateResponse): void {
    const project = this.project();
    if (!project || !this.canWrite()) return;

    const usedResourceTypes = new Set(
      project.resourceNamingTemplates
      .map((item) => item.resourceType)
      .filter((resourceType) => resourceType !== existing?.resourceType),
    );

    const availableResourceTypes = this.resourceTypeOptions
      .map((option) => option.value)
      .filter((resourceType) => !usedResourceTypes.has(resourceType));

    const dialogRef = this.dialog.open(AddProjectNamingTemplateDialogComponent, {
      data: {
        mode: 'resource',
        isEditMode: !!existing,
        template: existing?.template ?? '',
        resourceType: existing?.resourceType,
        availableResourceTypes: existing ? [existing.resourceType] : availableResourceTypes,
      } satisfies AddProjectNamingTemplateDialogData,
      width: '460px',
    });

    dialogRef.afterClosed().subscribe(async (result: AddProjectNamingTemplateDialogResult | null) => {
      if (!result?.resourceType) return;
      await this.saveResourceNamingTemplate(project.id, result.resourceType, result.template);
    });
  }

  protected openRemoveResourceNamingTemplateDialog(template: ResourceNamingTemplateResponse): void {
    const project = this.project();
    if (!project) return;

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'PROJECT_DETAIL.NAMING_TEMPLATES.REMOVE_CONFIRM_TITLE',
        messageKey: 'PROJECT_DETAIL.NAMING_TEMPLATES.REMOVE_CONFIRM_MESSAGE',
        messageParams: { resourceType: template.resourceType },
        confirmKey: 'PROJECT_DETAIL.NAMING_TEMPLATES.REMOVE_CONFIRM_YES',
        cancelKey: 'PROJECT_DETAIL.NAMING_TEMPLATES.REMOVE_CONFIRM_CANCEL',
      } satisfies ConfirmDialogData,
      width: '400px',
    });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;
      await this.removeResourceNamingTemplate(project.id, template.resourceType);
    });
  }

  protected openEditAbbreviationDialog(item: { resourceType: string; defaultAbbreviation: string; customAbbreviation: string | null }): void {
    const dialogRef = this.dialog.open<EditAbbreviationDialogComponent, EditAbbreviationDialogData, EditAbbreviationDialogResult>(
      EditAbbreviationDialogComponent,
      {
        width: '420px',
        data: {
          resourceType: item.resourceType,
          defaultAbbreviation: item.defaultAbbreviation,
          currentAbbreviation: item.customAbbreviation ?? item.defaultAbbreviation,
        },
      },
    );

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.saveAbbreviationOverride(item.resourceType, result.abbreviation);
      }
    });
  }

  protected openResetAbbreviationDialog(item: { resourceType: string; defaultAbbreviation: string }): void {
    const dialogRef = this.dialog.open<ConfirmDialogComponent, ConfirmDialogData, boolean>(
      ConfirmDialogComponent,
      {
        width: '420px',
        data: {
          titleKey: 'ABBREVIATIONS.RESET_CONFIRM_TITLE',
          messageKey: 'ABBREVIATIONS.RESET_CONFIRM_MESSAGE',
          messageParams: { resourceType: item.resourceType, default: item.defaultAbbreviation },
          confirmKey: 'ABBREVIATIONS.RESET_CONFIRM_ACTION',
          cancelKey: 'ABBREVIATIONS.RESET_CONFIRM_CANCEL',
        },
      },
    );

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.removeAbbreviationOverride(item.resourceType);
      }
    });
  }

  private async saveDefaultNamingTemplate(projectId: string, template: string): Promise<void> {
    this.namingActionKey.set('default');
    this.namingErrorKey.set('');

    try {
      await this.projectService.setDefaultNamingTemplate(projectId, { template });
      await this.refreshAndEmit(projectId);
    } catch {
      this.namingErrorKey.set('PROJECT_DETAIL.NAMING_TEMPLATES.DEFAULT_SAVE_ERROR');
    } finally {
      this.namingActionKey.set(null);
    }
  }

  private async saveResourceNamingTemplate(projectId: string, resourceType: string, template: string): Promise<void> {
    this.namingActionKey.set(`resource:${resourceType}`);
    this.namingErrorKey.set('');

    try {
      await this.projectService.setResourceNamingTemplate(projectId, resourceType, { template });
      await this.refreshAndEmit(projectId);
    } catch {
      this.namingErrorKey.set('PROJECT_DETAIL.NAMING_TEMPLATES.RESOURCE_SAVE_ERROR');
    } finally {
      this.namingActionKey.set(null);
    }
  }

  private async removeResourceNamingTemplate(projectId: string, resourceType: string): Promise<void> {
    this.namingActionKey.set(`resource-remove:${resourceType}`);
    this.namingErrorKey.set('');

    try {
      await this.projectService.removeResourceNamingTemplate(projectId, resourceType);
      await this.refreshAndEmit(projectId);
    } catch {
      this.namingErrorKey.set('PROJECT_DETAIL.NAMING_TEMPLATES.RESOURCE_REMOVE_ERROR');
    } finally {
      this.namingActionKey.set(null);
    }
  }

  private async saveAbbreviationOverride(resourceType: string, abbreviation: string): Promise<void> {
    const projectId = this.project()?.id;
    if (!projectId) return;

    this.namingActionKey.set(`abbr:${resourceType}`);
    this.namingErrorKey.set('');

    try {
      const request: SetResourceAbbreviationOverrideRequest = { abbreviation };
      await this.projectService.setResourceAbbreviation(projectId, resourceType, request);
      await this.refreshAndEmit(projectId);
    } catch {
      this.namingErrorKey.set('ABBREVIATIONS.SAVE_ERROR');
    } finally {
      this.namingActionKey.set(null);
    }
  }

  private async removeAbbreviationOverride(resourceType: string): Promise<void> {
    const projectId = this.project()?.id;
    if (!projectId) return;

    this.namingActionKey.set(`abbr-remove:${resourceType}`);
    this.namingErrorKey.set('');

    try {
      await this.projectService.removeResourceAbbreviation(projectId, resourceType);
      await this.refreshAndEmit(projectId);
    } catch {
      this.namingErrorKey.set('ABBREVIATIONS.RESET_ERROR');
    } finally {
      this.namingActionKey.set(null);
    }
  }

  private async refreshAndEmit(projectId: string): Promise<void> {
    const refreshed = await this.projectService.getProject(projectId);
    this.projectChange.emit(refreshed);
  }
}
