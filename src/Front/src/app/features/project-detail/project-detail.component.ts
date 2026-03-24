import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import { ProjectResponse, ProjectMemberResponse } from '../../shared/interfaces/project.interface';
import {
  InfrastructureConfigResponse,
  EnvironmentDefinitionResponse,
  ResourceNamingTemplateResponse,
} from '../../shared/interfaces/infra-config.interface';
import { UserResponse } from '../../shared/interfaces/infra-config.interface';
import { ProjectService } from '../../shared/services/project.service';
import { InfraConfigService } from '../../shared/services/infra-config.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { RecentlyViewedService } from '../../shared/services/recently-viewed.service';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { AddProjectMemberDialogComponent, AddProjectMemberDialogData } from './add-project-member-dialog/add-project-member-dialog.component';
import { AddConfigDialogComponent, AddConfigDialogData } from './add-config-dialog/add-config-dialog.component';
import {
  AddProjectEnvironmentDialogComponent,
  AddProjectEnvironmentDialogData,
} from './add-project-environment-dialog/add-project-environment-dialog.component';
import {
  AddProjectNamingTemplateDialogComponent,
  AddProjectNamingTemplateDialogData,
  AddProjectNamingTemplateDialogResult,
} from './add-project-naming-template-dialog/add-project-naming-template-dialog.component';
import { RESOURCE_TYPE_OPTIONS } from '../config-detail/enums/resource-type.enum';
import { FormsModule } from '@angular/forms';

