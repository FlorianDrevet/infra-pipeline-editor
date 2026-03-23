import { Component, inject, signal, computed } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { AzureResourceResponse } from '../../../shared/interfaces/resource-group.interface';
import { RoleAssignmentService } from '../../../shared/services/role-assignment.service';
import {
  AzureRoleDefinitionResponse,
  RoleAssignmentResponse,
} from '../../../shared/interfaces/role-assignment.interface';
import { RESOURCE_TYPE_ICONS } from '../../config-detail/enums/resource-type.enum';

export interface AddRoleAssignmentDialogData {
  sourceResourceId: string;
  currentResourceName: string;
  siblingResources: AzureResourceResponse[];
}

@Component({
  selector: 'app-add-role-assignment-dialog',
  standalone: true,
  imports: [
    TranslateModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatSelectModule,
    MatFormFieldModule,
    MatTooltipModule,
  ],
  templateUrl: './add-role-assignment-dialog.component.html',
  styleUrl: './add-role-assignment-dialog.component.scss',
})
export class AddRoleAssignmentDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AddRoleAssignmentDialogComponent>);
  private readonly data: AddRoleAssignmentDialogData = inject(MAT_DIALOG_DATA);
  private readonly roleAssignmentService = inject(RoleAssignmentService);

  protected readonly resourceTypeIcons = RESOURCE_TYPE_ICONS;

  // ─── Step management ───
  protected readonly step = signal<1 | 2>(1);

  // ─── Step 1 — Target selection ───
  protected readonly targets = computed(() => this.data.siblingResources);
  protected readonly selectedTarget = signal<AzureResourceResponse | null>(null);

  // ─── Step 2 — Configuration ───
  protected readonly availableRoles = signal<AzureRoleDefinitionResponse[]>([]);
  protected readonly rolesLoading = signal(false);
  protected readonly selectedIdentityType = signal<string>('SystemAssigned');
  protected readonly selectedRoleId = signal<string>('');
  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');

  protected readonly canSubmit = computed(() =>
    !!this.selectedTarget() &&
    !!this.selectedRoleId() &&
    !!this.selectedIdentityType() &&
    !this.isSubmitting()
  );

  protected selectTarget(resource: AzureResourceResponse): void {
    this.selectedTarget.set(resource);
    this.goToStep2();
  }

  protected async goToStep2(): Promise<void> {
    this.step.set(2);
    this.errorKey.set('');
    this.selectedRoleId.set('');

    const target = this.selectedTarget();
    if (!target) return;

    this.rolesLoading.set(true);
    try {
      const roles = await this.roleAssignmentService.getAvailableRoleDefinitions(target.id);
      this.availableRoles.set(roles);
    } catch {
      this.availableRoles.set([]);
    } finally {
      this.rolesLoading.set(false);
    }
  }

  protected goBack(): void {
    this.step.set(1);
    this.errorKey.set('');
  }

  protected async onSubmit(): Promise<void> {
    const target = this.selectedTarget();
    if (!target || !this.selectedRoleId() || this.isSubmitting()) return;

    this.isSubmitting.set(true);
    this.errorKey.set('');

    try {
      const result = await this.roleAssignmentService.add(this.data.sourceResourceId, {
        targetResourceId: target.id,
        managedIdentityType: this.selectedIdentityType(),
        roleDefinitionId: this.selectedRoleId(),
      });
      this.dialogRef.close(result);
    } catch {
      this.errorKey.set('RESOURCE_EDIT.ADD_ROLE_DIALOG.ERROR');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected onCancel(): void {
    this.dialogRef.close(undefined);
  }
}
