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
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { AzureResourceResponse } from '../../../shared/interfaces/resource-group.interface';
import { AppSettingService } from '../../../shared/services/app-setting.service';
import { RoleAssignmentService } from '../../../shared/services/role-assignment.service';
import { ProjectService } from '../../../shared/services/project.service';
import { AppSettingResponse, OutputDefinitionResponse } from '../../../shared/interfaces/app-setting.interface';
import { ProjectPipelineVariableGroupResponse } from '../../../shared/interfaces/project.interface';
import { RESOURCE_TYPE_ICONS } from '../../config-detail/enums/resource-type.enum';

export interface AddAppSettingDialogData {
  resourceId: string;
  currentResourceName: string;
  siblingResources: AzureResourceResponse[];
  environments: { name: string }[];
  projectId: string;
}

@Component({
  selector: 'app-add-app-setting-dialog',
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
    MatSlideToggleModule,
  ],
  templateUrl: './add-app-setting-dialog.component.html',
  styleUrl: './add-app-setting-dialog.component.scss',
})
export class AddAppSettingDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AddAppSettingDialogComponent>);
  protected readonly data: AddAppSettingDialogData = inject(MAT_DIALOG_DATA);
  private readonly appSettingService = inject(AppSettingService);
  private readonly roleAssignmentService = inject(RoleAssignmentService);
  private readonly projectService = inject(ProjectService);

  protected readonly resourceTypeIcons = RESOURCE_TYPE_ICONS;

  // ─── Mode: static value or resource output ───
  protected readonly mode = signal<'static' | 'output'>('output');

  // ─── Variable Group (static mode only) ───
  protected readonly isViaVariableGroup = signal(false);
  protected readonly vgOptions = signal<ProjectPipelineVariableGroupResponse[]>([]);
  protected readonly vgLoading = signal(false);
  protected readonly selectedVariableGroupId = signal<string | null>(null);
  protected readonly newGroupName = signal('');
  protected readonly isCreatingNewGroup = signal(false);
  protected readonly pipelineVariableName = signal('');

  // ─── Step management (output mode: 1→2→3(sensitive)→4 or 1→2→4) ───
  protected readonly step = signal<1 | 2 | 3 | 4>(1);

  // ─── Step 1 — Source selection (output mode) ───
  protected readonly sourceResources = computed(() => this.data.siblingResources);
  protected readonly selectedSource = signal<AzureResourceResponse | null>(null);

  // ─── Step 2 — Output selection ───
  protected readonly availableOutputs = signal<OutputDefinitionResponse[]>([]);
  protected readonly outputsLoading = signal(false);
  protected readonly selectedOutput = signal<OutputDefinitionResponse | null>(null);

  // ─── Step 2 — Sensitive / Non-sensitive split ───
  protected readonly nonSensitiveOutputs = computed(() => this.availableOutputs().filter(o => !o.isSensitive));
  protected readonly sensitiveOutputs = computed(() => this.availableOutputs().filter(o => o.isSensitive));
  protected readonly selectedOutputIsSensitive = computed(() => this.selectedOutput()?.isSensitive ?? false);

  // ─── Step 3 — Sensitive security choice ───
  protected readonly sensitiveChoice = signal<'keyvault' | 'direct' | null>(null);
  protected readonly sensitiveDirectConfirmed = signal(false);
  protected readonly sensitiveSecretName = signal('');
  protected readonly sensitiveRoleAssigning = signal(false);
  protected readonly sensitiveRoleAssigned = signal(false);
  protected readonly sensitiveRoleError = signal(false);
  protected readonly kvMissingRoleDefinitionId = signal<string | null>(null);

  // ─── Static mode — Is Secret toggle ───
  protected readonly isSecret = signal(false);
  protected readonly secretAssignmentMode = signal<'ViaBicepparam' | 'DirectInKeyVault' | null>(null);

  // ─── Key Vault selection (shared: static secret + output sensitive) ───
  protected readonly keyVaultResources = computed(() =>
    this.data.siblingResources.filter(r => r.resourceType === 'KeyVault')
  );
  protected readonly selectedKeyVault = signal<AzureResourceResponse | null>(null);

  // ─── Key Vault access check ───
  protected readonly secretName = signal('');
  protected readonly kvAccessChecking = signal(false);
  protected readonly kvHasAccess = signal<boolean | null>(null);
  protected readonly kvMissingRoleName = signal<string | null>(null);

  // ─── Step 4 / Static — Name configuration ───
  protected readonly settingName = signal('');
  protected readonly environmentValues = signal<Record<string, string>>({});

  // ─── Submit state ───
  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');

  protected readonly suggestedName = computed(() => {
    if (this.mode() === 'static' && this.isSecret()) {
      const kv = this.selectedKeyVault();
      const secret = this.secretName();
      if (!kv || !secret) return '';
      const prefix = kv.name.toUpperCase().replace(/[^A-Z0-9]/g, '_');
      const suffix = secret.toUpperCase().replace(/[^A-Z0-9]/g, '_');
      return `${prefix}__${suffix}`;
    }
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
    const name = this.settingName().trim();
    if (!name || this.isSubmitting()) return false;
    if (this.mode() === 'static') {
      if (this.isViaVariableGroup()) {
        const hasVg = this.isCreatingNewGroup() ? this.newGroupName().trim().length > 0 : !!this.selectedVariableGroupId();
        return hasVg && this.pipelineVariableName().trim().length > 0;
      }
      if (this.isSecret()) {
        return !!this.selectedKeyVault() && !!this.secretName().trim() && !!this.secretAssignmentMode();
      }
      const vals = this.environmentValues();
      return Object.values(vals).some(v => v.trim().length > 0);
    }
    // output mode — final step is now step 4
    return !!this.selectedSource() && !!this.selectedOutput();
  });

  protected readonly canProceedFromSensitiveStep = computed(() => {
    const choice = this.sensitiveChoice();
    if (!choice) return false;
    if (choice === 'keyvault') return !!this.selectedKeyVault() && !!this.sensitiveSecretName().trim();
    if (choice === 'direct') return this.sensitiveDirectConfirmed();
    return false;
  });

  protected onModeChange(value: 'static' | 'output'): void {
    this.mode.set(value);
    this.step.set(1);
    this.selectedSource.set(null);
    this.selectedOutput.set(null);
    this.selectedKeyVault.set(null);
    this.secretName.set('');
    this.settingName.set('');
    this.environmentValues.set(
      Object.fromEntries(this.data.environments.map(e => [e.name, '']))
    );
    this.errorKey.set('');
    this.kvHasAccess.set(null);
    this.kvMissingRoleName.set(null);
    this.kvMissingRoleDefinitionId.set(null);
    this.sensitiveChoice.set(null);
    this.sensitiveDirectConfirmed.set(false);
    this.sensitiveSecretName.set('');
    this.sensitiveRoleAssigning.set(false);
    this.sensitiveRoleAssigned.set(false);
    this.sensitiveRoleError.set(false);
    this.isSecret.set(false);
    this.secretAssignmentMode.set(null);
    this.isViaVariableGroup.set(false);
    this.selectedVariableGroupId.set(null);
    this.newGroupName.set('');
    this.isCreatingNewGroup.set(false);
    this.pipelineVariableName.set('');
  }

  protected onIsSecretToggle(value: boolean): void {
    this.isSecret.set(value);
    if (!value) {
      this.selectedKeyVault.set(null);
      this.secretName.set('');
      this.secretAssignmentMode.set(null);
      this.kvHasAccess.set(null);
      this.kvMissingRoleName.set(null);
      this.kvMissingRoleDefinitionId.set(null);
      this.sensitiveRoleAssigning.set(false);
      this.sensitiveRoleAssigned.set(false);
      this.sensitiveRoleError.set(false);
    }
  }

  protected async onViaVariableGroupToggle(value: boolean): Promise<void> {
    this.isViaVariableGroup.set(value);
    if (value) {
      this.isSecret.set(false);
      await this.loadVariableGroups();
    } else {
      this.selectedVariableGroupId.set(null);
      this.newGroupName.set('');
      this.isCreatingNewGroup.set(false);
      this.pipelineVariableName.set('');
    }
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

  protected selectStaticKeyVault(resource: AzureResourceResponse): void {
    this.selectedKeyVault.set(resource);
    this.checkKeyVaultAccess();
  }

  protected selectSecretAssignmentMode(mode: 'ViaBicepparam' | 'DirectInKeyVault'): void {
    this.secretAssignmentMode.set(mode);
  }

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
      // Go to step 3 — sensitive security choice
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
      // Non-sensitive → go directly to step 4 (name)
      this.settingName.set(this.suggestedName());
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
    this.settingName.set(this.suggestedName());
    this.step.set(4);
  }

  protected async assignSensitiveRole(): Promise<void> {
    const kv = this.selectedKeyVault();
    const roleDefId = this.kvMissingRoleDefinitionId();
    if (!kv || !roleDefId) return;

    this.sensitiveRoleAssigning.set(true);
    this.sensitiveRoleError.set(false);

    try {
      await this.roleAssignmentService.add(this.data.resourceId, {
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

  protected async checkKeyVaultAccess(): Promise<void> {
    const kv = this.selectedKeyVault();
    if (!kv) return;

    this.kvAccessChecking.set(true);
    this.kvHasAccess.set(null);
    this.kvMissingRoleName.set(null);
    this.kvMissingRoleDefinitionId.set(null);

    try {
      const result = await this.appSettingService.checkKeyVaultAccess(this.data.resourceId, kv.id);
      this.kvHasAccess.set(result.hasAccess);
      this.kvMissingRoleName.set(result.missingRoleName ?? null);
      this.kvMissingRoleDefinitionId.set(result.missingRoleDefinitionId ?? null);
    } catch {
      this.kvHasAccess.set(null);
    } finally {
      this.kvAccessChecking.set(false);
    }
  }

  protected goBack(): void {
    const currentStep = this.step();
    if (this.mode() === 'output') {
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
    }
    this.errorKey.set('');
  }

  protected async onSubmit(): Promise<void> {
    const name = this.settingName().trim();
    if (!name || this.isSubmitting()) return;

    this.isSubmitting.set(true);
    this.errorKey.set('');

    try {
      // If creating a new variable group, create it first
      let variableGroupId = this.selectedVariableGroupId();
      if (this.mode() === 'static' && this.isViaVariableGroup() && this.isCreatingNewGroup()) {
        const newGroupName = this.newGroupName().trim();
        if (newGroupName && this.data.projectId) {
          const newGroup = await this.projectService.addPipelineVariableGroup(this.data.projectId, { groupName: newGroupName });
          variableGroupId = newGroup.id;
        }
      }

      let request;
      if (this.mode() === 'static') {
        if (this.isViaVariableGroup()) {
          request = {
            name,
            variableGroupId: variableGroupId ?? undefined,
            pipelineVariableName: this.pipelineVariableName().trim(),
            environmentValues: this.environmentValues(),
          };
        } else if (this.isSecret()) {
          request = {
            name,
            keyVaultResourceId: this.selectedKeyVault()!.id,
            secretName: this.secretName().trim(),
            secretValueAssignment: this.secretAssignmentMode(),
          };
        } else {
          request = { name, environmentValues: this.environmentValues() };
        }
      } else if (this.mode() === 'output') {
        if (this.sensitiveChoice() === 'keyvault') {
          request = {
            name,
            sourceResourceId: this.selectedSource()!.id,
            sourceOutputName: this.selectedOutput()!.name,
            keyVaultResourceId: this.selectedKeyVault()!.id,
            secretName: this.sensitiveSecretName().trim(),
            exportToKeyVault: true,
          };
        } else {
          request = {
            name,
            sourceResourceId: this.selectedSource()!.id,
            sourceOutputName: this.selectedOutput()!.name,
          };
        }
      } else {
        request = { name, environmentValues: this.environmentValues() };
      }

      const result = await this.appSettingService.add(this.data.resourceId, request);
      this.dialogRef.close(result);
    } catch {
      this.errorKey.set('RESOURCE_EDIT.ADD_APP_SETTING_DIALOG.ERROR');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected updateEnvironmentValue(envName: string, value: string): void {
    this.environmentValues.update(current => ({ ...current, [envName]: value }));
  }

  protected onCancel(): void {
    this.dialogRef.close(undefined);
  }

  private generateSecretName(): string {
    const source = this.selectedSource();
    const output = this.selectedOutput();
    if (!source || !output) return '';
    const prefix = source.name.toLowerCase().replace(/[^a-z0-9]/g, '-');
    const suffix = output.name.toLowerCase().replace(/[^a-z0-9]/g, '-');
    return `${prefix}-${suffix}`;
  }
}
