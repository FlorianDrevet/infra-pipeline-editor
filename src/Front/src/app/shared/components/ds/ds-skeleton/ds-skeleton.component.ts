import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { DsSkeletonVariant } from './ds-skeleton.types';

interface SkeletonItemStyle {
  width: string;
  height: string;
}

const DEFAULT_DIMENSIONS: Record<DsSkeletonVariant, SkeletonItemStyle> = {
  box: { width: '100%', height: '80px' },
  line: { width: '100%', height: '12px' },
  circle: { width: '40px', height: '40px' },
};

/**
 * Design system loading placeholder. Renders a shimmering block sized via
 * `width` / `height`, repeated `count` times with `gap` spacing between items.
 */
@Component({
  selector: 'app-ds-skeleton',
  standalone: true,
  imports: [],
  templateUrl: './ds-skeleton.component.html',
  styleUrl: './ds-skeleton.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsSkeletonComponent {
  public readonly variant = input<DsSkeletonVariant>('line');
  public readonly width = input<string | undefined>(undefined);
  public readonly height = input<string | undefined>(undefined);
  public readonly count = input<number>(1);
  public readonly gap = input<string>('8px');

  protected readonly items = computed(() => Array.from({ length: Math.max(1, this.count()) }));

  protected readonly itemStyle = computed<SkeletonItemStyle>(() => {
    const defaults = DEFAULT_DIMENSIONS[this.variant()];
    return {
      width: this.width() ?? defaults.width,
      height: this.height() ?? defaults.height,
    };
  });
}
