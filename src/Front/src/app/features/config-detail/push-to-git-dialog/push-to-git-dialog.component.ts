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

interface PushErrorInfo {
  detail: string;
  isAmbiguous: boolean;
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

  private filterBranches(search: string): void { // NOSONAR S3776 - tracked under test-debt #22
    const lower = search.toLowerCase();
    const filtered = this.allBranches().filter(b => b.toLowerCase().includes(lower));
    this.filteredBranches.set(filtered);
  }

  protected async onPush(): Promise<void> {
    if (this.form.invalid) return;

    this.beginPush();

    try {
      const request = this.buildPushRequest();
      const response = await this.pushArtifactsToGit(request);
      this.handlePushSuccess(request.branchName, response);
    } catch (err: unknown) {
      this.handlePushFailure(err);
    }
  }

  private beginPush(): void {
    this.state.set('pushing');
    this.errorKey.set('');
    this.errorDetail.set('');
  }

  private buildPushRequest(): { branchName: string; commitMessage: string } {
    const value = this.form.getRawValue();

    return {
      branchName: value.branchName!,
      commitMessage: value.commitMessage ?? '',
    };
  }

  private handlePushSuccess(branchName: string, response: PushToGitResultSummary): void {
    localStorage.setItem(this.lastBranchKey, branchName);
    this.result.set(response);
    this.state.set('success');
  }

  private handlePushFailure(err: unknown): void {
    const pushError = this.extractPushError(err);

    if (pushError.isAmbiguous) {
      this.errorDetail.set('');
      this.errorKey.set('PROJECT_DETAIL.BOARD.PUSH_AMBIGUOUS_DESC');
      this.state.set('error');
      return;
    }

    this.errorDetail.set(pushError.detail);
    this.errorKey.set('CONFIG_DETAIL.PUSH_TO_GIT.ERROR');
    this.state.set('error');
  }

  private extractPushError(err: unknown): PushErrorInfo {
    if (!axios.isAxiosError(err) || !err.response?.data) {
      return { detail: '', isAmbiguous: false };
    }

    const data = this.asErrorRecord(err.response.data);
    if (!data) {
      return { detail: '', isAmbiguous: false };
    }

    return {
      detail: this.extractErrorDetail(data),
      isAmbiguous: this.isAmbiguousError(data),
    };
  }

  private extractErrorDetail(data: Record<string, unknown>): string {
    const firstError = this.getFirstErrorEntry(data['errors']);
    const description = this.readStringField(firstError, 'description');
    if (description) {
      return description;
    }

    return this.readStringField(data, 'detail');
  }

  private isAmbiguousError(data: Record<string, unknown>): boolean {
    if (this.readStringField(data, 'code') === AMBIGUOUS_PROJECT_LEVEL_GENERATION_CODE) {
      return true;
    }

    const firstError = this.getFirstErrorEntry(data['errors']);
    if (this.readStringField(firstError, 'code') === AMBIGUOUS_PROJECT_LEVEL_GENERATION_CODE) {
      return true;
    }

    const validationErrors = this.asErrorRecord(data['errors']);
    if (!validationErrors) {
      return false;
    }

    return Object.keys(validationErrors).includes(AMBIGUOUS_PROJECT_LEVEL_GENERATION_CODE);
  }

  private getFirstErrorEntry(errorsField: unknown): Record<string, unknown> | null {
    if (!Array.isArray(errorsField) || errorsField.length === 0) {
      return null;
    }

    return this.asErrorRecord(errorsField[0]);
  }

  private readStringField(data: Record<string, unknown> | null, key: string): string {
    if (!data) {
      return '';
    }

    const value = data[key];
    return typeof value === 'string' ? value : '';
  }

  private asErrorRecord(value: unknown): Record<string, unknown> | null {
    if (typeof value !== 'object' || value === null || Array.isArray(value)) {
      return null;
    }

    return value as Record<string, unknown>;
  }

  private async pushArtifactsToGit(
    request: { branchName: string; commitMessage: string },
  ): Promise<PushToGitResultSummary> {
    if (this.isCombinedProjectPush) {
      return this.pushCombinedProjectArtifactsToGit(request);
    }

    if (this.isProjectLevel) {
      return this.pushProjectLevelArtifactsToGit(request);
    }

    return this.pushConfigLevelArtifactsToGit(request);
  }

  private async pushProjectLevelArtifactsToGit(
    request: { branchName: string; commitMessage: string },
  ): Promise<PushToGitResultSummary> {
    if (this.isBootstrap) {
      return this.projectService.pushProjectBootstrapPipelineToGit(this.data.projectId, request);
    }

    if (this.isPipeline) {
      return this.projectService.pushProjectPipelineToGit(this.data.projectId, request);
    }

    return this.projectService.pushProjectBicepToGit(this.data.projectId, request);
  }

  private async pushConfigLevelArtifactsToGit(
    request: { branchName: string; commitMessage: string },
  ): Promise<PushToGitResultSummary> {
    if (this.isPipeline) {
      return this.pipelineService.pushToGit(this.data.configId, request);
    }

    return this.bicepService.pushToGit(this.data.configId, request);
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
