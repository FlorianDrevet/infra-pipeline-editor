import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';
import { SidebarContextService } from '../../../core/layouts/sidebar/sidebar-context.service';
import { ProjectResponse } from '../../../shared/interfaces/project.interface';
import { InfrastructureConfigResponse } from '../../../shared/interfaces/infra-config.interface';
import { ProjectLayoutPreset } from '../../../shared/interfaces/project-repository.interface';
import { ProjectService } from '../../../shared/services/project.service';
import {
  DsButtonComponent,
  DsCardComponent,
  DsPageHeaderComponent,
} from '../../../shared/components/ds';
import { LayoutRepositoriesComponent } from '../layout-repositories/layout-repositories.component';

const LAYOUT_PRESET_LABEL_KEYS: Record<ProjectLayoutPreset, string> = {
  AllInOne: 'PROJECT_DETAIL.LAYOUT.PRESET_ALL_IN_ONE',
  MultiRepo: 'PROJECT_DETAIL.LAYOUT.PRESET_MULTI_REPO',
  SplitInfraCode: 'PROJECT_DETAIL.LAYOUT.PRESET_SPLIT_INFRA_CODE',
};

@Component({
  selector: 'app-generation-config',
  standalone: true,
  imports: [
    TranslateModule,
    RouterLink,
    MatIconModule,
    MatProgressSpinnerModule,
    LayoutRepositoriesComponent,
    DsButtonComponent,
    DsCardComponent,
    DsPageHeaderComponent,
  ],
  templateUrl: './generation-config.component.html',
  styleUrl: './generation-config.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GenerationConfigComponent implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly sidebarContextService = inject(SidebarContextService);

  protected readonly project = signal<ProjectResponse | null>(null);
  protected readonly configs = signal<InfrastructureConfigResponse[]>([]);
  protected readonly isLoading = signal(true);

  protected readonly repositoryCount = computed(() => this.project()?.repositories?.length ?? 0);
  protected readonly configCount = computed(() => this.configs().length);
  protected readonly layoutPresetLabelKey = computed(() => {
    const preset = this.project()?.layoutPreset;
    if (!preset || !(preset in LAYOUT_PRESET_LABEL_KEYS)) {
      return 'PROJECT_DETAIL.BOARD.LAYOUT_PRESET_UNKNOWN';
    }
    return LAYOUT_PRESET_LABEL_KEYS[preset as ProjectLayoutPreset];
  });

  ngOnInit(): void {
    this.runTask(this.load());
  }

  private async load(): Promise<void> {
    this.isLoading.set(true);
    try {
      const id = this.route.snapshot.paramMap.get('id') ?? '';
      const [project, configs] = await Promise.all([
        this.projectService.getProject(id),
        this.projectService.getProjectConfigs(id),
      ]);
      this.project.set(project);
      this.configs.set(configs);
      this.sidebarContextService.setProjectContext(project.id, project.name);
    } finally {
      this.isLoading.set(false);
    }
  }

  protected navigateToGenerate(): void {
    const projectId = this.project()?.id;
    if (projectId) {
      this.router.navigate(['/projects', projectId, 'generate']).catch(() => undefined);
    }
  }

  protected openProjectDetail(): void {
    const projectId = this.project()?.id ?? this.route.snapshot.paramMap.get('id') ?? '';
    if (projectId) {
      this.router.navigate(['/projects', projectId]).catch(() => undefined);
    }
  }

  private runTask(taskPromise: Promise<void>): void {
    taskPromise.catch(() => undefined);
  }
}
