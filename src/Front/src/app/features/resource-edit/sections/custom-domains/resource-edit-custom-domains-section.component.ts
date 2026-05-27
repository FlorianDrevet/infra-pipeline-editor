import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { DsSpinnerComponent } from '../../../../shared/components/ds/ds-spinner/ds-spinner.component';
import { DsButtonComponent } from '../../../../shared/components/ds/ds-button/ds-button.component';
import { DsIconButtonComponent } from '../../../../shared/components/ds/ds-icon-button/ds-icon-button.component';
import { TranslateModule } from '@ngx-translate/core';

import { ResourceEditCustomDomainsSection } from './resource-edit-custom-domains-section.interface';

@Component({
  selector: 'app-resource-edit-custom-domains-section',
  standalone: true,
  imports: [
    MatExpansionModule,
    MatIconModule,
    DsSpinnerComponent,
    DsButtonComponent,
    DsIconButtonComponent,
    TranslateModule,
  ],
  templateUrl: './resource-edit-custom-domains-section.component.html',
  styleUrl: './resource-edit-custom-domains-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceEditCustomDomainsSectionComponent {
  readonly environmentName = input.required<string>();
  readonly section = input.required<ResourceEditCustomDomainsSection>();
}