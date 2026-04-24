import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { DsButtonComponent } from '../../../shared/components/ds';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';
import { UserResponse } from '../../../shared/interfaces/infra-config.interface';
import { ProjectResponse } from '../../../shared/interfaces/project.interface';
import { ProjectService } from '../../../shared/services/project.service';

export interface AddProjectMemberDialogData {
  projectId: string;
  existingUserIds: string[];
  availableUsers: UserResponse[];
}

const ROLES = ['Owner', 'Contributor', 'Reader'] as const;

@Component({
  selector: 'app-add-project-member-dialog',
  standalone: true,
  imports: [
    MatAutocompleteModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    ReactiveFormsModule,
    TranslateModule,
      DsButtonComponent,
  ],
  templateUrl: './add-project-member-dialog.component.html',
  styleUrl: './add-project-member-dialog.component.scss',
})
export class AddProjectMemberDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AddProjectMemberDialogComponent>);
  private readonly data: AddProjectMemberDialogData = inject(MAT_DIALOG_DATA);
  private readonly projectService = inject(ProjectService);
  private readonly fb = inject(FormBuilder);

  private readonly eligibleUsers = this.data.availableUsers.filter(
    (u) => !this.data.existingUserIds.includes(u.id)
  );

  protected readonly searchTerm = signal('');
  protected readonly selectedUser = signal<UserResponse | null>(null);

  protected readonly filteredUsers = computed(() => {
    const term = this.searchTerm().toLowerCase().trim();
    if (!term) return this.eligibleUsers;
    return this.eligibleUsers.filter(
      (u) =>
        u.firstName.toLowerCase().includes(term) ||
        u.lastName.toLowerCase().includes(term) ||
        `${u.firstName} ${u.lastName}`.toLowerCase().includes(term)
    );
  });

  protected readonly roles = ROLES;
  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');

  protected readonly form = this.fb.group({
    role: ['', Validators.required],
  });

  protected onSearchInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchTerm.set(value);
    if (this.selectedUser() && this.displayUser(this.selectedUser()!) !== value) {
      this.selectedUser.set(null);
    }
  }

  protected onUserSelected(user: UserResponse): void {
    this.selectedUser.set(user);
  }

  protected displayUser(user: UserResponse): string {
    return `${user.firstName} ${user.lastName}`;
  }

  protected displayUserFn = (user: UserResponse): string => {
    return user ? this.displayUser(user) : '';
  };

  protected async onSubmit(): Promise<void> {
    const user = this.selectedUser();
    if (this.form.invalid || !user) return;

    this.isSubmitting.set(true);
    this.errorKey.set('');

    try {
      const { role } = this.form.getRawValue();
      const updatedProject = await this.projectService.addMember(this.data.projectId, {
        userId: user.id,
        role: role!,
      });
      this.dialogRef.close(updatedProject);
    } catch {
      this.errorKey.set('PROJECT_DETAIL.MEMBERS.ADD_DIALOG_ERROR');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected onCancel(): void {
    this.dialogRef.close(null);
  }
}
