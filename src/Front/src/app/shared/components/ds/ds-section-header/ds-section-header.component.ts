import { ChangeDetectionStrategy, Component, input } from '@angular/core';

import { MatIconModule } from '@angular/material/icon';

/**
 * Design system section header. Title (required), optional icon/subtitle, action slot.
 *
 * V3 (UI refresh 2026-05): flat layout, no accent left bar, no gradient.
 * Optional `divider` input renders a 1px subtle bottom border.
 */
@Component({
  selector: 'app-ds-section-header',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './ds-section-header.component.html',
  styleUrl: './ds-section-header.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsSectionHeaderComponent {
  public readonly title = input.required<string>();
  public readonly icon = input<string | undefined>(undefined);
  public readonly subtitle = input<string | undefined>(undefined);
  public readonly level = input<1 | 2 | 3>(2);
  public readonly divider = input<boolean>(false);
}

