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
import { MatCheckboxModule } from '@angular/material/checkbox';

import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { DsButtonComponent } from '../../../shared/components/ds';
import { AzureResourceResponse } from '../../../shared/interfaces/resource-group.interface';
import { RoleAssignmentService } from '../../../shared/services/role-assignment.service';
import { ProjectService } from '../../../shared/services/project.service';
import { AppSettingService } from '../../../shared/services/app-setting.service';
import { OutputDefinitionResponse } from '../../../shared/interfaces/app-setting.interface';
import { ProjectPipelineVariableGroupResponse } from '../../../shared/interfaces/project.interface';
import { AppConfigurationKeyService } from '../services/app-configuration-key.service';
import { AppConfigurationKeyResponse, AddAppConfigurationKeyRequest } from '../models/app-configuration-key.interface';
import { RESOURCE_TYPE_ICONS } from '../../config-detail/enums/resource-type.enum';

export interface AddAppConfigKeyDialogData {
  appConfigurationId: string;
  siblingResources: AzureResourceResponse[];
  environments: { name: string }[];
  projectId: string;
}

@Component({
  selector: 'app-add-app-config-key-dialog',
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
    MatCheckboxModule,
    DsButtonComponent,
  ],
  templateUrl: './add-app-config-key-dialog.component.html',
  styleUrl: './add-app-config-key-dialog.component.scss',
})
export class AddAppConfigKeyDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AddAppConfigKeyDialogComponent>);
  protected readonly data: AddAppConfigKeyDialogData = inject(MAT_DIALOG_DATA);
  private readonly configKeyService = inject(AppConfigurationKeyService);
  private readonly appSettingService = inject(AppSettingService);
  private readonly roleAssignmentService = inject(RoleAssignmentService);
  private readonly projectService = inject(ProjectService);

  protected readonly resourceTypeIcons = RESOURCE_TYPE_ICONS;

  // ─── Mode: static value, secret, or output ───
  protected readonly mode = signal<'static' | 'secret' | 'output'>('static');

  // ─── Static sub-step ───
  protected readonly staticValueSource = signal<'environments' | 'variableGroup' | null>(null);

  // ─── Secret sub-step ───
  protected readonly secretValueSource = signal<'directInKeyVault' | 'variableGroup' | null>(null);

  protected readonly secretAssignmentMode = computed<'ViaBicepparam' | 'DirectInKeyVault' | null>(() => {
    const src = this.secretValueSource();
    if (src === 'variableGroup') return 'ViaBicepparam';
    if (src === 'directInKeyVault') return 'DirectInKeyVault';
    return null;
  });

  // ─── Variable Group ───
  protected readonly vgOptions = signal<ProjectPipelineVariableGroupResponse[]>([]);
  protected readonly vgLoading = signal(false);
  protected readonly selectedVariableGroupId = signal<string | null>(null);
  protected readonly newGroupName = signal('');
  protected readonly isCreatingNewGroup = signal(false);
  protected readonly pipelineVariableName = signal('');

  // ─── Key Vault selection ───
  protected readonly keyVaultResources = computed(() =>
    this.data.siblingResources.filter(r => r.resourceType === 'KeyVault')
  );
  protected readonly selectedKeyVault = signal<AzureResourceResponse | null>(null);

  // ─── Key Vault access check ───
  protected readonly kvAccessChecking = signal(false);
  protected readonly kvHasAccess = signal<boolean | null>(null);
  protected readonly kvMissingRoleName = signal<string | null>(null);
  protected readonly kvMissingRoleDefinitionId = signal<string | null>(null);
  protected readonly sensitiveRoleAssigning = signal(false);
  protected readonly sensitiveRoleAssigned = signal(false);
  protected readonly sensitiveRoleError = signal(false);

  // ─── Form fields ───
  protected readonly keyName = signal('');
  protected readonly label = signal('');
  protected readonly secretName = signal('');
  protected readonly environmentValues = signal<Record<string, string>>({});

  // ─── Output mode — Step-based wizard (1→2→3(sensitive)→4) ───
  protected readonly step = signal<1 | 2 | 3 | 4>(1);

  // Step 1 — Source selection
  protected readonly sourceResources = computed(() => this.data.siblingResources);
  protected readonly selectedSource = signal<AzureResourceResponse | null>(null);

  // Step 2 — Output selection
  protected readonly availableOutputs = signal<OutputDefinitionResponse[]>([]);
  protected readonly outputsLoading = signal(false);
  protected readonly selectedOutput = signal<OutputDefinitionResponse | null>(null);

  protected readonly nonSensitiveOutputs = computed(() => this.availableOutputs().filter(o => !o.isSensitive));
  protected readonly sensitiveOutputs = computed(() => this.availableOutputs().filter(o => o.isSensitive));
  protected readonly selectedOutputIsSensitive = computed(() => this.selectedOutput()?.isSensitive ?? false);

  // Step 3 — Sensitive security choice
  protected readonly sensitiveChoice = signal<'keyvault' | 'direct' | null>(null);
  protected readonly sensitiveDirectConfirmed = signal(false);
  protected readonly sensitiveSecretName = signal('');

  // ─── Submit state ───
  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');

  protected readonly suggestedKeyName = computed(() => {
    if (this.mode() === 'output' && this.sensitiveChoice() === 'keyvault') {
      const kv = this.selectedKeyVault();
      const secret = this.sensitiveSecretName();
      if (!kv || !secret) return '';
      const prefix = kv.name.toUpperCase().replace(/[^A-Z0-9]/g, '_');
      const suffix = secret.toUpperCase().replace(/[^A-Z0-9]/g, '_');
      return `${prefix}__${suffix}`;
    }
    const source = this.selectedSource();
    const output = this.selectedOutput();
    if (!source || !output) return '';
    const prefix = source.name.toUpperCase().replace(/[^A-Z0-9]/g, '_');
    const suffix = output.name.toUpperCase().replace(/[^A-Z0-9]/g, '_');
    return `${prefix}__${suffix}`;
  });

  protected readonly canSubmit = computed(() => {
    const key = this.keyName().trim();
    if (!key || this.isSubmitting()) return false;

    if (this.mode() === 'static') {
      const src = this.staticValueSource();
      if (src === 'variableGroup') {
        const hasVg = this.isCreatingNewGroup()
          ? this.newGroupName().trim().length > 0
          : !!this.selectedVariableGroupId();
        return hasVg && this.pipelineVariableName().trim().length > 0;
      }
      if (src === 'environments') {
        const vals = this.environmentValues();
        return Object.values(vals).some(v => v.trim().length > 0);
      }
      return false;
    }

    if (this.mode() === 'secret') {
      const hasKv = !!this.selectedKeyVault() && !!this.secretName().trim();
      if (!hasKv) return false;
      const src = this.secretValueSource();
      if (src === 'variableGroup') {
        const hasVg = this.isCreatingNewGroup()
          ? this.newGroupName().trim().length > 0
          : !!this.selectedVariableGroupId();
        return hasVg && this.pipelineVariableName().trim().length > 0;
      }
      return src === 'directInKeyVault';
    }

    if (this.mode() === 'output') {
      return !!this.selectedSource() && !!this.selectedOutput();
    }

    return false;
  });

  protected readonly canProceedFromSensitiveStep = computed(() => {
    const choice = this.sensitiveChoice();
    if (!choice) return false;
    if (choice === 'keyvault') return !!this.selectedKeyVault() && !!this.sensitiveSecretName().trim();
    if (choice === 'direct') return this.sensitiveDirectConfirmed();
    return false;
  });

  protected onModeChange(value: 'static' | 'secret' | 'output'): void {
    this.mode.set(value);
    this.staticValueSource.set(null);
    this.secretValueSource.set(null);
    this.selectedKeyVault.set(null);
    this.secretName.set('');
    this.keyName.set('');
    this.label.set('');
    this.environmentValues.set(
      Object.fromEntries(this.data.environments.map(e => [e.name, '']))
    );
    this.errorKey.set('');
    this.kvHasAccess.set(null);
    this.kvMissingRoleName.set(null);
    this.kvMissingRoleDefinitionId.set(null);
    this.selectedVariableGroupId.set(null);
    this.newGroupName.set('');
    this.isCreatingNewGroup.set(false);
    this.pipelineVariableName.set('');
    // Reset output-mode signals
    this.step.set(1);
    this.selectedSource.set(null);
    this.selectedOutput.set(null);
    this.availableOutputs.set([]);
    this.sensitiveChoice.set(null);
    this.sensitiveDirectConfirmed.set(false);
    this.sensitiveSecretName.set('');
    this.sensitiveRoleAssigning.set(false);
    this.sensitiveRoleAssigned.set(false);
    this.sensitiveRoleError.set(false);
  }

  protected async selectStaticValueSource(source: 'environments' | 'variableGroup'): Promise<void> {
    this.staticValueSource.set(source);
    if (source === 'variableGroup') {
      await this.loadVariableGroups();
    }
    if (source === 'environments') {
      this.environmentValues.set(
        Object.fromEntries(this.data.environments.map(e => [e.name, '']))
      );
    }
  }

  protected async selectSecretValueSource(source: 'directInKeyVault' | 'variableGroup'): Promise<void> {
    this.secretValueSource.set(source);
    if (source === 'variableGroup') {
      await this.loadVariableGroups();
    }
  }

  protected selectKeyVault(resource: AzureResourceResponse): void {
    this.selectedKeyVault.set(resource);
    this.checkKeyVaultAccess();
  }

  protected async checkKeyVaultAccess(): Promise<void> {
    const kv = this.selectedKeyVault();
    if (!kv) return;

    this.kvAccessChecking.set(true);
    this.kvHasAccess.set(null);
    this.kvMissingRoleName.set(null);
    this.kvMissingRoleDefinitionId.set(null);

    try {
      const result = await this.appSettingService.checkKeyVaultAccess(this.data.appConfigurationId, kv.id);
      this.kvHasAccess.set(result.hasAccess);
      this.kvMissingRoleName.set(result.missingRoleName ?? null);
      this.kvMissingRoleDefinitionId.set(result.missingRoleDefinitionId ?? null);
    } catch {
      this.kvHasAccess.set(null);
    } finally {
      this.kvAccessChecking.set(false);
    }
  }

  protected async assignRole(): Promise<void> {
    const kv = this.selectedKeyVault();
    const roleDefId = this.kvMissingRoleDefinitionId();
    if (!kv || !roleDefId) return;

    this.sensitiveRoleAssigning.set(true);
    this.sensitiveRoleError.set(false);

    try {
      await this.roleAssignmentService.add(this.data.appConfigurationId, {
        targetResourceId: kv.id,
        roleDefinitionId: roleDefId,
        managedIdentityType: 'SystemAssigned',
      });
      this.sensitiveRoleAssigned.set(true);
      this.kvHasAccess.set(true);
      this.kvMissingRoleName.set(null);
      this.kvMissingRoleDefinitionId.set(null);
    } catch {
      this.sensitiveRoleError.set(true);
    } finally {
      this.sensitiveRoleAssigning.set(false);
    }
  }

  // ─── Output mode — Step navigation ───

  protected selectSource(resource: AzureResourceResponse): void {
    this.selectedSource.set(resource);
    this.goToStep2();
  }

  protected async goToStep2(): Promise<void> {
    this.step.set(2);
    this.errorKey.set('');
    this.selectedOutput.set(null);

    const source = this.selectedSource();
    if (!source) return;

    this.outputsLoading.set(true);
    try {
      const response = await this.appSettingService.getAvailableOutputs(source.id);
      this.availableOutputs.set(response.outputs);
    } catch {
      this.availableOutputs.set([]);
    } finally {
      this.outputsLoading.set(false);
    }
  }

  protected selectOutput(output: OutputDefinitionResponse): void {
    this.selectedOutput.set(output);
    if (output.isSensitive) {
      this.sensitiveChoice.set(null);
      this.sensitiveDirectConfirmed.set(false);
      this.sensitiveSecretName.set(this.generateSecretName());
      this.selectedKeyVault.set(null);
      this.kvHasAccess.set(null);
      this.kvMissingRoleName.set(null);
      this.kvMissingRoleDefinitionId.set(null);
      this.sensitiveRoleAssigning.set(false);
      this.sensitiveRoleAssigned.set(false);
      this.sensitiveRoleError.set(false);
      this.step.set(3);
    } else {
      this.keyName.set(this.suggestedKeyName());
      this.step.set(4);
    }
  }

  protected selectSensitiveChoice(choice: 'keyvault' | 'direct'): void {
    this.sensitiveChoice.set(choice);
    this.sensitiveDirectConfirmed.set(false);
    if (choice === 'keyvault') {
      this.selectedKeyVault.set(null);
      this.kvHasAccess.set(null);
    }
  }

  protected selectSensitiveKeyVault(resource: AzureResourceResponse): void {
    this.selectedKeyVault.set(resource);
    this.checkKeyVaultAccess();
  }

  protected proceedFromSensitiveStep(): void {
    this.keyName.set(this.suggestedKeyName());
    this.step.set(4);
  }

  protected async assignSensitiveRole(): Promise<void> {
    const kv = this.selectedKeyVault();
    const roleDefId = this.kvMissingRoleDefinitionId();
    if (!kv || !roleDefId) return;

    this.sensitiveRoleAssigning.set(true);
    this.sensitiveRoleError.set(false);

    try {
      await this.roleAssignmentService.add(this.data.appConfigurationId, {
        targetResourceId: kv.id,
        roleDefinitionId: roleDefId,
        managedIdentityType: 'SystemAssigned',
      });
      this.sensitiveRoleAssigned.set(true);
      this.kvHasAccess.set(true);
      this.kvMissingRoleName.set(null);
      this.kvMissingRoleDefinitionId.set(null);
    } catch {
      this.sensitiveRoleError.set(true);
    } finally {
      this.sensitiveRoleAssigning.set(false);
    }
  }

  protected goBack(): void {
    const currentStep = this.step();
    if (currentStep === 4) {
      if (this.selectedOutputIsSensitive()) {
        this.step.set(3);
      } else {
        this.step.set(2);
      }
    } else if (currentStep === 3) {
      this.step.set(2);
    } else if (currentStep === 2) {
      this.step.set(1);
    }
    this.errorKey.set('');
  }

  private async loadVariableGroups(): Promise<void> {
    if (!this.data.projectId) return;
    this.vgLoading.set(true);
    try {
      const groups = await this.projectService.getPipelineVariableGroups(this.data.projectId);
      this.vgOptions.set(groups);
    } catch {
      this.vgOptions.set([]);
    } finally {
      this.vgLoading.set(false);
    }
  }

  protected onVgSelectionChange(value: string): void {
    if (value === '__create_new__') {
      this.isCreatingNewGroup.set(true);
      this.selectedVariableGroupId.set(null);
    } else {
      this.isCreatingNewGroup.set(false);
      this.selectedVariableGroupId.set(value);
      this.newGroupName.set('');
    }
  }

  protected updateEnvironmentValue(envName: string, value: string): void {
    this.environmentValues.update(current => ({ ...current, [envName]: value }));
  }

  protected async onSubmit(): Promise<void> {
    const key = this.keyName().trim();
    if (!key || this.isSubmitting()) return;

    this.isSubmitting.set(true);
    this.errorKey.set('');

    try {
      // If creating a new variable group, create it first
      let variableGroupId = this.selectedVariableGroupId();
      if (this.isViaVariableGroup() && this.isCreatingNewGroup()) {
        const newGroupName = this.newGroupName().trim();
        if (newGroupName && this.data.projectId) {
          const newGroup = await this.projectService.addPipelineVariableGroup(this.data.projectId, { groupName: newGroupName });
          variableGroupId = newGroup.id;
        }
      }

      const request: AddAppConfigurationKeyRequest = { key };

      // Add label if provided
      const labelValue = this.label().trim();
      if (labelValue) {
        request.label = labelValue;
      }

      if (this.mode() === 'output') {
        request.sourceResourceId = this.selectedSource()!.id;
        request.sourceOutputName = this.selectedOutput()!.name;
        if (this.sensitiveChoice() === 'keyvault') {
          request.keyVaultResourceId = this.selectedKeyVault()!.id;
          request.secretName = this.sensitiveSecretName().trim();
          request.exportToKeyVault = true;
        }
      } else if (this.mode() === 'secret') {
        request.keyVaultResourceId = this.selectedKeyVault()!.id;
        request.secretName = this.secretName().trim();
        request.secretValueAssignment = this.secretAssignmentMode() ?? undefined;

        if (this.secretValueSource() === 'variableGroup') {
          request.variableGroupId = variableGroupId ?? undefined;
          request.pipelineVariableName = this.pipelineVariableName().trim();
        }
      } else {
        // static mode
        if (this.staticValueSource() === 'variableGroup') {
          request.variableGroupId = variableGroupId ?? undefined;
          request.pipelineVariableName = this.pipelineVariableName().trim();
        } else {
          request.environmentValues = this.environmentValues();
        }
      }

      const result = await this.configKeyService.add(this.data.appConfigurationId, request);
      this.dialogRef.close(result);
    } catch (err: unknown) {
      const axios = await import('axios');
      if (axios.isAxiosError(err) && err.response?.status === 409) {
        this.errorKey.set('RESOURCE_EDIT.ADD_CONFIG_KEY_DIALOG.ERROR_DUPLICATE');
      } else {
        this.errorKey.set('RESOURCE_EDIT.ADD_CONFIG_KEY_DIALOG.ERROR');
      }
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected onCancel(): void {
    this.dialogRef.close(undefined);
  }

  private readonly isViaVariableGroup = computed(() => {
    if (this.mode() === 'static') return this.staticValueSource() === 'variableGroup';
    if (this.mode() === 'secret') return this.secretValueSource() === 'variableGroup';
    return false;
  });

  private generateSecretName(): string {
    const source = this.selectedSource();
    const output = this.selectedOutput();
    if (!source || !output) return '';
    const prefix = source.name.toLowerCase().replace(/[^a-z0-9]/g, '-');
    const suffix = output.name.toLowerCase().replace(/[^a-z0-9]/g, '-');
    return `${prefix}-${suffix}`;
  }
}
