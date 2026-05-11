import { ChangeDetectionStrategy, Component, input } from '@angular/core';

import { DsStatusDotSize, DsStatusDotVariant } from './ds-status-dot.types';

/**
 * Design system status indicator dot. Small colored circle used inline next to
 * a label to denote semantic state. When `pulse` is true an attenuated
 * opacity animation hints at a live/streaming state.
 */
@Component({
  selector: 'app-ds-status-dot',
  standalone: true,
  imports: [],
  templateUrl: './ds-status-dot.component.html',
  styleUrl: './ds-status-dot.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsStatusDotComponent {
  public readonly variant = input<DsStatusDotVariant>('idle');
  public readonly size = input<DsStatusDotSize>('sm');
  public readonly ariaLabel = input<string | undefined>(undefined);
  public readonly pulse = input<boolean>(false);
}
