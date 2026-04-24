import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { DsButtonComponent, DsTextFieldComponent } from '../../../shared/components/ds';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';
import axios from 'axios';
import { BicepGeneratorService } from '../../../shared/services/bicep-generator.service';
import { PipelineGeneratorService } from '../../../shared/services/pipeline-generator.service';
import { ProjectService } from '../../../shared/services/project.service';

export interface PushToGitDialogData {
  configId: string;
  projectId: string;
  isProjectLevel?: boolean;
  isCombinedProjectPush?: boolean;
  isPipeline?: boolean;
  isBootstrap?: boolean;
}

type DialogState = 'form' | 'pushing' | 'success' | 'error';

const AMBIGUOUS_PROJECT_LEVEL_GENERATION_CODE = 'GitRouting.AmbiguousProjectLevelGeneration';

interface PushToGitResultSummary {
  branchName: string;
  branchUrl: string;
  commitSha: string;
  fileCount: number;
}

@Component({
  selector: 'app-push-to-git-dialog',
  standalone: true,
  imports: [
    MatAutocompleteModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    ReactiveFormsModule,
    TranslateModule,
    DsButtonComponent,
    DsTextFieldComponent,
  ],
  templateUrl: './push-to-git-dialog.component.html',
  styleUrl: './push-to-git-dialog.component.scss',
})
export class PushToGitDialogComponent implements OnInit {
  private readonly dialogRef = inject(MatDialogRef<PushToGitDialogComponent>);
  private readonly data: PushToGitDialogData = inject(MAT_DIALOG_DATA);
  private readonly bicepService = inject(BicepGeneratorService);
  private readonly pipelineService = inject(PipelineGeneratorService);
  private readonly projectService = inject(ProjectService);
  private readonly fb = inject(FormBuilder);

  protected readonly isProjectLevel = this.data.isProjectLevel ?? false;
  protected readonly isCombinedProjectPush = this.data.isCombinedProjectPush ?? false;
  protected readonly isPipeline = this.data.isPipeline ?? false;
  protected readonly isBootstrap = this.data.isBootstrap ?? false;
  protected readonly dialogTitleKey = this.isCombinedProjectPush
    ? 'CONFIG_DETAIL.PUSH_TO_GIT.DIALOG_TITLE_COMBINED'
    : 'CONFIG_DETAIL.PUSH_TO_GIT.DIALOG_TITLE';
  protected readonly pushingMessageKey = this.isCombinedProjectPush
    ? 'CONFIG_DETAIL.PUSH_TO_GIT.PUSHING_COMBINED'
    : 'CONFIG_DETAIL.PUSH_TO_GIT.PUSHING';
  protected readonly successTitleKey = this.isCombinedProjectPush
    ? 'CONFIG_DETAIL.PUSH_TO_GIT.SUCCESS_TITLE_COMBINED'
    : 'CONFIG_DETAIL.PUSH_TO_GIT.SUCCESS_TITLE';
  protected readonly commitLabelKey = this.isCombinedProjectPush
    ? 'CONFIG_DETAIL.PUSH_TO_GIT.LAST_COMMIT_SHA'
    : 'CONFIG_DETAIL.PUSH_TO_GIT.COMMIT_SHA';
  protected readonly combinedSuccessDetailKey = this.isCombinedProjectPush
    ? 'CONFIG_DETAIL.PUSH_TO_GIT.COMBINED_SUCCESS_DETAIL'
    : '';

  protected readonly state = signal<DialogState>('form');
  protected readonly result = signal<PushToGitResultSummary | null>(null);
  protected readonly errorKey = signal('');
  protected readonly errorDetail = signal('');
  protected readonly allBranches = signal<string[]>([]);
  protected readonly filteredBranches = signal<string[]>([]);
  protected readonly branchesLoading = signal(true);

  private readonly lastBranchKey = `ifs-push-branch-${this.data.projectId}`;

  protected readonly branchControl = new FormControl(
    localStorage.getItem(this.lastBranchKey) ?? 'main',
    Validators.required,
  );

  protected readonly form = this.fb.group({
    branchName: this.branchControl,
    commitMessage: [''],
  });

