import { Component, inject, signal, computed } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCheckboxModule } from '@angular/material/checkbox';

import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DsButtonComponent, DsSelectComponent, DsSelectOption, DsTextFieldComponent } from '../../../shared/components/ds';
import { AzureResourceResponse } from '../../../shared/interfaces/resource-group.interface';
import { AppSettingService } from '../../../shared/services/app-setting.service';
import { RoleAssignmentService } from '../../../shared/services/role-assignment.service';
import { ProjectService } from '../../../shared/services/project.service';
import { OutputDefinitionResponse } from '../../../shared/interfaces/app-setting.interface';
import { ProjectPipelineVariableGroupResponse } from '../../../shared/interfaces/project.interface';
import { RESOURCE_TYPE_ICONS } from '../../config-detail/enums/resource-type.enum';

export interface AddAppSettingDialogData {
  resourceId: string;
  currentResourceName: string;
  siblingResources: AzureResourceResponse[];
  environments: { name: string }[];
  projectId: string;
}

function toSettingNameSegment(value: string): string {
  return value.toUpperCase().replaceAll(/[^A-Z0-9]/g, '_');
}

function toSecretNameSegment(value: string): string {
  return value.toLowerCase().replaceAll(/[^a-z0-9]/g, '-');
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
    MatProgressSpinnerModule,
    MatRadioModule,
    MatTooltipModule,
    MatCheckboxModule,
    DsButtonComponent,
    DsSelectComponent,
    DsTextFieldComponent,
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
  private readonly translate = inject(TranslateService);

  protected readonly resourceTypeIcons = RESOURCE_TYPE_ICONS;

  // ─── Variable group select options (DS) ───
  private readonly createNewGroupLabel = this.translate.instant('RESOURCE_EDIT.ADD_APP_SETTING_DIALOG.CREATE_NEW_GROUP');
  protected readonly vgSelectOptions = computed<DsSelectOption[]>(() => [
    ...this.vgOptions().map((vg) => ({ value: vg.id, label: vg.groupName })),
    { value: '__create_new__', label: this.createNewGroupLabel, icon: 'add' },
  ]);
  protected readonly vgSelectValue = computed<string>(() =>
    this.selectedVariableGroupId() ?? (this.isCreatingNewGroup() ? '__create_new__' : ''),
  );

  // ─── Mode: static value or resource output ───
  protected readonly mode = signal<'static' | 'output'>('output');

  // ─── Static mode — Step-based flow ───
  protected readonly staticStep = signal<1 | 2>(1);
  protected readonly staticType = signal<'standard' | 'secret' | null>(null);
  protected readonly standardValueSource = signal<'environments' | 'variableGroup' | null>(null);
  protected readonly secretValueSource = signal<'viaBicepparam' | 'directInKeyVault' | 'variableGroup' | null>(null);

  // Derived from step choices (replaces old toggle signals)
  protected readonly isSecret = computed(() => this.staticType() === 'secret');
  protected readonly isViaVariableGroup = computed(() => {
    if (this.staticType() === 'standard') return this.standardValueSource() === 'variableGroup';
    if (this.staticType() === 'secret') return this.secretValueSource() === 'variableGroup';
    return false;
  });
  protected readonly secretAssignmentMode = computed<'ViaBicepparam' | 'DirectInKeyVault' | null>(() => {
    const src = this.secretValueSource();
    if (src === 'viaBicepparam' || src === 'variableGroup') return 'ViaBicepparam';
    if (src === 'directInKeyVault') return 'DirectInKeyVault';
    return null;
  });

  // ─── Variable Group (static mode only) ───
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
      const prefix = toSettingNameSegment(kv.name);
      const suffix = toSettingNameSegment(secret);
      return `${prefix}__${suffix}`;
    }
    if (this.mode() === 'output' && this.sensitiveChoice() === 'keyvault') {
      const kv = this.selectedKeyVault();
      const secret = this.sensitiveSecretName();
      if (!kv || !secret) return '';
      const prefix = toSettingNameSegment(kv.name);
      const suffix = toSettingNameSegment(secret);
      return `${prefix}__${suffix}`;
    }
    const source = this.selectedSource();
    const output = this.selectedOutput();
    if (!source || !output) return '';
    const prefix = toSettingNameSegment(source.name);
    const suffix = toSettingNameSegment(output.name);
    return `${prefix}__${suffix}`;
  });

  protected readonly canSubmit = computed(() => {
    const name = this.settingName().trim();
    if (!name || this.isSubmitting()) return false;

    if (this.mode() === 'static') {
      return this.canSubmitStaticMode();
    }

    return this.canSubmitOutputMode();
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
    this.staticStep.set(1);
    this.staticType.set(null);
    this.standardValueSource.set(null);
    this.secretValueSource.set(null);
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
    this.selectedVariableGroupId.set(null);
    this.newGroupName.set('');
    this.isCreatingNewGroup.set(false);
    this.pipelineVariableName.set('');
  }

  // ─── Static step-based flow ───

  protected async selectStaticType(type: 'standard' | 'secret'): Promise<void> {
    this.staticType.set(type);
    this.staticStep.set(2);
    this.standardValueSource.set(null);
    this.secretValueSource.set(null);
    this.selectedKeyVault.set(null);
    this.secretName.set('');
    this.kvHasAccess.set(null);
    this.kvMissingRoleName.set(null);
    this.kvMissingRoleDefinitionId.set(null);
    this.sensitiveRoleAssigning.set(false);
    this.sensitiveRoleAssigned.set(false);
    this.sensitiveRoleError.set(false);
    this.selectedVariableGroupId.set(null);
    this.newGroupName.set('');
    this.isCreatingNewGroup.set(false);
    this.pipelineVariableName.set('');
  }

  protected async selectStandardValueSource(source: 'environments' | 'variableGroup'): Promise<void> {
    this.standardValueSource.set(source);
    if (source === 'variableGroup') {
      await this.loadVariableGroups();
    }
  }

  protected async selectSecretValueSource(source: 'viaBicepparam' | 'directInKeyVault' | 'variableGroup'): Promise<void> {
    this.secretValueSource.set(source);
    if (source === 'variableGroup') {
      await this.loadVariableGroups();
    }
  }

  protected goBackStatic(): void {
    const currentStep = this.staticStep();
    if (currentStep === 2) {
      this.staticStep.set(1);
      this.staticType.set(null);
      this.standardValueSource.set(null);
      this.secretValueSource.set(null);
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

  protected selectStaticKeyVault(resource: AzureResourceResponse): void {
    this.selectedKeyVault.set(resource);
    this.checkKeyVaultAccess();
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
      if (currentStep === 4) { // NOSONAR S3776 - tracked under test-debt #22
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
      const variableGroupId = await this.resolveVariableGroupId();
      const request = this.buildSubmitRequest(name, variableGroupId);
      const result = await this.appSettingService.add(this.data.resourceId, request);
      this.dialogRef.close(result);
    } catch (err: unknown) {
      await this.setSubmitError(err);
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected updateEnvironmentValue(envName: string, value: string): void {
    this.environmentValues.update(current => ({ ...current, [envName]: value }));
  }

  protected onCancel(): void {
    this.dialogRef.close();
  }

  private canSubmitStaticMode(): boolean {
    if (this.staticType() === 'standard') {
      return this.canSubmitStaticStandardMode();
    }

    if (this.staticType() === 'secret') {
      return this.canSubmitStaticSecretMode();
    }

    return false;
  }

  private canSubmitStaticStandardMode(): boolean {
    if (this.standardValueSource() === 'variableGroup') {
      return this.hasVariableGroupTarget() && this.hasPipelineVariableName();
    }

    if (this.standardValueSource() === 'environments') {
      return this.hasEnvironmentValues();
    }

    return false;
  }

  private canSubmitStaticSecretMode(): boolean {
    if (!this.hasSecretTarget()) {
      return false;
    }

    if (this.secretValueSource() === 'variableGroup') {
      return this.hasVariableGroupTarget() && this.hasPipelineVariableName();
    }

    return this.secretValueSource() === 'viaBicepparam' || this.secretValueSource() === 'directInKeyVault';
  }

  private canSubmitOutputMode(): boolean {
    return this.selectedSource() !== null && this.selectedOutput() !== null;
  }

  private hasVariableGroupTarget(): boolean {
    if (this.isCreatingNewGroup()) {
      return this.newGroupName().trim().length > 0;
    }

    return this.selectedVariableGroupId() !== null;
  }

  private hasPipelineVariableName(): boolean {
    return this.pipelineVariableName().trim().length > 0;
  }

  private hasEnvironmentValues(): boolean {
    return Object.values(this.environmentValues()).some(value => value.trim().length > 0);
  }

  private hasSecretTarget(): boolean {
    return this.selectedKeyVault() !== null && this.secretName().trim().length > 0;
  }

  private async resolveVariableGroupId(): Promise<string | null> {
    const currentVariableGroupId = this.selectedVariableGroupId();
    if (this.mode() !== 'static' || !this.isViaVariableGroup() || !this.isCreatingNewGroup()) {
      return currentVariableGroupId;
    }

    const newGroupName = this.newGroupName().trim();
    if (!newGroupName || !this.data.projectId) {
      return currentVariableGroupId;
    }

    const newGroup = await this.projectService.addPipelineVariableGroup(this.data.projectId, { groupName: newGroupName });
    return newGroup.id;
  }

  private buildSubmitRequest(name: string, variableGroupId: string | null) {
    if (this.mode() === 'static') {
      return this.buildStaticRequest(name, variableGroupId);
    }

    return this.buildOutputRequest(name);
  }

  private buildStaticRequest(name: string, variableGroupId: string | null) {
    if (this.isViaVariableGroup() && this.isSecret()) {
      const selectedKeyVault = this.selectedKeyVault();

      return {
        name,
        variableGroupId: variableGroupId ?? undefined,
        pipelineVariableName: this.pipelineVariableName().trim(),
        keyVaultResourceId: selectedKeyVault?.id,
        secretName: this.secretName().trim(),
        secretValueAssignment: this.secretAssignmentMode(),
      };
    }

    if (this.isViaVariableGroup()) {
      return {
        name,
        variableGroupId: variableGroupId ?? undefined,
        pipelineVariableName: this.pipelineVariableName().trim(),
      };
    }

    if (this.isSecret()) {
      const selectedKeyVault = this.selectedKeyVault();

      return {
        name,
        keyVaultResourceId: selectedKeyVault?.id,
        secretName: this.secretName().trim(),
        secretValueAssignment: this.secretAssignmentMode(),
      };
    }

    return {
      name,
      environmentValues: this.environmentValues(),
    };
  }

  private buildOutputRequest(name: string) {
    const sourceResourceId = this.selectedSource()?.id;
    const sourceOutputName = this.selectedOutput()?.name;

    if (this.sensitiveChoice() === 'keyvault') {
      return {
        name,
        sourceResourceId,
        sourceOutputName,
        keyVaultResourceId: this.selectedKeyVault()?.id,
        secretName: this.sensitiveSecretName().trim(),
        exportToKeyVault: true,
      };
    }

    return {
      name,
      sourceResourceId,
      sourceOutputName,
    };
  }

  private async setSubmitError(err: unknown): Promise<void> {
    const axios = await import('axios');
    if (axios.isAxiosError(err) && err.response?.status === 409) {
      this.errorKey.set('RESOURCE_EDIT.ADD_APP_SETTING_DIALOG.ERROR_DUPLICATE');
      return;
    }

    this.errorKey.set('RESOURCE_EDIT.ADD_APP_SETTING_DIALOG.ERROR');
  }

  private generateSecretName(): string {
    const source = this.selectedSource();
    const output = this.selectedOutput();
    if (!source || !output) return '';
    const prefix = toSecretNameSegment(source.name);
    const suffix = toSecretNameSegment(output.name);
    return `${prefix}-${suffix}`;
  }
}
