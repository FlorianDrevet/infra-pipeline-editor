import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
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
  ProjectLayoutPreset,
  ProjectRepositoryResponse,
  RepositoryContentKind,
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
import { DsOptionCardComponent } from '../../../shared/components/ds';

interface PresetOption {
  value: ProjectLayoutPreset;
  labelKey: string;
  descriptionKey: string;
  icon: string;
}

interface RepoSlot {
  readonly kind: RepositoryContentKind;
  readonly labelKey: string;
  readonly repo: ProjectRepositoryResponse | null;
}

const LAYOUT_PRESETS: ReadonlyArray<PresetOption> = [
  {
    value: 'AllInOne',
    labelKey: 'PROJECT_DETAIL.LAYOUT.PRESET_ALL_IN_ONE',
    descriptionKey: 'PROJECT_DETAIL.LAYOUT.PRESET_ALL_IN_ONE_DESC',
    icon: 'inventory_2',
  },
  {
    value: 'SplitInfraCode',
    labelKey: 'PROJECT_DETAIL.LAYOUT.PRESET_SPLIT_INFRA_CODE',
    descriptionKey: 'PROJECT_DETAIL.LAYOUT.PRESET_SPLIT_INFRA_CODE_DESC',
    icon: 'call_split',
  },
  {
    value: 'MultiRepo',
    labelKey: 'PROJECT_DETAIL.LAYOUT.PRESET_MULTI_REPO',
    descriptionKey: 'PROJECT_DETAIL.LAYOUT.PRESET_MULTI_REPO_DESC',
    icon: 'hub',
  },
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
    DsOptionCardComponent,
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
  protected readonly repoActionId = signal<string | null>(null);

  protected readonly layoutPresets = LAYOUT_PRESETS;

  protected readonly currentPreset = computed<ProjectLayoutPreset>(() => {
    const preset = this.project()?.layoutPreset;
    if (preset === 'SplitInfraCode' || preset === 'MultiRepo') return preset;
    return 'AllInOne';
  });

  protected readonly repositories = computed<ProjectRepositoryResponse[]>(
    () => this.project()?.repositories ?? [],
  );

  /** Repo currently filling the AllInOne slot (must hold both kinds). */
  protected readonly allInOneRepo = computed<ProjectRepositoryResponse | null>(() => {
    return this.repositories()[0] ?? null;
  });

  /** Slots for the SplitInfraCode preset. */
  protected readonly splitSlots = computed<RepoSlot[]>(() => {
    const repos = this.repositories();
    return [
      {
        kind: 'Infrastructure',
        labelKey: 'PROJECT_DETAIL.LAYOUT.SLOT_INFRASTRUCTURE',
        repo: repos.find((r) => r.contentKinds.includes('Infrastructure')) ?? null,
      },
      {
        kind: 'ApplicationCode',
        labelKey: 'PROJECT_DETAIL.LAYOUT.SLOT_APPLICATION_CODE',
        repo: repos.find((r) => r.contentKinds.includes('ApplicationCode')) ?? null,
      },
    ];
  });

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

  protected async onPresetChange(preset: ProjectLayoutPreset): Promise<void> {
    if (preset === this.currentPreset()) return;
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

  protected openAllInOneDialog(): void {
    const existing = this.allInOneRepo();
    const data: RepositoryDialogData = {
      projectId: this.projectId(),
      mode: existing ? 'edit' : 'create',
      existing: existing ?? undefined,
      lockedKinds: ['Infrastructure', 'ApplicationCode'],
    };
    const ref = this.dialog.open(RepositoryDialogComponent, { data, width: '560px' });
    ref.afterClosed().subscribe(async (result) => {
      if (result) await this.load();
    });
  }

  protected openSlotDialog(slot: RepoSlot): void {
    const data: RepositoryDialogData = {
      projectId: this.projectId(),
      mode: slot.repo ? 'edit' : 'create',
      existing: slot.repo ?? undefined,
      lockedKinds: [slot.kind],
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
