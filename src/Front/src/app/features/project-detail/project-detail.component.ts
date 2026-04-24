import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { trigger, transition, style, animate } from '@angular/animations';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ProjectResponse, ProjectMemberResponse, GenerateProjectBicepResponse, GenerateProjectPipelineResponse, GenerateProjectBootstrapPipelineResponse, ProjectPipelineVariableGroupResponse, SetProjectTagsRequest } from '../../shared/interfaces/project.interface';
import {
  InfrastructureConfigResponse,
  EnvironmentDefinitionResponse,
  ResourceNamingTemplateResponse,
  ResourceAbbreviationOverrideResponse,
  SetResourceAbbreviationOverrideRequest,
  TagRequest,
} from '../../shared/interfaces/infra-config.interface';
import { UserResponse } from '../../shared/interfaces/infra-config.interface';
import { ProjectService } from '../../shared/services/project.service';
import { InfraConfigService } from '../../shared/services/infra-config.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { RecentlyViewedService } from '../../shared/services/recently-viewed.service';
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
import { LayoutRepositoriesComponent } from './layout-repositories/layout-repositories.component';
import { SplitGenerationSwitcherComponent } from './split-generation-switcher/split-generation-switcher.component';
import {
  PushToGitDialogComponent,
  PushToGitDialogData,
} from '../config-detail/push-to-git-dialog/push-to-git-dialog.component';
import { RESOURCE_TYPE_OPTIONS, RESOURCE_TYPE_ABBREVIATIONS, RESOURCE_TYPE_ICONS } from '../config-detail/enums/resource-type.enum';
import { AddVariableGroupDialogComponent } from '../config-detail/add-variable-group-dialog/add-variable-group-dialog.component';
import { saveAs } from 'file-saver';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatChipsModule } from '@angular/material/chips';
import { BicepFilePanelComponent, BicepFileNode, BicepFolderNode, BicepFileType, BicepTreeNode } from '../../shared/components/bicep-file-panel/bicep-file-panel.component';
import {
  GenerationDiagnosticsDialogComponent,
  GenerationDiagnosticsDialogData,
  ConfigDiagnosticGroup,
  ConfigMissingEnvGroup,
  MissingEnvResource,
} from '../../shared/components/generation-diagnostics-dialog/generation-diagnostics-dialog.component';
import { ResourceGroupService } from '../../shared/services/resource-group.service';
import { AzureResourceResponse } from '../../shared/interfaces/resource-group.interface';
import { firstValueFrom } from 'rxjs';

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
    SplitGenerationSwitcherComponent,
    MatButtonModule,
    MatButtonToggleModule,
    MatChipsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatTabsModule,
    MatTooltipModule,
  ],
  templateUrl: './project-detail.component.html',
  styleUrl: './project-detail.component.scss',
  animations: [
    trigger('slideIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(-8px)' }),
        animate('200ms ease-out', style({ opacity: 1, transform: 'translateY(0)' })),
      ]),
    ]),
  ],
})
export class ProjectDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly projectService = inject(ProjectService);
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly authService = inject(AuthenticationService);
  private readonly recentlyViewedService = inject(RecentlyViewedService);
  private readonly dialog = inject(MatDialog);
  private readonly resourceGroupService = inject(ResourceGroupService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);

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
  protected readonly validatingDiagnostics = signal(false);

  // ─── Project Bicep Generation (mono-repo) ───
  protected readonly projectBicepLoading = signal(false);
  protected readonly projectBicepResult = signal<GenerateProjectBicepResponse | null>(null);
  protected readonly projectBicepDownloading = signal(false);
  protected readonly projectBicepErrorKey = signal('');
  protected readonly projectBicepPanelOpen = signal(false);
  protected readonly projectBicepPanelCollapsed = signal(false);

  // ─── Project Pipeline Generation (mono-repo) ───
  protected readonly projectPipelineLoading = signal(false);
  protected readonly projectPipelineResult = signal<GenerateProjectPipelineResponse | null>(null);
  protected readonly projectPipelineDownloading = signal(false);
  protected readonly projectPipelineErrorKey = signal('');
  protected readonly projectPipelinePanelOpen = signal(false);
  protected readonly projectPipelinePanelCollapsed = signal(false);

  // ─── Project Bootstrap Pipeline Generation (Azure DevOps) ───
  protected readonly projectBootstrapLoading = signal(false);
  protected readonly projectBootstrapResult = signal<GenerateProjectBootstrapPipelineResponse | null>(null);
  protected readonly projectBootstrapDownloading = signal(false);
  protected readonly projectBootstrapErrorKey = signal('');
  protected readonly projectBootstrapPanelOpen = signal(false);
  protected readonly canPushAllProjectArtifacts = computed(
    () => this.projectBicepResult() !== null
      && this.projectPipelineResult() !== null
      && this.projectBootstrapResult() !== null,
  );

  // ─── Pipeline Variable Groups ───
  protected readonly variableGroups = signal<ProjectPipelineVariableGroupResponse[]>([]);
  protected readonly vgLoading = signal(false);
  protected readonly vgErrorKey = signal('');
  protected readonly vgLoaded = signal(false);

  protected readonly projectBicepNodes = computed<BicepTreeNode[]>(() => {
    const result = this.projectBicepResult();
    if (!result) return [];
    const nodes: BicepTreeNode[] = [];

    // ── Common/ folder ──
    // Backend stores keys with "Common/" prefix (e.g. "Common/types.bicep", "Common/modules/WebApp/...").
    // Strip it so filters like key.startsWith('modules/') and path templates work correctly.
    const commonEntries = Object.entries(result.commonFileUris)
      .map(([key, uri]) => [key.startsWith('Common/') ? key.slice('Common/'.length) : key, uri] as [string, string]);
    if (commonEntries.length > 0) {
      nodes.push({ kind: 'folder', key: 'Common', name: 'Common/', badge: 'PROJECT_DETAIL.BICEP.SHARED', badgeVariant: 'types', folderIcon: 'folder_shared', depth: 0 } satisfies BicepFolderNode);

      for (const [key, uri] of commonEntries) {
        if (key.startsWith('modules/')) continue;
        const type: BicepFileType =
          key === 'types.bicep' ? 'types'
          : key === 'functions.bicep' ? 'functions'
          : key === 'constants.bicep' ? 'constants'
          : 'generic';
        nodes.push({ kind: 'file', path: `Common/${key}`, displayName: key, type, uri: `Common/${key}`, depth: 1, parentFolderKey: 'Common' } satisfies BicepFileNode);
      }

      const moduleEntries = commonEntries.filter(([key]) => key.startsWith('modules/'));
      if (moduleEntries.length > 0) {
        nodes.push({ kind: 'folder', key: 'Common/modules', name: 'modules/', folderIcon: 'folder', depth: 1, parentFolderKey: 'Common' } satisfies BicepFolderNode);
        const folderMap = new Map<string, { name: string; files: Array<{ path: string; displayName: string; uri: string }> }>();
        for (const [filePath, uri] of moduleEntries) {
          const parts = filePath.split('/');
          if (parts.length < 3) continue;
          const fn = parts[1];
          const folderKey = `Common/modules/${fn}`;
          if (!folderMap.has(folderKey)) folderMap.set(folderKey, { name: fn, files: [] });
          folderMap.get(folderKey)!.files.push({ path: `Common/${filePath}`, displayName: parts[2], uri });
        }
        for (const [folderKey, folder] of folderMap) {
          nodes.push({ kind: 'folder', key: folderKey, name: `${folder.name}/`, folderIcon: 'folder', depth: 2, parentFolderKey: 'Common/modules' } satisfies BicepFolderNode);
          for (const file of folder.files) {
            const type: BicepFileType =
              file.displayName === 'types.bicep' ? 'types'
              : file.displayName.endsWith('.roleassignments.module.bicep') ? 'role-assignments'
              : 'module-type';
            nodes.push({ kind: 'file', path: file.path, displayName: file.displayName, type, uri: file.path, depth: 3, parentFolderKey: folderKey } satisfies BicepFileNode);
          }
        }
      }
    }

    // ── Per-config folders ──
    for (const [configName, files] of Object.entries(result.configFileUris)) {
      nodes.push({ kind: 'folder', key: configName, name: `${configName}/`, folderIcon: 'folder', depth: 0 } satisfies BicepFolderNode);
      for (const [fileName, uri] of Object.entries(files)) {
        const parts = fileName.split('/');
        if (parts.length > 1) {
          // File is nested (e.g. "parameters/main.dev.bicepparam") — create intermediate folder nodes.
          let parentKey = configName;
          for (let i = 0; i < parts.length - 1; i++) {
            const folderKey = `${configName}/${parts.slice(0, i + 1).join('/')}`;
            if (!nodes.some(n => n.kind === 'folder' && n.key === folderKey)) {
              nodes.push({ kind: 'folder', key: folderKey, name: `${parts[i]}/`, folderIcon: 'folder', depth: i + 1, parentFolderKey: parentKey } satisfies BicepFolderNode);
            }
            parentKey = folderKey;
          }
          const displayName = parts[parts.length - 1];
          const type: BicepFileType =
            displayName.endsWith('.bicepparam') ? 'params'
            : displayName.endsWith('.roleassignments.module.bicep') ? 'role-assignments'
            : 'generic';
          nodes.push({ kind: 'file', path: `${configName}/${fileName}`, displayName, type, uri: `${configName}/${fileName}`, depth: parts.length, parentFolderKey: parentKey } satisfies BicepFileNode);
        } else {
          const type: BicepFileType =
            fileName === 'main.bicep' ? 'entry-point'
            : fileName.endsWith('.bicepparam') ? 'params'
            : fileName.endsWith('.roleassignments.module.bicep') ? 'role-assignments'
            : 'generic';
          nodes.push({ kind: 'file', path: `${configName}/${fileName}`, displayName: fileName, type, uri: `${configName}/${fileName}`, depth: 1, parentFolderKey: configName } satisfies BicepFileNode);
        }
      }
    }

    return nodes;
  });

  protected readonly loadProjectBicepFile = (filePath: string): Promise<string> => {
    const projectId = this.project()?.id ?? '';
    return this.projectService.getProjectBicepFileContent(projectId, filePath);
  };

  protected readonly projectPipelineNodes = computed<BicepTreeNode[]>(() => {
    const result = this.projectPipelineResult();
    if (!result) return [];
    const nodes: BicepTreeNode[] = [];

    // ── .azuredevops/ common folder ──
    if (result.commonFileUris && Object.keys(result.commonFileUris).length > 0) {
      nodes.push({ kind: 'folder', key: '.azuredevops', name: '.azuredevops/', folderIcon: 'folder_shared', depth: 0 } satisfies BicepFolderNode);

      // Group common files by subfolder (e.g. "pipelines", "jobs", "steps")
      const subfolderMap = new Map<string, Array<{ path: string; displayName: string }>>();
      for (const [key] of Object.entries(result.commonFileUris)) {
        // Keys are like ".azuredevops/pipelines/ci.pipeline.yml"
        const relativePath = key.startsWith('.azuredevops/') ? key.slice('.azuredevops/'.length) : key;
        const parts = relativePath.split('/');
        if (parts.length >= 3) {
          // e.g. "pipelines" → folder, "ci.pipeline.yml" → file
          const subfolderKey = parts.slice(0, parts.length - 1).join('/');
          if (!subfolderMap.has(subfolderKey)) subfolderMap.set(subfolderKey, []);
          subfolderMap.get(subfolderKey)!.push({ path: key, displayName: parts[parts.length - 1] });
        } else if (parts.length === 2) {
          const subfolderKey = parts[0];
          if (!subfolderMap.has(subfolderKey)) subfolderMap.set(subfolderKey, []);
          subfolderMap.get(subfolderKey)!.push({ path: key, displayName: parts[1] });
        } else {
          // File directly under .azuredevops/
          nodes.push({ kind: 'file', path: key, displayName: parts[0], type: 'generic', uri: key, depth: 1, parentFolderKey: '.azuredevops' } satisfies BicepFileNode);
        }
      }

      for (const [subfolderPath, files] of subfolderMap) {
        // Build nested folder structure: .azuredevops > pipelines|jobs|steps
        const subParts = subfolderPath.split('/');
        let parentKey = '.azuredevops';
        for (let i = 0; i < subParts.length; i++) {
          const folderKey = `.azuredevops/${subParts.slice(0, i + 1).join('/')}`;
          // Only push if not already added
          if (!nodes.some(n => n.kind === 'folder' && n.key === folderKey)) {
            nodes.push({ kind: 'folder', key: folderKey, name: `${subParts[i]}/`, folderIcon: 'folder', depth: i + 1, parentFolderKey: parentKey } satisfies BicepFolderNode);
          }
          parentKey = folderKey;
        }
        // Push files under the deepest subfolder
        for (const file of files) {
          nodes.push({ kind: 'file', path: file.path, displayName: file.displayName, type: 'generic', uri: file.path, depth: subParts.length + 1, parentFolderKey: parentKey } satisfies BicepFileNode);
        }
      }
    }

    // ── Per-config folders ──
    for (const [configName, files] of Object.entries(result.configFileUris)) {
      nodes.push({ kind: 'folder', key: configName, name: `${configName}/`, folderIcon: 'folder', depth: 0 } satisfies BicepFolderNode);
      for (const [fileName] of Object.entries(files)) {
        // Build nested subfolders for files like "variables/dev.yml"
        const parts = fileName.split('/');
        if (parts.length > 1) {
          let parentKey = configName;
          for (let i = 0; i < parts.length - 1; i++) {
            const folderKey = `${configName}/${parts.slice(0, i + 1).join('/')}`;
            if (!nodes.some(n => n.kind === 'folder' && n.key === folderKey)) {
              nodes.push({ kind: 'folder', key: folderKey, name: `${parts[i]}/`, folderIcon: 'folder', depth: i + 1, parentFolderKey: parentKey } satisfies BicepFolderNode);
            }
            parentKey = folderKey;
          }
          nodes.push({ kind: 'file', path: `${configName}/${fileName}`, displayName: parts[parts.length - 1], type: 'generic', uri: `${configName}/${fileName}`, depth: parts.length, parentFolderKey: parentKey } satisfies BicepFileNode);
        } else {
          nodes.push({ kind: 'file', path: `${configName}/${fileName}`, displayName: fileName, type: 'generic', uri: `${configName}/${fileName}`, depth: 1, parentFolderKey: configName } satisfies BicepFileNode);
        }
      }
    }

    return nodes;
  });

  protected readonly loadProjectPipelineFile = (filePath: string): Promise<string> => {
    const projectId = this.project()?.id ?? '';
    return this.projectService.getProjectPipelineFileContent(projectId, filePath);
  };

  protected readonly projectBootstrapNodes = computed<BicepTreeNode[]>(() => {
    const result = this.projectBootstrapResult();
    if (!result) return [];
    const nodes: BicepTreeNode[] = [];
    for (const [fileName] of Object.entries(result.fileUris)) {
      nodes.push({
        kind: 'file',
        path: fileName,
        displayName: fileName,
        type: 'generic',
        uri: fileName,
        depth: 0,
        parentFolderKey: '',
      } satisfies BicepFileNode);
    }
    return nodes;
  });

  protected readonly loadProjectBootstrapFile = (filePath: string): Promise<string> => {
    const projectId = this.project()?.id ?? '';
    return this.projectService.getProjectBootstrapPipelineFileContent(projectId, filePath);
  };

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
      this.agentPoolName.set(project.agentPoolName);
      this.useCustomPool.set(project.agentPoolName != null);
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

  // ─── Abbreviation Overrides ───

  protected readonly abbreviationDisplayItems = computed(() => {
    const proj = this.project();
    if (!proj) return [];

    const overrides = proj.resourceAbbreviations ?? [];
    const overrideMap = new Map(overrides.map(o => [o.resourceType, o.abbreviation]));

    const usedTypes = proj.usedResourceTypes ?? [];

    return usedTypes
      .sort()
      .map(rt => {
        const defaultAbbr = RESOURCE_TYPE_ABBREVIATIONS[rt] ?? rt.toLowerCase();
        const customAbbr = overrideMap.get(rt);
        return {
          resourceType: rt,
          icon: RESOURCE_TYPE_ICONS[rt] ?? 'widgets',
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

  // ─── Project Bicep Generation (mono-repo) ───

  protected async generateProjectBicep(): Promise<void> {
    const projectId = this.project()?.id;
    if (!projectId || this.projectBicepLoading()) return;

    const shouldContinue = await this.checkProjectDiagnostics();
    if (!shouldContinue) return;

    await this.doGenerateProjectBicep();
  }

  private async doGenerateProjectBicep(): Promise<void> {
    const projectId = this.project()?.id;
    if (!projectId || this.projectBicepLoading()) return;

    this.projectBicepLoading.set(true);
    this.projectBicepErrorKey.set('');
    this.projectBicepResult.set(null);
    this.projectBicepPanelOpen.set(true);

    try {
      const result = await this.projectService.generateProjectBicep(projectId);
      this.projectBicepResult.set(result);
    } catch {
      this.projectBicepErrorKey.set('PROJECT_DETAIL.BICEP.GENERATE_ERROR');
    } finally {
      this.projectBicepLoading.set(false);
    }
  }

  // ─── Project Pipeline Generation (mono-repo) ───

  protected async generateProjectPipeline(): Promise<void> {
    const projectId = this.project()?.id;
    if (!projectId || this.projectPipelineLoading()) return;

    const shouldContinue = await this.checkProjectDiagnostics();
    if (!shouldContinue) return;

    await this.doGenerateProjectPipeline();
  }

  private async doGenerateProjectPipeline(): Promise<void> {
    const projectId = this.project()?.id;
    if (!projectId || this.projectPipelineLoading()) return;

    this.projectPipelineLoading.set(true);
    this.projectPipelineErrorKey.set('');
    this.projectPipelineResult.set(null);
    this.projectPipelinePanelOpen.set(true);

    try {
      const result = await this.projectService.generateProjectPipeline(projectId);
      this.projectPipelineResult.set(result);
    } catch {
      this.projectPipelineErrorKey.set('PROJECT_DETAIL.PIPELINE.GENERATE_ERROR');
    } finally {
      this.projectPipelineLoading.set(false);
    }
  }

  // ─── Unified Generate All (mono-repo) ───

  protected readonly projectGenerateAllLoading = computed(
    () => this.validatingDiagnostics() || this.projectBicepLoading() || this.projectPipelineLoading() || this.projectBootstrapLoading(),
  );

  protected readonly projectGenerationPanelOpen = computed(
    () => this.projectBicepPanelOpen() || this.projectPipelinePanelOpen() || this.projectBootstrapPanelOpen() || this.projectBicepLoading() || this.projectPipelineLoading() || this.projectBootstrapLoading(),
  );

  protected async generateAll(): Promise<void> {
    const projectId = this.project()?.id;
    if (!projectId || this.projectGenerateAllLoading()) return;

    this.validatingDiagnostics.set(true);
    try {
      const shouldContinue = await this.checkProjectDiagnostics();
      if (!shouldContinue) return;
    } finally {
      this.validatingDiagnostics.set(false);
    }

    // Launch all generations in parallel
    await Promise.all([
      this.doGenerateProjectBicep(),
      this.doGenerateProjectPipeline(),
      this.doGenerateProjectBootstrap(),
    ]);
  }

  protected closeProjectGenerationPanel(): void {
    this.closeProjectBicepPanel();
    this.closeProjectPipelinePanel();
    this.closeProjectBootstrapPanel();
  }

  // ─── Generation Diagnostics Dialog ───

  private async checkProjectDiagnostics(): Promise<boolean> {
    const allConfigs = this.configs();
    if (allConfigs.length === 0) return true;

    const project = this.project();
    const allEnvNames = (project?.environmentDefinitions ?? [])
      .sort((a, b) => a.order - b.order)
      .map(e => e.name);

    const ENV_SETTINGS_EXCLUDED_TYPES = new Set(['UserAssignedIdentity']);

    const results = await Promise.all(
      allConfigs.map(async (config) => {
        try {
          const [diagResult, rgs] = await Promise.all([
            this.infraConfigService.getDiagnostics(config.id),
            this.infraConfigService.getResourceGroups(config.id),
          ]);

          const rgResources = await Promise.all(
            rgs.map(rg => this.resourceGroupService.getResources(rg.id).catch(() => [] as AzureResourceResponse[])),
          );

          const missingEnvResources: MissingEnvResource[] = [];
          for (const resources of rgResources) {
            for (const resource of resources) {
              if (resource.isExisting) continue;
              if (ENV_SETTINGS_EXCLUDED_TYPES.has(resource.resourceType)) continue;
              const configured = new Set(resource.configuredEnvironments ?? []);
              const missing = allEnvNames.filter(name => !configured.has(name));
              if (missing.length > 0) {
                missingEnvResources.push({
                  resourceId: resource.id,
                  resourceName: resource.name,
                  resourceType: resource.resourceType,
                  missingEnvironments: missing,
                });
              }
            }
          }

          return { config, diagnostics: diagResult.diagnostics, missingEnvResources };
        } catch {
          return { config, diagnostics: [], missingEnvResources: [] as MissingEnvResource[] };
        }
      }),
    );

    const configsWithIssues: ConfigDiagnosticGroup[] = results
      .filter(r => r.diagnostics.length > 0)
      .map(r => ({
        configId: r.config.id,
        configName: r.config.name,
        diagnostics: r.diagnostics,
      }));

    const configsWithMissingEnvs: ConfigMissingEnvGroup[] = results
      .filter(r => r.missingEnvResources.length > 0)
      .map(r => ({
        configId: r.config.id,
        configName: r.config.name,
        resources: r.missingEnvResources,
      }));

    if (configsWithIssues.length === 0 && configsWithMissingEnvs.length === 0) return true;

    const dialogRef = this.dialog.open(GenerationDiagnosticsDialogComponent, {
      data: {
        configDiagnostics: configsWithIssues,
        missingEnvConfigs: configsWithMissingEnvs.length > 0 ? configsWithMissingEnvs : undefined,
      } satisfies GenerationDiagnosticsDialogData,
      width: '640px',
      maxHeight: '80vh',
    });
    const result = await firstValueFrom(dialogRef.afterClosed());
    return result === true;
  }

  protected closeProjectBicepPanel(): void {
    this.projectBicepPanelOpen.set(false);
    this.projectBicepResult.set(null);
    this.projectBicepErrorKey.set('');
  }

  protected async downloadProjectBicepFiles(): Promise<void> {
    const project = this.project();
    const result = this.projectBicepResult();

    if (!project?.id || !result || this.projectBicepDownloading()) return;

    this.projectBicepDownloading.set(true);
    try {
      const blob = await this.projectService.downloadProjectZip(project.id);
      const projectName = project.name ?? 'project';
      saveAs(blob, `${projectName}-bicep.zip`);
    } finally {
      this.projectBicepDownloading.set(false);
    }
  }



  protected openProjectPushAllToGitDialog(): void {
    const project = this.project();
    if (!project || !(project.repositories?.length)) return;

    const data: PushToGitDialogData = {
      configId: '', // Not used for project push
      projectId: project.id,
      isProjectLevel: true,
      isCombinedProjectPush: true,
    };
    this.dialog.open(PushToGitDialogComponent, { width: '480px', data });
  }

  protected closeProjectPipelinePanel(): void {
    this.projectPipelinePanelOpen.set(false);
    this.projectPipelineResult.set(null);
    this.projectPipelineErrorKey.set('');
  }

  protected async downloadProjectPipelineFiles(): Promise<void> {
    const project = this.project();
    const result = this.projectPipelineResult();

    if (!project?.id || !result || this.projectPipelineDownloading()) return;

    this.projectPipelineDownloading.set(true);
    try {
      const blob = await this.projectService.downloadProjectPipelineZip(project.id);
      const projectName = project.name ?? 'project';
      saveAs(blob, `${projectName}-pipeline.zip`);
    } finally {
      this.projectPipelineDownloading.set(false);
    }
  }

  // ─── Project Bootstrap Pipeline Generation (Azure DevOps) ───

  protected async generateProjectBootstrap(): Promise<void> {
    const projectId = this.project()?.id;
    if (!projectId || this.projectBootstrapLoading()) return;
    await this.doGenerateProjectBootstrap();
  }

  private async doGenerateProjectBootstrap(): Promise<void> {
    const projectId = this.project()?.id;
    if (!projectId || this.projectBootstrapLoading()) return;

    this.projectBootstrapLoading.set(true);
    this.projectBootstrapErrorKey.set('');
    this.projectBootstrapResult.set(null);
    this.projectBootstrapPanelOpen.set(true);

    try {
      const result = await this.projectService.generateProjectBootstrapPipeline(projectId);
      this.projectBootstrapResult.set(result);
    } catch {
      this.projectBootstrapErrorKey.set('PROJECT_DETAIL.BOOTSTRAP.GENERATE_ERROR');
    } finally {
      this.projectBootstrapLoading.set(false);
    }
  }

  protected closeProjectBootstrapPanel(): void {
    this.projectBootstrapPanelOpen.set(false);
    this.projectBootstrapResult.set(null);
    this.projectBootstrapErrorKey.set('');
  }

  protected async downloadProjectBootstrapFiles(): Promise<void> {
    const project = this.project();
    const result = this.projectBootstrapResult();

    if (!project?.id || !result || this.projectBootstrapDownloading()) return;

    this.projectBootstrapDownloading.set(true);
    try {
      const blob = await this.projectService.downloadProjectBootstrapPipelineZip(project.id);
      const projectName = project.name ?? 'project';
      saveAs(blob, `${projectName}-bootstrap.zip`);
    } finally {
      this.projectBootstrapDownloading.set(false);
    }
  }

  // ─── Pipeline Variable Groups ───

  protected onTabChange(_index: number): void {
    // Variable groups are now loaded eagerly in loadProject()
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
