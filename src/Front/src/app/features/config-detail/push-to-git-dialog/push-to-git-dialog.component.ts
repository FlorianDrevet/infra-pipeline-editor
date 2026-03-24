import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
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

export interface PushToGitDialogData {
  configId: string;
  gitConfig: GitConfigResponse;
}

type DialogState = 'form' | 'pushing' | 'success' | 'error';

@Component({
  selector: 'app-push-to-git-dialog',
  standalone: true,
  imports: [
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
export class PushToGitDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<PushToGitDialogComponent>);
  private readonly data: PushToGitDialogData = inject(MAT_DIALOG_DATA);
  private readonly bicepService = inject(BicepGeneratorService);
  private readonly fb = inject(FormBuilder);

  protected readonly state = signal<DialogState>('form');
  protected readonly result = signal<PushBicepToGitResponse | null>(null);
  protected readonly errorKey = signal('');

  protected readonly form = this.fb.group({
    branchName: [this.data.gitConfig.defaultBranch, Validators.required],
    commitMessage: [''],
  });

  protected async onPush(): Promise<void> {
    if (this.form.invalid) return;

    this.state.set('pushing');
    this.errorKey.set('');

    try {
      const value = this.form.getRawValue();
      const response = await this.bicepService.pushToGit(this.data.configId, {
        branchName: value.branchName!,
        commitMessage: value.commitMessage || null,
      });
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
