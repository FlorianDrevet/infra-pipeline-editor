import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ProjectResponse } from '../../../shared/interfaces/project.interface';
import { InfrastructureConfigResponse } from '../../../shared/interfaces/infra-config.interface';
import { ProjectRepositoryResponse } from '../../../shared/interfaces/project-repository.interface';
import { ProjectService } from '../../../shared/services/project.service';
import { DsButtonComponent } from '../../../shared/components/ds';
import {
  PushToGitDialogComponent,
  PushToGitDialogData,
} from '../../config-detail/push-to-git-dialog/push-to-git-dialog.component';
import {
  MultiRepoPushDialogComponent,
  MultiRepoPushDialogData,
} from '../multi-repo-push-dialog/multi-repo-push-dialog.component';

type BoardTopology = 'single' | 'split' | 'mixed' | 'empty' | 'split-infra-code';

interface AliasGroup {
  readonly alias: string;
  readonly repo: ProjectRepositoryResponse | null;
  readonly configs: InfrastructureConfigResponse[];
}

const DEFAULT_ALIAS = 'default';

@Component({
  selector: 'app-generation-board',
  standalone: true,
  imports: [
    TranslateModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatDialogModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    DsButtonComponent,
  ],
  templateUrl: './generation-board.component.html',
  styleUrl: './generation-board.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GenerationBoardComponent implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);

  readonly projectId = input.required<string>();

  protected readonly project = signal<ProjectResponse | null>(null);
  protected readonly configs = signal<InfrastructureConfigResponse[]>([]);
  protected readonly isLoading = signal(true);

  protected readonly groupedByAlias = computed<AliasGroup[]>(() => {
    const configs = this.configs();
    const project = this.project();
    const repos = project?.repositories ?? [];
    const repoByAlias = new Map(repos.map((r) => [r.alias, r]));
    const isMultiRepo = project?.layoutPreset === 'MultiRepo';

    const byAlias = new Map<string, InfrastructureConfigResponse[]>();
    for (const config of configs) {
      // MultiRepo: bucket by the first config-level repo alias (or fallback to config name).
      // Mono-repo layouts: all configs share project-level repos, bucket by first project repo alias.
      const alias = isMultiRepo
        ? (config.repositories?.[0]?.alias ?? config.name ?? DEFAULT_ALIAS)
        : (repos[0]?.alias ?? DEFAULT_ALIAS);
      const bucket = byAlias.get(alias);
      if (bucket) {
        bucket.push(config);
      } else {
        byAlias.set(alias, [config]);
      }
    }

    return Array.from(byAlias.entries())
      .map<AliasGroup>(([alias, items]) => ({
        alias,
        repo: repoByAlias.get(alias)
          ?? (isMultiRepo
            ? (items[0]?.repositories?.find((r) => r.alias === alias) ?? null)
            : null),
        configs: items,
      }))
      .sort((a, b) => a.alias.localeCompare(b.alias));
  });

  protected readonly topology = computed<BoardTopology>(() => {
    const project = this.project();
    if (project?.layoutPreset === 'SplitInfraCode') return 'split-infra-code';
    const groups = this.groupedByAlias();
    if (groups.length === 0) return 'empty';
    if (groups.length === 1) return 'single';
    return groups.every((g) => g.configs.length === 1) ? 'split' : 'mixed';
  });

  protected readonly canGenerateAll = computed(() => {
    const t = this.topology();
    return t === 'single' || t === 'empty' || t === 'split-infra-code';
  });

  async ngOnInit(): Promise<void> {
    await this.load();
  }

  private async load(): Promise<void> {
    this.isLoading.set(true);
    try {
      const [project, configs] = await Promise.all([
        this.projectService.getProject(this.projectId()),
        this.projectService.getProjectConfigs(this.projectId()),
      ]);
      this.project.set(project);
      this.configs.set(configs);
    } catch {
      this.showError('PROJECT_DETAIL.BOARD.LOAD_ERROR');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected onGenerateAll(): void {
    const project = this.project();
    const hasRepositories = (project?.repositories?.length ?? 0) > 0;
    if (!project || !hasRepositories) {
      this.showError('PROJECT_DETAIL.BOARD.NO_GIT_CONFIG');
      return;
    }

    if (project.layoutPreset === 'SplitInfraCode') {
      const repos = project.repositories ?? [];
      const infraRepo = repos.find((r) => r.contentKinds?.includes('Infrastructure'));
      const codeRepo = repos.find((r) => r.contentKinds?.includes('ApplicationCode'));
      if (!infraRepo || !codeRepo) {
        this.showError('PROJECT_DETAIL.MULTI_REPO_PUSH.MISSING_SLOTS');
        return;
      }
      const data: MultiRepoPushDialogData = {
        projectId: project.id,
        infraAlias: infraRepo.alias,
        codeAlias: codeRepo.alias,
      };
      this.dialog.open(MultiRepoPushDialogComponent, {
        width: '68rem',
        maxWidth: '96vw',
        panelClass: 'ifs-multi-repo-push-dialog',
        data,
      });
      return;
    }

    const data: PushToGitDialogData = {
      configId: '',
      projectId: project.id,
      isProjectLevel: true,
      isCombinedProjectPush: true,
    };
    this.dialog.open(PushToGitDialogComponent, { width: '480px', data });
  }

  private showError(key: string): void {
    const message = this.translate.instant(key);
    this.snackBar.open(message, this.translate.instant('COMMON.CLOSE'), {
      duration: 5000,
      panelClass: 'error-snackbar',
    });
  }
}
