import { ChangeDetectionStrategy, Component, input } from '@angular/core';

import { MatIconModule } from '@angular/material/icon';

/**
 * Design system empty state. Displays a centered icon, title, description, and
 * an optional `[actions]` slot. Used wherever a list, table, or panel has no
 * content to display.
 */
@Component({
  selector: 'app-ds-empty-state',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './ds-empty-state.component.html',
  styleUrl: './ds-empty-state.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsEmptyStateComponent {
  public readonly icon = input<string | undefined>(undefined);
  public readonly title = input<string | undefined>(undefined);
  public readonly description = input<string | undefined>(undefined);
}
