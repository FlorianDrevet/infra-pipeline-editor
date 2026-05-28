import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';

import { DsButtonComponent, DsIconButtonComponent, DsKeyValueInputComponent } from '../../../../shared/components/ds';
import { ConfigDetailTagsSection } from './config-detail-tags-section.interface';

@Component({
  selector: 'app-config-detail-tags-section',
  standalone: true,
  imports: [DsButtonComponent, DsIconButtonComponent, DsKeyValueInputComponent, FormsModule, MatIconModule, TranslateModule],
  templateUrl: './config-detail-tags-section.component.html',
  styleUrl: './config-detail-tags-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfigDetailTagsSectionComponent {
  readonly canWrite = input.required<boolean>();
  readonly section = input.required<ConfigDetailTagsSection>();
}