const ROLES = ['Owner', 'Contributor', 'Reader'] as const;
const ROLE_ORDER: Record<string, number> = { Owner: 0, Contributor: 1, Reader: 2 };
const ROLE_ICONS: Record<string, string> = { Owner: 'shield', Contributor: 'edit', Reader: 'visibility' };

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [
    TranslateModule,
    RouterLink,
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatTabsModule,
    MatTooltipModule,
  ],
  templateUrl: './project-detail.component.html',
  styleUrl: './project-detail.component.scss',
})
export class ProjectDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly projectService = inject(ProjectService);
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly authService = inject(AuthenticationService);
  private readonly recentlyViewedService = inject(RecentlyViewedService);
  private readonly dialog = inject(MatDialog);

  protected readonly project = signal<ProjectResponse | null>(null);
  protected readonly configs = signal<InfrastructureConfigResponse[]>([]);
  protected readonly availableUsers = signal<UserResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly loadError = signal('');
  protected readonly memberActionId = signal<string | null>(null);
  protected readonly memberErrorKey = signal('');
  protected readonly configErrorKey = signal('');
  protected readonly envActionId = signal<string | null>(null);
  protected readonly envErrorKey = signal('');
  protected readonly namingActionKey = signal<string | null>(null);
  protected readonly namingErrorKey = signal('');
  protected readonly roles = ROLES;
  protected readonly resourceTypeOptions = RESOURCE_TYPE_OPTIONS;

  protected readonly sortedEnvironments = computed(() => {
    const envs = this.project()?.environmentDefinitions ?? [];
    return [...envs].sort((a, b) => a.order - b.order);
  });

  protected readonly canAddResourceNamingTemplate = computed(() => {
    const configuredTypes = new Set((this.project()?.resourceNamingTemplates ?? []).map((item) => item.resourceType));
    return this.resourceTypeOptions.some((option) => !configuredTypes.has(option.value));
  });

  protected readonly membersByRole = computed(() => {
    const members = this.project()?.members ?? [];
    return ROLES
      .map((role) => ({
        role,
        icon: ROLE_ICONS[role],
        members: members.filter((m) => m.role === role),
      }))
      .filter((group) => group.members.length > 0);
  });

  protected readonly isOwner = computed(() => {
    const oid = this.authService.getMsalAccount?.localAccountId;
    if (!oid) return false;
    const members = this.project()?.members ?? [];
    const me = members.find((m) => m.entraId === oid);
    return me?.role === 'Owner';
  });

  protected readonly canWrite = computed(() => {
    const oid = this.authService.getMsalAccount?.localAccountId;
    if (!oid) return false;
    const members = this.project()?.members ?? [];
    const me = members.find((m) => m.entraId === oid);
    return me?.role === 'Owner' || me?.role === 'Contributor';
  });

  async ngOnInit(): Promise<void> {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loadError.set('PROJECT_DETAIL.ERROR.NO_ID');
      return;
    }
    await this.loadProject(id);
  }

  private async loadProject(id: string): Promise<void> {
    this.isLoading.set(true);
    this.loadError.set('');

    try {
      const [project, configs, users] = await Promise.all([
        this.projectService.getProject(id),
        this.projectService.getProjectConfigs(id),
        this.projectService.getUsers(),
      ]);
      this.project.set(project);
      this.configs.set(configs);
      this.availableUsers.set(users);
      this.recentlyViewedService.trackView({
        id: project.id,
        name: project.name,
        type: 'project',
        description: project.description,
      });
    } catch {
      this.loadError.set('PROJECT_DETAIL.ERROR.LOAD_FAILED');
    } finally {
      this.isLoading.set(false);
    }
  }

  // ─── Members ───

  protected async onRoleChange(member: ProjectMemberResponse, newRole: string): Promise<void> {
    if (newRole === member.role) return;

    const projectId = this.project()?.id;
    if (!projectId) return;

    this.memberActionId.set(member.id);
    this.memberErrorKey.set('');

    try {
      const updated = await this.projectService.updateMemberRole(projectId, member.userId, { newRole });
      this.project.set(updated);
    } catch {
      this.memberErrorKey.set('PROJECT_DETAIL.MEMBERS.ROLE_CHANGE_ERROR');
    } finally {
      this.memberActionId.set(null);
    }
  }

  protected openAddMemberDialog(): void {
    const projectId = this.project()?.id;
    if (!projectId) return;

    const data: AddProjectMemberDialogData = {
      projectId,
      existingUserIds: (this.project()?.members ?? []).map((m) => m.userId),
      availableUsers: this.availableUsers(),
    };

    const dialogRef = this.dialog.open(AddProjectMemberDialogComponent, {
      width: '480px',
      data,
    });

    dialogRef.afterClosed().subscribe((result?: ProjectResponse) => {
      if (result) {
        this.project.set(result);
      }
    });
  }

  protected openRemoveMemberDialog(member: ProjectMemberResponse): void {
    const projectId = this.project()?.id;
    if (!projectId) return;

    const data: ConfirmDialogData = {
      titleKey: 'PROJECT_DETAIL.MEMBERS.REMOVE_CONFIRM_TITLE',
      messageKey: 'PROJECT_DETAIL.MEMBERS.REMOVE_CONFIRM_MESSAGE',
      messageParams: { name: `${member.firstName} ${member.lastName}` },
      confirmKey: 'PROJECT_DETAIL.MEMBERS.REMOVE_CONFIRM_YES',
      cancelKey: 'PROJECT_DETAIL.MEMBERS.REMOVE_CONFIRM_CANCEL',
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, { width: '400px', data });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;

      this.memberActionId.set(member.id);
      this.memberErrorKey.set('');

      try {
        await this.projectService.removeMember(projectId, member.userId);
        this.project.update((p) => {
          if (!p) return p;
          return { ...p, members: p.members.filter((m) => m.id !== member.id) };
        });
      } catch {
        this.memberErrorKey.set('PROJECT_DETAIL.MEMBERS.REMOVE_ERROR');
      } finally {
        this.memberActionId.set(null);
      }
    });
  }

  // ─── Configs ───

  protected openAddConfigDialog(): void {
    const projectId = this.project()?.id;
    if (!projectId) return;

    const data: AddConfigDialogData = { projectId };

    const dialogRef = this.dialog.open(AddConfigDialogComponent, {
      width: '480px',
      data,
    });

    dialogRef.afterClosed().subscribe((result?: InfrastructureConfigResponse) => {
      if (result) {
        this.configs.update((configs) => [result, ...configs]);
      }
    });
  }

  // ─── Environments ───

  protected openAddEnvironmentDialog(existing?: EnvironmentDefinitionResponse): void {
    const project = this.project();
    if (!project) return;

    const dialogRef = this.dialog.open(AddProjectEnvironmentDialogComponent, {
      data: {
        projectId: project.id,
        existing,
        allEnvironments: project.environmentDefinitions,
      } satisfies AddProjectEnvironmentDialogData,
      width: '520px',
      maxWidth: '95vw',
    });

    dialogRef.afterClosed().subscribe(async (saved?: boolean) => {
      if (saved) {
        await this.refreshProject(project.id);
      }
    });
  }

  protected openRemoveEnvironmentDialog(env: EnvironmentDefinitionResponse): void {
    const project = this.project();
    if (!project) return;

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'PROJECT_DETAIL.ENVIRONMENTS.REMOVE_CONFIRM_TITLE',
        messageKey: 'PROJECT_DETAIL.ENVIRONMENTS.REMOVE_CONFIRM_MESSAGE',
        messageParams: { name: env.name },
        confirmKey: 'PROJECT_DETAIL.ENVIRONMENTS.REMOVE_CONFIRM_YES',
        cancelKey: 'PROJECT_DETAIL.ENVIRONMENTS.REMOVE_CONFIRM_CANCEL',
      } satisfies ConfirmDialogData,
      width: '400px',
    });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;
      await this.removeEnvironment(project.id, env);
    });
  }

  private async removeEnvironment(projectId: string, env: EnvironmentDefinitionResponse): Promise<void> {
    this.envActionId.set(env.id);
    this.envErrorKey.set('');

    try {
      await this.projectService.removeEnvironment(projectId, env.id);
      await this.refreshProject(projectId);
    } catch {
      this.envErrorKey.set('PROJECT_DETAIL.ENVIRONMENTS.REMOVE_ERROR');
    } finally {
      this.envActionId.set(null);
    }
  }

  // ─── Naming Templates ───

  protected isNamingActionActive(actionKey: string): boolean {
    return this.namingActionKey() === actionKey;
  }

  protected isResourceNamingTemplateBusy(resourceType: string): boolean {
    const actionKey = this.namingActionKey();
    return actionKey === `resource:${resourceType}` || actionKey === `resource-remove:${resourceType}`;
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

    const usedResourceTypes = project.resourceNamingTemplates
      .map((item) => item.resourceType)
      .filter((resourceType) => resourceType !== existing?.resourceType);

    const availableResourceTypes = this.resourceTypeOptions
      .map((option) => option.value)
      .filter((resourceType) => !usedResourceTypes.includes(resourceType));

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

  private async saveDefaultNamingTemplate(projectId: string, template: string): Promise<void> {
    this.namingActionKey.set('default');
    this.namingErrorKey.set('');

    try {
      await this.projectService.setDefaultNamingTemplate(projectId, { template });
      await this.refreshProject(projectId);
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
      await this.refreshProject(projectId);
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
      await this.refreshProject(projectId);
    } catch {
      this.namingErrorKey.set('PROJECT_DETAIL.NAMING_TEMPLATES.RESOURCE_REMOVE_ERROR');
    } finally {
      this.namingActionKey.set(null);
    }
  }

  private async refreshProject(projectId: string): Promise<void> {
    const refreshed = await this.projectService.getProject(projectId);
    this.project.set(refreshed);
  }

  // ─── Delete Project ───

  protected openDeleteProjectDialog(): void {
    const project = this.project();
    if (!project) return;

    const data: ConfirmDialogData = {
      titleKey: 'PROJECT_DETAIL.DELETE.CONFIRM_TITLE',
      messageKey: 'PROJECT_DETAIL.DELETE.CONFIRM_MESSAGE',
      messageParams: { name: project.name },
      confirmKey: 'PROJECT_DETAIL.DELETE.CONFIRM_YES',
      cancelKey: 'PROJECT_DETAIL.DELETE.CONFIRM_CANCEL',
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, { width: '400px', data });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;

      try {
        await this.projectService.deleteProject(project.id);
        this.router.navigate(['/']);
      } catch {
        this.loadError.set('PROJECT_DETAIL.DELETE.ERROR');
      }
    });
  }

  // ─── Delete Config ───

  protected openDeleteConfigDialog(config: InfrastructureConfigResponse): void {
    const project = this.project();
    if (!project) return;

    const data: ConfirmDialogData = {
      titleKey: 'PROJECT_DETAIL.DELETE_CONFIG.CONFIRM_TITLE',
      messageKey: 'PROJECT_DETAIL.DELETE_CONFIG.CONFIRM_MESSAGE',
      messageParams: { name: config.name },
      confirmKey: 'PROJECT_DETAIL.DELETE_CONFIG.CONFIRM_YES',
      cancelKey: 'PROJECT_DETAIL.DELETE_CONFIG.CONFIRM_CANCEL',
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, { width: '400px', data });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;

      this.configErrorKey.set('');
      try {
        await this.infraConfigService.delete(config.id);
        this.configs.update((configs) => configs.filter((c) => c.id !== config.id));
      } catch {
        this.configErrorKey.set('PROJECT_DETAIL.DELETE_CONFIG.ERROR');
      }
    });
  }
}
