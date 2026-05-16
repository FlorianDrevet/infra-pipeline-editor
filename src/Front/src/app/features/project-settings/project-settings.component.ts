import { Component, OnDestroy, OnInit, computed, effect, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';

import { ProjectResponse } from '../../shared/interfaces/project.interface';
import { ProjectService } from '../../shared/services/project.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { PageContextService } from '../../shared/services/page-context.service';
import { SidebarContextService } from '../../core/layouts/sidebar/sidebar-context.service';
import { DsButtonComponent, DsTextFieldComponent, DsToggleComponent } from '../../shared/components/ds';
import {
  hasProjectDetailAgentPoolChanges,
  resolveProjectDetailAgentPoolDraft,
  resolveProjectDetailAgentPoolValue,
  toggleProjectDetailAgentPoolDraft,
} from '../project-detail/project-detail-agent-pool.helper';

@Component({
  selector: 'app-project-settings',
  standalone: true,
  imports: [
    TranslateModule,
    RouterLink,
    FormsModule,
    MatIconModule,
    MatProgressSpinnerModule,
    DsButtonComponent,
    DsTextFieldComponent,
    DsToggleComponent,
  ],
  templateUrl: './project-settings.component.html',
  styleUrl: './project-settings.component.scss',
})
export class ProjectSettingsComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly projectService = inject(ProjectService);
  private readonly authService = inject(AuthenticationService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  private readonly pageContextService = inject(PageContextService);
  private readonly sidebarContextService = inject(SidebarContextService);

  protected readonly project = signal<ProjectResponse | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly loadError = signal('');

  // ─── Agent Pool ───
  protected readonly agentPoolLoading = signal(false);
  protected readonly agentPoolName = signal<string | null>(null);
  protected readonly useCustomPool = signal(false);
  protected readonly isAgentPoolDirty = computed(() =>
    hasProjectDetailAgentPoolChanges(this.project()?.agentPoolName, {
      useCustomPool: this.useCustomPool(),
      agentPoolName: this.agentPoolName(),
    })
  );

  protected readonly canWrite = computed(() => {
    const oid = this.authService.getMsalAccount?.localAccountId;
    if (!oid) return false;
    const members = this.project()?.members ?? [];
    const me = members.find((m) => m.entraId === oid);
    return me?.role === 'Owner' || me?.role === 'Contributor';
  });

  private readonly breadcrumbEffect = effect(() => {
    const project = this.project();
    const projectsLabel = this.translate.instant('NAV.BREADCRUMB.PROJECTS') as string;
    const settingsLabel = this.translate.instant('PROJECT_SETTINGS.BREADCRUMB') as string;
    const segments = project
      ? [
          { label: projectsLabel, routerLink: '/' },
          { label: project.name, routerLink: `/projects/${project.id}` },
          { label: settingsLabel },
        ]
      : [{ label: projectsLabel, routerLink: '/' }];
    this.pageContextService.setBreadcrumb(segments);
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loadError.set('PROJECT_SETTINGS.ERROR.NO_ID');
      return;
    }
    void this.loadProject(id);
  }

  ngOnDestroy(): void {
    this.pageContextService.clear();
  }

  private async loadProject(id: string): Promise<void> {
    this.isLoading.set(true);
    this.loadError.set('');

    try {
      const project = await this.projectService.getProject(id);
      this.project.set(project);
      this.syncAgentPoolDraft(project.agentPoolName);
      this.sidebarContextService.setProjectContext(project.id, project.name);
    } catch {
      this.loadError.set('PROJECT_SETTINGS.ERROR.LOAD_FAILED');
    } finally {
      this.isLoading.set(false);
    }
  }

  private syncAgentPoolDraft(agentPoolName: string | null | undefined): void {
    const draft = resolveProjectDetailAgentPoolDraft(agentPoolName);
    this.agentPoolName.set(draft.agentPoolName);
    this.useCustomPool.set(draft.useCustomPool);
  }

  protected onCustomPoolToggle(checked: boolean): void {
    const next = toggleProjectDetailAgentPoolDraft(
      { useCustomPool: this.useCustomPool(), agentPoolName: this.agentPoolName() },
      checked
    );
    this.useCustomPool.set(next.useCustomPool);
    this.agentPoolName.set(next.agentPoolName);
  }

  protected async saveAgentPool(): Promise<void> {
    const project = this.project();
    if (!project || !this.isAgentPoolDirty()) return;

    const agentPoolName = resolveProjectDetailAgentPoolValue({
      useCustomPool: this.useCustomPool(),
      agentPoolName: this.agentPoolName(),
    });

    this.agentPoolLoading.set(true);
    try {
      await this.projectService.setAgentPool(project.id, { agentPoolName });
      this.project.set({ ...project, agentPoolName });
      this.syncAgentPoolDraft(agentPoolName);
      this.snackBar.open(
        this.translate.instant('PROJECT_SETTINGS.AGENT_POOL.SAVE_SUCCESS'),
        '✕',
        { duration: 3000 }
      );
    } catch {
      this.snackBar.open(
        this.translate.instant('PROJECT_SETTINGS.AGENT_POOL.SAVE_ERROR'),
        '✕',
        { duration: 5000, panelClass: 'error-snackbar' }
      );
    } finally {
      this.agentPoolLoading.set(false);
    }
  }
}
