import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { TranslateModule } from '@ngx-translate/core';

import { DsSpinnerSize } from './ds-spinner.types';

const SIZE_PX: Record<DsSpinnerSize, number> = {
  sm: 14,
  md: 18,
  lg: 24,
  xl: 40,
};

const DEFAULT_LABEL_KEY = 'DS.SPINNER.LOADING';

/**
 * Design system circular spinner. Indeterminate loading indicator built with
 * an SVG circle and a rotating stroke. Honors `prefers-reduced-motion`.
 *
 * Use `inline` to render inside a line of text (vertical-align: middle).
 */
@Component({
  selector: 'app-ds-spinner',
  standalone: true,
  imports: [TranslateModule],
  templateUrl: './ds-spinner.component.html',
  styleUrl: './ds-spinner.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsSpinnerComponent {
  public readonly size = input<DsSpinnerSize>('md');
  public readonly label = input<string | undefined>(undefined);
  public readonly inline = input<boolean>(false);

  protected readonly diameter = computed(() => SIZE_PX[this.size()]);
  protected readonly resolvedLabelKey = computed(() => this.label() ?? DEFAULT_LABEL_KEY);
  protected readonly hasCustomLabel = computed(() => this.label() !== undefined);
}