  ngOnInit(): void {
    this.loadBranches();
    this.branchControl.valueChanges.subscribe(value => {
      this.filterBranches(value ?? '');
    });
  }

  private async loadBranches(): Promise<void> {
    this.branchesLoading.set(true);
    try {
      const branches = await this.projectService.listBranches(this.data.projectId);
      const names = branches.map(b => b.name);
      this.allBranches.set(names);
      this.filterBranches(this.branchControl.value ?? '');
    } catch {
      this.allBranches.set([]);
      this.filteredBranches.set([]);
    } finally {
      this.branchesLoading.set(false);
    }
  }

  private filterBranches(search: string): void {
    const lower = search.toLowerCase();
    const filtered = this.allBranches().filter(b => b.toLowerCase().includes(lower));
    this.filteredBranches.set(filtered);
  }

  protected async onPush(): Promise<void> {
    if (this.form.invalid) return;

    this.state.set('pushing');
    this.errorKey.set('');

    try {
      const value = this.form.getRawValue();
      const request = {
        branchName: value.branchName!,
        commitMessage: value.commitMessage || '',
      };
      const response = this.isCombinedProjectPush
        ? await this.pushCombinedProjectArtifactsToGit(request)
        : this.isProjectLevel
        ? (this.isBootstrap
          ? await this.projectService.pushProjectBootstrapPipelineToGit(this.data.projectId, request)
          : this.isPipeline
          ? await this.projectService.pushProjectPipelineToGit(this.data.projectId, request)
          : await this.projectService.pushProjectBicepToGit(this.data.projectId, request))
        : (this.isPipeline
          ? await this.pipelineService.pushToGit(this.data.configId, request)
          : await this.bicepService.pushToGit(this.data.configId, request));
      localStorage.setItem(this.lastBranchKey, request.branchName);
      this.result.set(response);
      this.state.set('success');
    } catch (err: unknown) {
      let detail = '';
      let isAmbiguous = false;
      if (axios.isAxiosError(err) && err.response?.data) {
        const data = err.response.data as Record<string, unknown>;
        const errorsField = data['errors'];

        // ValidationProblem shape: { errors: { "GitRouting.AmbiguousProjectLevelGeneration": ["..."] } }
        if (errorsField && typeof errorsField === 'object' && !Array.isArray(errorsField)) {
          const keys = Object.keys(errorsField as Record<string, unknown>);
          if (keys.includes(AMBIGUOUS_PROJECT_LEVEL_GENERATION_CODE)) {
            isAmbiguous = true;
          }
        } else if (Array.isArray(errorsField)) {
          const first = errorsField[0] as Record<string, unknown> | undefined;
          detail = (first?.['description'] as string | undefined) ?? '';
          if ((first?.['code'] as string | undefined) === AMBIGUOUS_PROJECT_LEVEL_GENERATION_CODE) {
            isAmbiguous = true;
          }
        }

        if (!detail) {
          detail = (data['detail'] as string | undefined) ?? '';
        }
        // BadRequest ErrorOr shape may expose `code` at top-level too.
        if (!isAmbiguous && (data['code'] as string | undefined) === AMBIGUOUS_PROJECT_LEVEL_GENERATION_CODE) {
          isAmbiguous = true;
        }
      }

      if (isAmbiguous) {
        this.errorDetail.set('');
        this.errorKey.set('PROJECT_DETAIL.BOARD.PUSH_AMBIGUOUS_DESC');
        this.state.set('error');
      } else {
        this.errorDetail.set(detail);
        this.errorKey.set('CONFIG_DETAIL.PUSH_TO_GIT.ERROR');
        this.state.set('error');
      }
    }
  }

  private async pushCombinedProjectArtifactsToGit(
    request: { branchName: string; commitMessage: string },
  ): Promise<PushToGitResultSummary> {
    return this.projectService.pushProjectGeneratedArtifactsToGit(this.data.projectId, request);
  }

  protected onRetry(): void {
    this.state.set('form');
    this.errorKey.set('');
    this.errorDetail.set('');
  }

  protected onClose(): void {
    this.dialogRef.close();
  }
}
