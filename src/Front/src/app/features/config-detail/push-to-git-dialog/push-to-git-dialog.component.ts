import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';
import { GitConfigResponse } from '../../../shared/interfaces/project.interface';
import { PushBicepToGitResponse } from '../../../shared/interfaces/bicep-generator.interface';
import { BicepGeneratorService } from '../../../shared/services/bicep-generator.service';
import { ProjectService } from '../../../shared/services/project.service';

export interface PushToGitDialogData {
  configId: string;
  projectId: string;
  gitConfig: GitConfigResponse;
  isProjectLevel?: boolean;
}

type DialogState = 'form' | 'pushing' | 'success' | 'error';

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
  ],
  templateUrl: './push-to-git-dialog.component.html',
  styleUrl: './push-to-git-dialog.component.scss',
})
export class PushToGitDialogComponent implements OnInit {
  private readonly dialogRef = inject(MatDialogRef<PushToGitDialogComponent>);
  private readonly data: PushToGitDialogData = inject(MAT_DIALOG_DATA);
  private readonly bicepService = inject(BicepGeneratorService);
  private readonly projectService = inject(ProjectService);
  private readonly fb = inject(FormBuilder);

  protected readonly isProjectLevel = this.data.isProjectLevel ?? false;

  protected readonly state = signal<DialogState>('form');
  protected readonly result = signal<PushBicepToGitResponse | null>(null);
  protected readonly errorKey = signal('');
  protected readonly allBranches = signal<string[]>([]);
  protected readonly filteredBranches = signal<string[]>([]);
  protected readonly branchesLoading = signal(true);

  protected readonly branchControl = new FormControl(this.data.gitConfig.defaultBranch, Validators.required);

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
        commitMessage: value.commitMessage || null,
      };
      const response = this.isProjectLevel
        ? await this.projectService.pushProjectBicepToGit(this.data.projectId, request)
        : await this.bicepService.pushToGit(this.data.configId, request);
      this.result.set(response);
      this.state.set('success');
    } catch {
      this.errorKey.set('CONFIG_DETAIL.PUSH_TO_GIT.ERROR');
      this.state.set('error');
    }
  }

  protected onRetry(): void {
    this.state.set('form');
    this.errorKey.set('');
  }

  protected onClose(): void {
    this.dialogRef.close();
  }
}
