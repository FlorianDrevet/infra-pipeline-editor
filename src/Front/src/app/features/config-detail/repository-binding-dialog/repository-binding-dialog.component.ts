import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { TranslateModule } from '@ngx-translate/core';
import { RepositoryBindingResponse } from '../../../shared/interfaces/repository-binding.interface';
import { ProjectRepositoryResponse } from '../../../shared/interfaces/project-repository.interface';
import { ProjectService } from '../../../shared/services/project.service';

export interface RepositoryBindingDialogData {
  projectId: string;
  configId: string;
  currentBinding?: RepositoryBindingResponse | null;
}

export interface RepositoryBindingDialogResult {
  updated: true;
}

@Component({
  selector: 'app-repository-binding-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    ReactiveFormsModule,
    TranslateModule,
  ],
  templateUrl: './repository-binding-dialog.component.html',
  styleUrl: './repository-binding-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RepositoryBindingDialogComponent implements OnInit {
  private readonly dialogRef =
    inject(MatDialogRef<RepositoryBindingDialogComponent, RepositoryBindingDialogResult>);
  private readonly data: RepositoryBindingDialogData = inject(MAT_DIALOG_DATA);
  private readonly projectService = inject(ProjectService);
  private readonly fb = inject(FormBuilder);

  protected readonly repositories = signal<ProjectRepositoryResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');

  protected readonly form = this.fb.group({
    alias: new FormControl<string | null>(this.data.currentBinding?.alias ?? null),
    branch: new FormControl<string>(this.data.currentBinding?.branch ?? '', { nonNullable: true }),
    infraPath: new FormControl<string>(this.data.currentBinding?.infraPath ?? '', {
      nonNullable: true,
    }),
    pipelinePath: new FormControl<string>(this.data.currentBinding?.pipelinePath ?? '', {
      nonNullable: true,
    }),
  });

  async ngOnInit(): Promise<void> {
    this.isLoading.set(true);
    try {
      const project = await this.projectService.getProject(this.data.projectId);
      this.repositories.set(project.repositories ?? []);
    } catch {
      this.errorKey.set('CONFIG_DETAIL.BINDING.LOAD_ERROR');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected async onSubmit(): Promise<void> {
    if (this.form.invalid) return;

    this.isSubmitting.set(true);
    this.errorKey.set('');

    const value = this.form.getRawValue();
    const alias = value.alias ?? null;

    try {
      await this.projectService.setConfigRepositoryBinding(
        this.data.projectId,
        this.data.configId,
        {
          repositoryAlias: alias,
          branch: alias ? emptyToNull(value.branch) : null,
          infraPath: alias ? emptyToNull(value.infraPath) : null,
          pipelinePath: alias ? emptyToNull(value.pipelinePath) : null,
        }
      );
      this.dialogRef.close({ updated: true });
    } catch {
      this.errorKey.set('CONFIG_DETAIL.BINDING.SAVE_ERROR');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected onCancel(): void {
    this.dialogRef.close();
  }
}

function emptyToNull(value: string | null | undefined): string | null {
  if (value === null || value === undefined) return null;
  const trimmed = value.trim();
  return trimmed.length > 0 ? trimmed : null;
}
