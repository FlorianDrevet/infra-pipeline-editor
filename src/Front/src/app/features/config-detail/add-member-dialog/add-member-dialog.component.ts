import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';
import { UserResponse } from '../../../shared/interfaces/infra-config.interface';
import { InfraConfigService } from '../../../shared/services/infra-config.service';

export interface AddMemberDialogData {
  configId: string;
  existingUserIds: string[];
  availableUsers: UserResponse[];
}

const ROLES = ['Owner', 'Contributor', 'Reader'] as const;

@Component({
  selector: 'app-add-member-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    ReactiveFormsModule,
    TranslateModule,
  ],
  templateUrl: './add-member-dialog.component.html',
  styleUrl: './add-member-dialog.component.scss',
})
export class AddMemberDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AddMemberDialogComponent>);
  private readonly data: AddMemberDialogData = inject(MAT_DIALOG_DATA);
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly fb = inject(FormBuilder);

  protected readonly filteredUsers = this.data.availableUsers.filter(
    (u) => !this.data.existingUserIds.includes(u.id)
  );

  protected readonly roles = ROLES;
  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');

  protected readonly form = this.fb.group({
    userId: ['', Validators.required],
    role: ['', Validators.required],
  });

  protected async onSubmit(): Promise<void> {
    if (this.form.invalid) return;

    this.isSubmitting.set(true);
    this.errorKey.set('');

    try {
      const { userId, role } = this.form.getRawValue();
      const updatedConfig = await this.infraConfigService.addMember(this.data.configId, {
        userId: userId!,
        role: role!,
      });
      this.dialogRef.close(updatedConfig);
    } catch {
      this.errorKey.set('CONFIG_DETAIL.MEMBERS.ADD_DIALOG_ERROR');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected onCancel(): void {
    this.dialogRef.close(null);
  }
}
