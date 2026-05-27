import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { DsTextFieldComponent } from '../../../../shared/components/ds';
import { ResourceEditEnvironmentFormEntry } from '../../helpers/resource-edit-environment-settings.helpers';

const CONTAINER_REGISTRY_SERVICE_CONNECTION_CONTROL = 'containerRegistryServiceConnection';

@Component({
  selector: 'app-container-app-acr-service-connections',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TranslateModule,
    DsTextFieldComponent,
  ],
  templateUrl: './container-app-acr-service-connections.component.html',
  styleUrl: './container-app-acr-service-connections.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContainerAppAcrServiceConnectionsComponent {
  public readonly envForms = input.required<ReadonlyArray<ResourceEditEnvironmentFormEntry>>();

  protected readonly serviceConnectionControlName = CONTAINER_REGISTRY_SERVICE_CONNECTION_CONTROL;
}