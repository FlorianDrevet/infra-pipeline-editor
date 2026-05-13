import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';

import { ResourceEditCustomDomainsSection } from './resource-edit-custom-domains-section.interface';

@Component({
  selector: 'app-resource-edit-custom-domains-section',
  standalone: true,
  imports: [
    MatButtonModule,
    MatExpansionModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
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