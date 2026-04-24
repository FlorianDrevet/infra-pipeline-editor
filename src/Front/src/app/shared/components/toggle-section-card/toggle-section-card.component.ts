import { Component, input, output } from '@angular/core';

import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-toggle-section-card',
  standalone: true,
  imports: [MatIconModule, MatSlideToggleModule, TranslateModule],
  templateUrl: './toggle-section-card.component.html',
  styleUrl: './toggle-section-card.component.scss',
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
