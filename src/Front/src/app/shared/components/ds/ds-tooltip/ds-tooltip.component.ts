import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * Position of {@link DsTooltipDirective} overlay relative to its host.
 */
export type DsTooltipPosition = 'top' | 'bottom' | 'left' | 'right';

/**
 * Internal overlay component rendered by {@link DsTooltipDirective}.
 * Not consumed directly — use the `[appDsTooltip]` directive instead.
 */
@Component({
  selector: 'app-ds-tooltip',
  standalone: true,
  imports: [],
  templateUrl: './ds-tooltip.component.html',
  styleUrl: './ds-tooltip.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsTooltipComponent {
  public readonly text = input.required<string>();
  public readonly position = input<DsTooltipPosition>('top');
}
