import { Component, OnDestroy, OnInit, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ProjectResponse, ProjectMemberResponse, ProjectPipelineVariableGroupResponse } from '../../shared/interfaces/project.interface';
import {
  InfrastructureConfigResponse,
  EnvironmentDefinitionResponse,
  ResourceNamingTemplateResponse,
  SetResourceAbbreviationOverrideRequest,
  TagRequest,
  UserResponse,
} from '../../shared/interfaces/infra-config.interface';
import { ProjectService } from '../../shared/services/project.service';
import { InfraConfigService } from '../../shared/services/infra-config.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import {
  DsButtonComponent,
  DsPanelActionButtonComponent,
  DsSelectComponent,
  DsSelectOption,
  DsTextFieldComponent,
} from '../../shared/components/ds';
import { RecentlyViewedService } from '../../shared/services/recently-viewed.service';
import { PageContextService } from '../../shared/services/page-context.service';
import { SidebarContextService } from '../../core/layouts/sidebar/sidebar-context.service';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import {
  EditAbbreviationDialogComponent,
  EditAbbreviationDialogData,
  EditAbbreviationDialogResult,
} from '../../shared/components/edit-abbreviation-dialog/edit-abbreviation-dialog.component';
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
import { ProjectDetailEnvironmentsSectionComponent } from './environments-section/project-detail-environments-section.component';
import { LayoutRepositoriesComponent } from './layout-repositories/layout-repositories.component';
import { SplitGenerationSwitcherComponent } from './split-generation-switcher/split-generation-switcher.component';
import { RESOURCE_TYPE_OPTIONS, RESOURCE_TYPE_ABBREVIATIONS, RESOURCE_TYPE_ICONS } from '../../shared/resource-metadata/resource-type.metadata';
import { AddVariableGroupDialogComponent } from '../config-detail/add-variable-group-dialog/add-variable-group-dialog.component';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatChipsModule } from '@angular/material/chips';
import { BicepFilePanelComponent } from '../../shared/components/bicep-file-panel/bicep-file-panel.component';
import { ProjectDetailGenerationWorkflowService } from './project-detail-generation-workflow.service';
import { getProjectDetailTabIndex, getProjectDetailTabQuery } from '../../shared/enums/detail-route-tabs';

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
    ReactiveFormsModule,
    BicepFilePanelComponent,
    LayoutRepositoriesComponent,
    ProjectDetailEnvironmentsSectionComponent,
    SplitGenerationSwitcherComponent,
    MatButtonModule,
    MatButtonToggleModule,
    MatChipsModule,
    MatDialogModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
    MatTabsModule,
    MatTooltipModule,
    DsButtonComponent,
    DsPanelActionButtonComponent,
    DsSelectComponent,
    DsTextFieldComponent,
  ],
  templateUrl: './project-detail.component.html',
  styleUrl: './project-detail.component.scss',
  providers: [ProjectDetailGenerationWorkflowService],
})
export class ProjectDetailComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly routeQueryParamMap = toSignal(this.route.queryParamMap, {
    initialValue: this.route.snapshot.queryParamMap,
  });
  private readonly projectService = inject(ProjectService);
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly authService = inject(AuthenticationService);
  private readonly recentlyViewedService = inject(RecentlyViewedService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  private readonly pageContextService = inject(PageContextService);
  private readonly sidebarContextService = inject(SidebarContextService);
  private readonly generationWorkflow = inject(ProjectDetailGenerationWorkflowService);

  protected readonly project = signal<ProjectResponse | null>(null);
  private readonly breadcrumbEffect = effect(() => {
    const project = this.project();
    const projectsLabel = this.translate.instant('NAV.BREADCRUMB.PROJECTS') as string;
    const segments = project
      ? [
          { label: projectsLabel, routerLink: '/' },
          { label: project.name },
        ]
      : [{ label: projectsLabel, routerLink: '/' }];
    this.pageContextService.setBreadcrumb(segments);
  });

  public ngOnDestroy(): void {
    this.pageContextService.clear();
  }
  protected readonly configs = signal<InfrastructureConfigResponse[]>([]);
  private readonly generationSyncEffect = effect(() => {
    this.generationWorkflow.setProject(this.project());
    this.generationWorkflow.setConfigs(this.configs());
  });
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

  private readonly translateService = inject(TranslateService);
  protected readonly roleDsOptions: DsSelectOption[] = ROLES.map((role) => ({
    value: role,
    label: this.translateService.instant('PROJECT_DETAIL.MEMBERS.ROLE_' + role.toUpperCase()),
  }));

  // ─── Project Tags ───
  protected readonly isEditingProjectTags = signal(false);
  protected readonly editingTags = signal<TagRequest[]>([]);
  protected readonly tagsErrorKey = signal('');
  protected readonly tagsSaving = signal(false);
  protected readonly tagNameCtrl = new FormControl('', { nonNullable: true });
  protected readonly tagValueCtrl = new FormControl('', { nonNullable: true });

  protected readonly projectTags = computed(() => this.project()?.tags ?? []);

  // ─── Agent Pool ───
  protected readonly agentPoolLoading = signal(false);
  protected readonly agentPoolName = signal<string | null>(null);
  protected readonly useCustomPool = signal(false);

  // ─── Diagnostics Validation ───
  protected readonly validatingDiagnostics = this.generationWorkflow.validatingDiagnostics;

  // ─── Project Bicep Generation (mono-repo) ───
  protected readonly projectBicepLoading = this.generationWorkflow.projectBicepLoading;
  protected readonly projectBicepResult = this.generationWorkflow.projectBicepResult;
  protected readonly projectBicepDownloading = this.generationWorkflow.projectBicepDownloading;
  protected readonly projectInfraArtifactsDownloading = this.generationWorkflow.projectInfraArtifactsDownloading;
  protected readonly projectBicepErrorKey = this.generationWorkflow.projectBicepErrorKey;
  protected readonly projectGenerationPanelCollapsed = this.generationWorkflow.projectGenerationPanelCollapsed;

  // ─── Project Pipeline Generation (mono-repo) ───
  protected readonly projectPipelineLoading = this.generationWorkflow.projectPipelineLoading;
  protected readonly projectPipelineResult = this.generationWorkflow.projectPipelineResult;
  protected readonly projectPipelineDownloading = this.generationWorkflow.projectPipelineDownloading;
  protected readonly projectCodeArtifactsDownloading = this.generationWorkflow.projectCodeArtifactsDownloading;
  protected readonly projectPipelineErrorKey = this.generationWorkflow.projectPipelineErrorKey;

  // ─── Project Bootstrap Pipeline Generation (Azure DevOps) ───
  protected readonly projectBootstrapLoading = this.generationWorkflow.projectBootstrapLoading;
  protected readonly projectBootstrapResult = this.generationWorkflow.projectBootstrapResult;
  protected readonly projectBootstrapDownloading = this.generationWorkflow.projectBootstrapDownloading;
  protected readonly projectBootstrapErrorKey = this.generationWorkflow.projectBootstrapErrorKey;
  protected readonly canPushAllProjectArtifacts = this.generationWorkflow.canPushAllProjectArtifacts;
  protected readonly isSplitInfraCodeLayout = this.generationWorkflow.isSplitInfraCodeLayout;

  // ─── Pipeline Variable Groups ───
  protected readonly variableGroups = signal<ProjectPipelineVariableGroupResponse[]>([]);
  protected readonly vgLoading = signal(false);
  protected readonly vgErrorKey = signal('');
  protected readonly vgLoaded = signal(false);

  protected readonly projectBicepNodes = this.generationWorkflow.projectBicepNodes;
  protected readonly loadProjectBicepFile = this.generationWorkflow.loadProjectBicepFile;
  protected readonly projectPipelineNodes = this.generationWorkflow.projectPipelineNodes;
  protected readonly loadProjectPipelineFile = this.generationWorkflow.loadProjectPipelineFile;
  protected readonly projectBootstrapNodes = this.generationWorkflow.projectBootstrapNodes;
  protected readonly loadProjectBootstrapFile = this.generationWorkflow.loadProjectBootstrapFile;

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
  private readonly currentTabQuery = computed(() => this.routeQueryParamMap().get('tab'));
  protected readonly selectedTabIndex = computed(() => getProjectDetailTabIndex(this.currentTabQuery()));

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) { // NOSONAR S3776 - tracked under test-debt #22
      this.loadError.set('PROJECT_DETAIL.ERROR.NO_ID');
      return;
    }

    void this.loadProject(id);
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
      this.agentPoolName.set(project.agentPoolName);
      this.useCustomPool.set(project.agentPoolName != null);
      this.sidebarContextService.setProjectContext(project.id, project.name);
      this.recentlyViewedService.trackView({
        id: project.id,
        name: project.name,
        type: 'project',
        description: project.description,
      });

      // Non-blocking: eagerly load pipeline variable groups for badge count
      this.loadVariableGroups().catch(() => {});
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

  // ─── Abbreviation Overrides ───

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

  protected isAbbreviationBusy(resourceType: string): boolean {
    const key = this.namingActionKey();
    return key === `abbr:${resourceType}` || key === `abbr-remove:${resourceType}`;
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

  private async saveAbbreviationOverride(resourceType: string, abbreviation: string): Promise<void> {
    const projectId = this.project()?.id;
    if (!projectId) return;

    this.namingActionKey.set(`abbr:${resourceType}`);
    this.namingErrorKey.set('');

    try {
      const request: SetResourceAbbreviationOverrideRequest = { abbreviation };
      await this.projectService.setResourceAbbreviation(projectId, resourceType, request);
      await this.refreshProject(projectId);
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
      await this.refreshProject(projectId);
    } catch {
      this.namingErrorKey.set('ABBREVIATIONS.RESET_ERROR');
    } finally {
      this.namingActionKey.set(null);
    }
  }

  private async refreshProject(projectId: string): Promise<void> {
    const refreshed = await this.projectService.getProject(projectId);
    this.project.set(refreshed);
  }

  // ─── Agent Pool ───

  protected onCustomPoolToggle(checked: boolean): void {
    this.useCustomPool.set(checked);
    if (!checked) {
      this.agentPoolName.set(null);
    }
  }

  protected async saveAgentPool(): Promise<void> {
    const projectId = this.project()?.id;
    if (!projectId) return;

    this.agentPoolLoading.set(true);
    try {
      await this.projectService.setAgentPool(projectId, {
        agentPoolName: this.agentPoolName() || null,
      });
      this.snackBar.open(
        this.translate.instant('PROJECT_DETAIL.SETTINGS.AGENT_POOL.SAVE_SUCCESS'),
        '✕',
        { duration: 3000 }
      );
    } catch {
      this.snackBar.open(
        this.translate.instant('PROJECT_DETAIL.SETTINGS.AGENT_POOL.SAVE_ERROR'),
        '✕',
        { duration: 5000, panelClass: 'error-snackbar' }
      );
    } finally {
      this.agentPoolLoading.set(false);
    }
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

  // ─── Project Tags ───

  protected startEditProjectTags(): void {
    this.editingTags.set(this.projectTags().map(t => ({ name: t.name, value: t.value })));
    this.tagNameCtrl.reset();
    this.tagValueCtrl.reset();
    this.tagsErrorKey.set('');
    this.isEditingProjectTags.set(true);
  }

  protected addProjectTag(): void {
    const name = this.tagNameCtrl.value.trim();
    const value = this.tagValueCtrl.value.trim();
    if (!name || !value) return;
    if (this.editingTags().some(t => t.name === name)) return;
    this.editingTags.update(tags => [...tags, { name, value }]);
    this.tagNameCtrl.reset();
    this.tagValueCtrl.reset();
  }

  protected removeProjectTag(name: string): void {
    this.editingTags.update(tags => tags.filter(t => t.name !== name));
  }

  protected cancelProjectTagsEdit(): void {
    this.isEditingProjectTags.set(false);
    this.tagsErrorKey.set('');
  }

  protected async saveProjectTags(): Promise<void> {
    const projectId = this.project()?.id;
    if (!projectId) return;

    this.tagsSaving.set(true);
    this.tagsErrorKey.set('');

    try {
      await this.projectService.setTags(projectId, { tags: this.editingTags() });
      this.project.update(p => p ? { ...p, tags: this.editingTags() } : p);
      this.isEditingProjectTags.set(false);
    } catch {
      this.tagsErrorKey.set('PROJECT_DETAIL.TAGS.SAVE_ERROR');
    } finally {
      this.tagsSaving.set(false);
    }
  }

  // ─── Project Generation Workflow ───

  protected readonly generateProjectBicep = this.generationWorkflow.generateProjectBicep;
  protected readonly generateProjectPipeline = this.generationWorkflow.generateProjectPipeline;
  protected readonly generateProjectBootstrap = this.generationWorkflow.generateProjectBootstrap;
  protected readonly projectGenerateAllLoading = this.generationWorkflow.projectGenerateAllLoading;
  protected readonly deferMonoRepoBatchReveal = this.generationWorkflow.deferMonoRepoBatchReveal;
  protected readonly projectGenerationPanelOpen = this.generationWorkflow.projectGenerationPanelOpen;
  protected readonly generateAll = this.generationWorkflow.generateAll;
  protected readonly toggleProjectGenerationPanelCollapsed = this.generationWorkflow.toggleProjectGenerationPanelCollapsed;
  protected readonly downloadProjectBicepFiles = this.generationWorkflow.downloadProjectBicepFiles;
  protected readonly downloadProjectInfraArtifacts = this.generationWorkflow.downloadProjectInfraArtifacts;
  protected readonly downloadProjectCodeArtifacts = this.generationWorkflow.downloadProjectCodeArtifacts;
  protected readonly openProjectPushAllToGitDialog = this.generationWorkflow.openProjectPushAllToGitDialog;
  protected readonly openProjectMultiRepoPushDialog = this.generationWorkflow.openProjectMultiRepoPushDialog;
  protected readonly downloadProjectPipelineFiles = this.generationWorkflow.downloadProjectPipelineFiles;
  protected readonly downloadProjectBootstrapFiles = this.generationWorkflow.downloadProjectBootstrapFiles;

  // ─── Pipeline Variable Groups ───

  protected onTabChange(index: number): void {
    const tab = getProjectDetailTabQuery(index);
    if (tab === this.currentTabQuery()) {
      return;
    }

    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab },
      queryParamsHandling: 'merge',
    });
  }

  protected async loadVariableGroups(): Promise<void> {
    const project = this.project();
    if (!project) return;

    this.vgLoading.set(true);
    this.vgErrorKey.set('');
    try {
      const groups = await this.projectService.getPipelineVariableGroups(project.id);
      this.variableGroups.set(groups.map(g => ({ ...g, variables: g.variables ?? [] })));
      this.vgLoaded.set(true);
    } catch {
      this.vgErrorKey.set('PROJECT_DETAIL.PIPELINE_VARIABLES.ERROR_ADD_GROUP');
    } finally {
      this.vgLoading.set(false);
    }
  }

  protected openAddVariableGroupDialog(): void {
    const dialogRef = this.dialog.open(AddVariableGroupDialogComponent, {
      width: '420px',
    });

    dialogRef.afterClosed().subscribe(async (groupName?: string) => {
      if (!groupName) return;
      const project = this.project();
      if (!project) return;

      this.vgErrorKey.set('');
      try {
        const newGroup = await this.projectService.addPipelineVariableGroup(project.id, { groupName });
        this.variableGroups.update(groups => [...groups, { ...newGroup, variables: newGroup.variables ?? [] }]);
      } catch {
        this.vgErrorKey.set('PROJECT_DETAIL.PIPELINE_VARIABLES.ERROR_ADD_GROUP');
      }
    });
  }

  protected openRemoveVariableGroupDialog(group: ProjectPipelineVariableGroupResponse): void {
    const data: ConfirmDialogData = {
      titleKey: 'PROJECT_DETAIL.PIPELINE_VARIABLES.REMOVE_GROUP',
      messageKey: 'PROJECT_DETAIL.PIPELINE_VARIABLES.CONFIRM_DELETE_GROUP',
      confirmKey: 'PROJECT_DETAIL.PIPELINE_VARIABLES.REMOVE_GROUP',
      cancelKey: 'CONFIG_DETAIL.PIPELINE_VARIABLES.DIALOG_CANCEL',
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, { width: '400px', data });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;
      const project = this.project();
      if (!project) return;

      this.vgErrorKey.set('');
      try {
        await this.projectService.removePipelineVariableGroup(project.id, group.id);
        this.variableGroups.update(groups => groups.filter(g => g.id !== group.id));
      } catch {
        this.vgErrorKey.set('PROJECT_DETAIL.PIPELINE_VARIABLES.ERROR_REMOVE_GROUP');
      }
    });
  }
}
