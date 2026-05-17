import { Component, OnDestroy, OnInit, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';

import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ProjectResponse } from '../../shared/interfaces/project.interface';
import {
  InfrastructureConfigResponse,
  EnvironmentDefinitionResponse,
} from '../../shared/interfaces/infra-config.interface';
import { ProjectService } from '../../shared/services/project.service';
import { InfraConfigService } from '../../shared/services/infra-config.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { RecentlyViewedService } from '../../shared/services/recently-viewed.service';
import { PageContextService } from '../../shared/services/page-context.service';
import { SidebarContextService } from '../../core/layouts/sidebar/sidebar-context.service';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { AddConfigDialogComponent, AddConfigDialogData } from './add-config-dialog/add-config-dialog.component';
import {
  AddProjectEnvironmentDialogComponent,
  AddProjectEnvironmentDialogData,
} from './add-project-environment-dialog/add-project-environment-dialog.component';
import { ProjectDetailEnvironmentsSectionComponent } from './environments-section/project-detail-environments-section.component';
import { ProjectDetailTagsSectionComponent } from './tags-section/project-detail-tags-section.component';
import { ProjectDetailVariableGroupsSectionComponent } from './variable-groups-section/project-detail-variable-groups-section.component';
import { ProjectDetailNamingSectionComponent } from './naming-section/project-detail-naming-section.component';

import { ProjectDetailGenerationWorkflowService } from './project-detail-generation-workflow.service';
import { getProjectDetailTabIndex, getProjectDetailTabQuery, isProjectDetailTab } from '../../shared/enums/detail-route-tabs';


@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [
    TranslateModule,
    RouterLink,
    ProjectDetailEnvironmentsSectionComponent,
    ProjectDetailTagsSectionComponent,
    ProjectDetailVariableGroupsSectionComponent,
    ProjectDetailNamingSectionComponent,
    MatButtonModule,
    MatButtonToggleModule,
    MatDialogModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    MatTooltipModule,
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
  protected readonly isLoading = signal(false);
  protected readonly loadError = signal('');
  protected readonly configErrorKey = signal('');
  protected readonly envActionId = signal<string | null>(null);
  protected readonly envErrorKey = signal('');

  protected readonly sortedEnvironments = computed(() => {
    const envs = this.project()?.environmentDefinitions ?? [];
    return [...envs].sort((a, b) => a.order - b.order);
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
  private readonly invalidTabNormalizationEffect = effect(() => {
    const currentTab = this.currentTabQuery();
    if (currentTab === null || isProjectDetailTab(currentTab)) {
      return;
    }

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: null },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  });
  protected readonly selectedTabIndex = computed(() => getProjectDetailTabIndex(this.currentTabQuery()));

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) { // NOSONAR S3776 - tracked under test-debt #22
      this.loadError.set('PROJECT_DETAIL.ERROR.NO_ID');
      return;
    }

    this.loadProject(id).catch(() => undefined);
  }

  private async loadProject(id: string): Promise<void> {
    this.isLoading.set(true);
    this.loadError.set('');

    try {
      const [project, configs] = await Promise.all([
        this.projectService.getProject(id),
        this.projectService.getProjectConfigs(id),
      ]);
      this.project.set(project);
      this.configs.set(configs);
      this.sidebarContextService.setProjectContext(project.id, project.name);
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

  // ─── Project Generation Workflow ───

  protected navigateToGenerate(): void {
    const projectId = this.project()?.id;
    if (!projectId) return;
    this.router.navigate(['/projects', projectId, 'generate']).catch(() => undefined);
  }

  protected async onTabChange(index: number): Promise<void> {
    const tab = getProjectDetailTabQuery(index);
    if (tab === this.currentTabQuery()) {
      return;
    }

    await this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab },
      queryParamsHandling: 'merge',
    });
  }

  protected onProjectChange(updated: ProjectResponse): void {
    this.project.set(updated);
  }

  private async refreshProject(projectId: string): Promise<void> {
    const refreshed = await this.projectService.getProject(projectId);
    this.project.set(refreshed);
  }
}
