import { Component, inject, signal, computed } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { AzureResourceResponse } from '../../../shared/interfaces/resource-group.interface';
import { RoleAssignmentService } from '../../../shared/services/role-assignment.service';
import { UserAssignedIdentityService } from '../../../shared/services/user-assigned-identity.service';
import {
  AzureRoleDefinitionResponse,
  RoleAssignmentResponse,
} from '../../../shared/interfaces/role-assignment.interface';
import { RESOURCE_TYPE_ICONS } from '../../config-detail/enums/resource-type.enum';

export interface AddRoleAssignmentDialogData {
  sourceResourceId: string;
  currentResourceName: string;
  siblingResources: AzureResourceResponse[];
  resourceGroupId: string;
  configLocation: string;
  // When opening from a UAI's Granted Rights tab
  isFromUserAssignedIdentity?: boolean;
  userAssignedIdentityId?: string;
  userAssignedIdentityName?: string;
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
    MatInputModule,
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
  private readonly userAssignedIdentityService = inject(UserAssignedIdentityService);

  protected readonly resourceTypeIcons = RESOURCE_TYPE_ICONS;

  // ─── UAI mode detection ───
  protected readonly isFromUAI = computed(() => !!this.data.isFromUserAssignedIdentity);

  // ─── Step management ───
  protected readonly step = signal<1 | 2 | 3>(1);

  // ─── Step 1 — Target selection (normal & UAI) ───
  protected readonly targets = computed(() => this.data.siblingResources);
  protected readonly uaiTargetCandidates = computed(() =>
    this.data.siblingResources.filter(r => r.resourceType !== 'UserAssignedIdentity')
  );
  protected readonly selectedTarget = signal<AzureResourceResponse | null>(null);

  // ─── Step 2 — Configuration ───
  protected readonly availableRoles = signal<AzureRoleDefinitionResponse[]>([]);
  protected readonly rolesLoading = signal(false);
  protected readonly selectedIdentityType = signal<string>('SystemAssigned');
  protected readonly selectedRoleId = signal<string>('');
  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');

  // ─── User-Assigned Identity picker ───
  private readonly extraIdentities = signal<AzureResourceResponse[]>([]);
  protected readonly availableIdentities = computed(() => {
    const fromSiblings = this.data.siblingResources.filter(r => r.resourceType === 'UserAssignedIdentity');
    return [...fromSiblings, ...this.extraIdentities()];
  });
  protected readonly selectedIdentityId = signal<string>('');
  protected readonly showCreateIdentity = signal(false);
  protected readonly newIdentityName = signal('');
  protected readonly isCreatingIdentity = signal(false);

  protected readonly canSubmit = computed(() => {
    const notSubmitting = !this.isSubmitting();
    const hasRole = !!this.selectedRoleId();
    const hasTarget = !!this.selectedTarget();

    if (this.isFromUAI()) {
      return hasTarget && hasRole && notSubmitting;
    }

    const hasIdentityType = !!this.selectedIdentityType();
    const identityOk = this.selectedIdentityType() !== 'UserAssigned' || !!this.selectedIdentityId();
    return hasTarget && hasRole && hasIdentityType && notSubmitting && identityOk;
  });

  protected selectTarget(resource: AzureResourceResponse): void {
    this.selectedTarget.set(resource);
    if (this.isFromUAI()) {
      this.goToUAIStep2();
    } else {
      this.goToStep2();
    }
  }

  // ─── UAI mode: step 2 — load roles for selected target ───
  private async goToUAIStep2(): Promise<void> {
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

  protected onIdentityTypeChange(value: string): void {
    this.selectedIdentityType.set(value);
    if (value === 'SystemAssigned') {
      this.selectedIdentityId.set('');
      this.showCreateIdentity.set(false);
      this.newIdentityName.set('');
    }
  }

  protected selectIdentity(id: string): void {
    this.selectedIdentityId.set(id);
  }

  protected toggleCreateIdentity(show: boolean): void {
    this.showCreateIdentity.set(show);
    if (!show) {
      this.newIdentityName.set('');
    }
  }

  protected async createIdentity(): Promise<void> {
    const name = this.newIdentityName().trim();
    if (!name) return;

    this.isCreatingIdentity.set(true);
    this.errorKey.set('');
    try {
      const created = await this.userAssignedIdentityService.create({
        resourceGroupId: this.data.resourceGroupId,
        name,
        location: this.data.configLocation,
      });
      this.extraIdentities.update(list => [...list, {
        id: created.id,
        resourceType: 'UserAssignedIdentity',
        name: created.name,
        location: created.location,
      }]);
      this.selectedIdentityId.set(created.id);
      this.showCreateIdentity.set(false);
      this.newIdentityName.set('');
    } catch {
      this.errorKey.set('RESOURCE_EDIT.ADD_ROLE_DIALOG.CREATE_IDENTITY_ERROR');
    } finally {
      this.isCreatingIdentity.set(false);
    }
  }

  protected async onSubmit(): Promise<void> {
    const target = this.selectedTarget();
    if (!target || !this.selectedRoleId() || this.isSubmitting()) return;

    this.isSubmitting.set(true);
    this.errorKey.set('');

    try {
      if (this.isFromUAI()) {
        // UAI mode: the UAI itself is the source resource
        const result = await this.roleAssignmentService.add(this.data.userAssignedIdentityId!, {
          targetResourceId: target.id,
          managedIdentityType: 'UserAssigned',
          roleDefinitionId: this.selectedRoleId(),
          userAssignedIdentityId: this.data.userAssignedIdentityId,
        });
        this.dialogRef.close(result);
      } else {
        const result = await this.roleAssignmentService.add(this.data.sourceResourceId, {
          targetResourceId: target.id,
          managedIdentityType: this.selectedIdentityType(),
          roleDefinitionId: this.selectedRoleId(),
          userAssignedIdentityId: this.selectedIdentityType() === 'UserAssigned' ? this.selectedIdentityId() || undefined : undefined,
        });
        this.dialogRef.close(result);
      }
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
