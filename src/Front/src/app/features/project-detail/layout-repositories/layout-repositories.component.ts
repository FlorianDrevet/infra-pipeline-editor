import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
  input,
  signal,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AxiosError } from 'axios';
import { ProjectResponse } from '../../../shared/interfaces/project.interface';
import {
  ProjectCommonsStrategy,
  ProjectLayoutPreset,
  ProjectRepositoryResponse,
} from '../../../shared/interfaces/project-repository.interface';
import { ProjectService } from '../../../shared/services/project.service';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import {
  RepositoryDialogComponent,
  RepositoryDialogData,
} from './repository-dialog/repository-dialog.component';

interface PresetOption {
  value: ProjectLayoutPreset;
  labelKey: string;
  descriptionKey: string;
}

interface CommonsStrategyOption {
  value: ProjectCommonsStrategy;
  labelKey: string;
  disabled: boolean;
}

const LAYOUT_PRESETS: ReadonlyArray<PresetOption> = [
  {
    value: 'AllInOne',
    labelKey: 'PROJECT_DETAIL.LAYOUT.PRESET_ALL_IN_ONE',
    descriptionKey: 'PROJECT_DETAIL.LAYOUT.PRESET_ALL_IN_ONE_DESC',
  },
  {
    value: 'SplitInfraCode',
    labelKey: 'PROJECT_DETAIL.LAYOUT.PRESET_SPLIT_INFRA_CODE',
    descriptionKey: 'PROJECT_DETAIL.LAYOUT.PRESET_SPLIT_INFRA_CODE_DESC',
  },
  {
    value: 'MultiRepo',
    labelKey: 'PROJECT_DETAIL.LAYOUT.PRESET_MULTI_REPO',
    descriptionKey: 'PROJECT_DETAIL.LAYOUT.PRESET_MULTI_REPO_DESC',
  },
  {
    value: 'Custom',
    labelKey: 'PROJECT_DETAIL.LAYOUT.PRESET_CUSTOM',
    descriptionKey: 'PROJECT_DETAIL.LAYOUT.PRESET_CUSTOM_DESC',
  },
];

const COMMONS_STRATEGIES: ReadonlyArray<CommonsStrategyOption> = [
  { value: 'DuplicatePerRepo', labelKey: 'PROJECT_DETAIL.LAYOUT.COMMONS_DUPLICATE', disabled: false },
  { value: 'DedicatedCommonsRepo', labelKey: 'PROJECT_DETAIL.LAYOUT.COMMONS_DEDICATED', disabled: true },
  { value: 'AzdoRepoResource', labelKey: 'PROJECT_DETAIL.LAYOUT.COMMONS_AZDO', disabled: true },
];

@Component({
  selector: 'app-layout-repositories',
  standalone: true,
  imports: [
    TranslateModule,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatTooltipModule,
  ],
  templateUrl: './layout-repositories.component.html',
  styleUrl: './layout-repositories.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LayoutRepositoriesComponent implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);

  readonly projectId = input.required<string>();

  protected readonly project = signal<ProjectResponse | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly presetSaving = signal(false);
  protected readonly strategySaving = signal(false);
  protected readonly repoActionId = signal<string | null>(null);

  protected readonly layoutPresets = LAYOUT_PRESETS;
  protected readonly commonsStrategies = COMMONS_STRATEGIES;

  async ngOnInit(): Promise<void> {
    await this.load();
  }

  private async load(): Promise<void> {
    this.isLoading.set(true);
    try {
      const project = await this.projectService.getProject(this.projectId());
      this.project.set(project);
    } catch {
      this.showError('PROJECT_DETAIL.LAYOUT.LOAD_ERROR');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected get repositories(): ProjectRepositoryResponse[] {
    return this.project()?.repositories ?? [];
  }

  protected async onPresetChange(preset: ProjectLayoutPreset): Promise<void> {
    this.presetSaving.set(true);
    try {
      await this.projectService.setLayoutPreset(this.projectId(), preset);
      await this.load();
    } catch (error) {
      this.showError(this.mapError(error, 'PROJECT_DETAIL.LAYOUT.PRESET_ERROR'));
    } finally {
      this.presetSaving.set(false);
    }
  }

  protected async onStrategyChange(strategy: ProjectCommonsStrategy): Promise<void> {
    this.strategySaving.set(true);
    try {
      await this.projectService.setCommonsStrategy(this.projectId(), strategy);
      await this.load();
    } catch (error) {
      this.showError(this.mapError(error, 'PROJECT_DETAIL.LAYOUT.STRATEGY_ERROR'));
    } finally {
      this.strategySaving.set(false);
    }
  }

  protected openAddRepoDialog(): void {
    const data: RepositoryDialogData = { projectId: this.projectId(), mode: 'create' };
    const ref = this.dialog.open(RepositoryDialogComponent, { data, width: '560px' });
    ref.afterClosed().subscribe(async (result) => {
      if (result) await this.load();
    });
  }

  protected openEditRepoDialog(repo: ProjectRepositoryResponse): void {
    const data: RepositoryDialogData = {
      projectId: this.projectId(),
      mode: 'edit',
      existing: repo,
    };
    const ref = this.dialog.open(RepositoryDialogComponent, { data, width: '560px' });
    ref.afterClosed().subscribe(async (result) => {
      if (result) await this.load();
    });
  }

  protected openRemoveRepoDialog(repo: ProjectRepositoryResponse): void {
    const data: ConfirmDialogData = {
      titleKey: 'PROJECT_DETAIL.LAYOUT.DELETE_CONFIRM_TITLE',
      messageKey: 'PROJECT_DETAIL.LAYOUT.DELETE_CONFIRM_MESSAGE',
      messageParams: { alias: repo.alias },
      confirmKey: 'PROJECT_DETAIL.LAYOUT.DELETE_CONFIRM_YES',
      cancelKey: 'PROJECT_DETAIL.LAYOUT.DELETE_CONFIRM_CANCEL',
    };
    const ref = this.dialog.open(ConfirmDialogComponent, { data });
    ref.afterClosed().subscribe(async (confirmed) => {
      if (!confirmed) return;
      this.repoActionId.set(repo.id);
      try {
        await this.projectService.removeRepository(this.projectId(), repo.id);
        await this.load();
      } catch (error) {
        if (this.isConflict(error)) {
          this.showError('PROJECT_DETAIL.LAYOUT.REPO_DELETE_IN_USE');
        } else {
          this.showError('PROJECT_DETAIL.LAYOUT.DELETE_ERROR');
        }
      } finally {
        this.repoActionId.set(null);
      }
    });
  }

  private isConflict(error: unknown): boolean {
    return error instanceof AxiosError && error.response?.status === 409;
  }

  private mapError(error: unknown, fallbackKey: string): string {
    if (error instanceof AxiosError && error.response?.status === 400) {
      return 'PROJECT_DETAIL.LAYOUT.INVALID_VALUE';
    }
    return fallbackKey;
  }

  private showError(key: string): void {
    const message = this.translate.instant(key);
    this.snackBar.open(message, this.translate.instant('COMMON.CLOSE'), {
      duration: 5000,
      panelClass: 'error-snackbar',
    });
  }
}
