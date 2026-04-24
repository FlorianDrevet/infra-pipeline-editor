import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';

import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';

import { DsToggleComponent } from '../ds';

@Component({
  selector: 'app-toggle-section-card',
  standalone: true,
  imports: [FormsModule, MatIconModule, DsToggleComponent],
  templateUrl: './toggle-section-card.component.html',
  styleUrl: './toggle-section-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToggleSectionCardComponent {
  icon = input.required<string>();
  title = input.required<string>();
  subtitle = input<string>('');
  enabled = input(false);
  disabled = input(false);
  accentColor = input('#1565c0');

  enabledChange = output<boolean>();
}
