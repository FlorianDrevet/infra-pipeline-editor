import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

/**
 * Design system page header. Hero-style banner with title (required), optional icon, subtitle and actions slot.
 */
@Component({
  selector: 'app-ds-page-header',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './ds-page-header.component.html',
  styleUrl: './ds-page-header.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsPageHeaderComponent {
  public readonly title = input.required<string>();
  public readonly subtitle = input<string | undefined>(undefined);
  public readonly icon = input<string | undefined>(undefined);
  public readonly variant = input<'gradient' | 'plain'>('gradient');
}
