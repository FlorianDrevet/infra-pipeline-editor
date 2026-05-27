import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { DsProgressBarMode, DsProgressBarTone } from './ds-progress-bar.types';

const MIN_VALUE = 0;
const MAX_VALUE = 100;

/**
 * Design system linear progress bar. Supports `determinate` (driven by `value`)
 * and `indeterminate` (animated) modes. Indeterminate animation respects
 * `prefers-reduced-motion`.
 */
@Component({
  selector: 'app-ds-progress-bar',
  standalone: true,
  imports: [],
  templateUrl: './ds-progress-bar.component.html',
  styleUrl: './ds-progress-bar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsProgressBarComponent {
  public readonly mode = input<DsProgressBarMode>('determinate');
  public readonly value = input<number>(0);
  public readonly tone = input<DsProgressBarTone>('brand');
  public readonly label = input<string | undefined>(undefined);

  protected readonly clampedValue = computed(() => {
    const raw = this.value();
    if (Number.isNaN(raw)) {
      return MIN_VALUE;
    }
    return Math.min(MAX_VALUE, Math.max(MIN_VALUE, raw));
  });

  protected readonly isIndeterminate = computed(() => this.mode() === 'indeterminate');
  protected readonly fillWidth = computed(() => `${this.clampedValue()}%`);
  protected readonly ariaValueNow = computed(() =>
    this.isIndeterminate() ? null : this.clampedValue(),
  );
}
