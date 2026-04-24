import { Component, computed, inject, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CompactSelectComponent, CompactSelectOption } from '../compact-select/compact-select.component';
import { AcrAuthMode } from '../../interfaces/container-registry.interface';
import {
  DsSelectComponent,
  DsSelectOption,
  DsTextFieldComponent,
} from '../ds';

export type DeploymentConfigMode = 'code-or-container' | 'container-only';
export type DeploymentModeValue = 'Code' | 'Container';
export type AcrUaiStateValue = 'idle' | 'checking' | 'ok' | 'uai-missing-role' | 'no-uai';

@Component({
  selector: 'app-deployment-config',
  standalone: true,
  imports: [
    FormsModule,
    MatIconModule,
    MatProgressSpinnerModule,
    TranslateModule,
    CompactSelectComponent,
    DsSelectComponent,
    DsTextFieldComponent,
],
  templateUrl: './deployment-config.component.html',
  styleUrl: './deployment-config.component.scss',
})
export class DeploymentConfigComponent {
  // ─── Inputs ───
  readonly mode = input<DeploymentConfigMode>('code-or-container');
  readonly deploymentMode = input<DeploymentModeValue>('Code');
  readonly containerRegistryId = input<string | null>(null);
  readonly acrAuthMode = input<AcrAuthMode | null>(null);
  readonly dockerImageName = input<string | null>(null);
  readonly showDockerImageName = input(true);
  readonly runtimeStack = input('');
  readonly runtimeVersion = input('');
  readonly runtimeStackOptions = input<{ label: string; value: string }[]>([]);
  readonly runtimeVersionOptions = input<string[]>([]);
  readonly availableContainerRegistries = input<{ id: string; name: string }[]>([]);
  readonly acrUaiState = input<AcrUaiStateValue>('idle');
  readonly acrAssignedUaiName = input<string | null>(null);
  readonly acrSelectedUaiId = input<string | null>(null);
  readonly acrRoleAssigning = input(false);
  readonly uaiOptions = input<CompactSelectOption[]>([]);
  readonly canWrite = input(true);
  readonly disabled = input(false);

  // ─── Outputs ───
  readonly deploymentModeChange = output<DeploymentModeValue>();
  readonly runtimeStackChange = output<string>();
  readonly runtimeVersionChange = output<string>();
  readonly containerRegistryChange = output<string | null>();
  readonly acrAuthModeChange = output<AcrAuthMode>();
  readonly dockerImageNameChange = output<string>();
  readonly acrSelectedUaiIdChange = output<string | null>();
  readonly addAcrPullRole = output<void>();
  readonly createUai = output<void>();

  // ─── DS Select options (computed) ───
  private readonly translate = inject(TranslateService);
  private readonly acrNoneLabel = this.translate.instant('RESOURCE_EDIT.FIELDS.ACR_NONE');

  protected readonly runtimeStackDsOptions = computed<DsSelectOption[]>(() =>
    this.runtimeStackOptions().map((o) => ({ value: o.value, label: o.label })),
  );

  protected readonly runtimeVersionDsOptions = computed<DsSelectOption[]>(() =>
    this.runtimeVersionOptions().map((v) => ({ value: v, label: v })),
  );

  protected readonly containerRegistryDsOptions = computed<DsSelectOption[]>(() => [
    { value: null, label: this.acrNoneLabel },
    ...this.availableContainerRegistries().map((acr) => ({ value: acr.id, label: acr.name })),
  ]);

  protected get isContainerMode(): boolean {
    return this.deploymentMode() === 'Container';
  }

  protected get hasContainerRegistrySelected(): boolean {
    return !!this.containerRegistryId();
  }

  protected get resolvedAcrAuthMode(): AcrAuthMode {
    return this.acrAuthMode() ?? 'ManagedIdentity';
  }

  protected get isManagedIdentityAcrMode(): boolean {
    return this.resolvedAcrAuthMode === 'ManagedIdentity';
  }

  protected get isAdminCredentialsAcrMode(): boolean {
    return this.resolvedAcrAuthMode === 'AdminCredentials';
  }

  protected showWhyUai = false;
}
