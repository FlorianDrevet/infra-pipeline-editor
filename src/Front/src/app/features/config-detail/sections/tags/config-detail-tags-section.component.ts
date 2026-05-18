import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';

import { DsButtonComponent, DsTextFieldComponent } from '../../../../shared/components/ds';
import { ConfigDetailTagsSection } from './config-detail-tags-section.interface';

@Component({
  selector: 'app-config-detail-tags-section',
  standalone: true,
  imports: [DsButtonComponent, DsTextFieldComponent, MatIconModule, MatTooltipModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './config-detail-tags-section.component.html',
  styleUrl: './config-detail-tags-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfigDetailTagsSectionComponent {
  readonly canWrite = input.required<boolean>();
  readonly section = input.required<ConfigDetailTagsSection>();
}