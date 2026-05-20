import { ChangeDetectionStrategy, Component, computed, effect, input, signal } from '@angular/core';
import { AbstractControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { DsSegmentedControlComponent, DsTextFieldComponent } from '../../../../shared/components/ds';
import { ResourceEditEnvironmentFormEntry } from '../../helpers/resource-edit-environment-settings.helpers';

const CONTAINER_REGISTRY_SERVICE_CONNECTION_CONTROL = 'containerRegistryServiceConnection';
const EMPTY_SERVICE_CONNECTION_NAME = '';
const CONTAINER_APP_ACR_SERVICE_CONNECTION_MODES = {
  shared: 'shared',
  perEnvironment: 'perEnvironment',
} as const;

type ContainerAppAcrServiceConnectionMode = typeof CONTAINER_APP_ACR_SERVICE_CONNECTION_MODES[keyof typeof CONTAINER_APP_ACR_SERVICE_CONNECTION_MODES];

@Component({
  selector: 'app-container-app-acr-service-connections',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    DsSegmentedControlComponent,
    DsTextFieldComponent,
  ],
  templateUrl: './container-app-acr-service-connections.component.html',
  styleUrl: './container-app-acr-service-connections.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContainerAppAcrServiceConnectionsComponent {
  public readonly envForms = input.required<ReadonlyArray<ResourceEditEnvironmentFormEntry>>();

  protected readonly serviceConnectionControlName = CONTAINER_REGISTRY_SERVICE_CONNECTION_CONTROL;
  protected readonly modeValues = CONTAINER_APP_ACR_SERVICE_CONNECTION_MODES;
  protected readonly mode = signal<ContainerAppAcrServiceConnectionMode>(CONTAINER_APP_ACR_SERVICE_CONNECTION_MODES.shared);
  protected readonly sharedServiceConnectionName = signal(EMPTY_SERVICE_CONNECTION_NAME);
  protected readonly isSharedMode = computed(() => this.mode() === CONTAINER_APP_ACR_SERVICE_CONNECTION_MODES.shared);

  private readonly syncEffect = effect(() => {
    const envForms = this.envForms();
    const inferredMode = this.inferMode(envForms);

    this.mode.set(inferredMode);
    this.sharedServiceConnectionName.set(this.resolveSharedServiceConnectionName(envForms));
  });

  protected onModeChanged(value: string): void {
    if (!this.isMode(value)) {
      return;
    }

    this.mode.set(value);
    if (value === CONTAINER_APP_ACR_SERVICE_CONNECTION_MODES.shared) {
      const sharedServiceConnectionName = this.resolveFirstNonEmptyServiceConnectionName(this.envForms());
      this.sharedServiceConnectionName.set(sharedServiceConnectionName);
      this.applySharedServiceConnectionNameToEnvironmentForms(sharedServiceConnectionName);
    }
  }

  protected onSharedServiceConnectionNameChanged(value: string): void {
    this.sharedServiceConnectionName.set(value);
    this.applySharedServiceConnectionNameToEnvironmentForms(value);
  }

  private applySharedServiceConnectionNameToEnvironmentForms(value: string): void {
    for (const envForm of this.envForms()) {
      const control = this.getServiceConnectionControl(envForm);
      if (!control) {
        continue;
      }

      control.setValue(value);
      control.markAsDirty();
      envForm.form.markAsDirty();
    }
  }

  private resolveFirstNonEmptyServiceConnectionName(envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>): string {
    return this.readNonEmptyServiceConnectionNames(envForms)[0] ?? EMPTY_SERVICE_CONNECTION_NAME;
  }

  private inferMode(envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>): ContainerAppAcrServiceConnectionMode {
    const serviceConnectionNames = this.readNonEmptyServiceConnectionNames(envForms);
    const uniqueServiceConnectionNames = new Set(serviceConnectionNames);

    return uniqueServiceConnectionNames.size <= 1
      ? CONTAINER_APP_ACR_SERVICE_CONNECTION_MODES.shared
      : CONTAINER_APP_ACR_SERVICE_CONNECTION_MODES.perEnvironment;
  }

  private resolveSharedServiceConnectionName(envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>): string {
    const serviceConnectionNames = this.readNonEmptyServiceConnectionNames(envForms);
    const uniqueServiceConnectionNames = new Set(serviceConnectionNames);

    return uniqueServiceConnectionNames.size === 1
      ? serviceConnectionNames[0]
      : EMPTY_SERVICE_CONNECTION_NAME;
  }

  private readNonEmptyServiceConnectionNames(envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>): string[] {
    return envForms
      .map((envForm) => this.getServiceConnectionControl(envForm)?.value ?? null)
      .filter((value): value is string => typeof value === 'string' && value !== EMPTY_SERVICE_CONNECTION_NAME);
  }

  private getServiceConnectionControl(envForm: ResourceEditEnvironmentFormEntry): AbstractControl<string | null> | null {
    return envForm.form.get(CONTAINER_REGISTRY_SERVICE_CONNECTION_CONTROL) as AbstractControl<string | null> | null;
  }

  private isMode(value: string): value is ContainerAppAcrServiceConnectionMode {
    return value === CONTAINER_APP_ACR_SERVICE_CONNECTION_MODES.shared
      || value === CONTAINER_APP_ACR_SERVICE_CONNECTION_MODES.perEnvironment;
  }
}