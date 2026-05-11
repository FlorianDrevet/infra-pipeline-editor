import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

import { DsCardInputAccent, DsCardInputVariant, DsCardPadding } from './ds-card.types';

/**
 * Design system card. V2 supports `default`, `interactive` and `outlined`
 * variants on a flat surface. Legacy `elevated` and `glass` are accepted but
 * silently rendered as `default` for backward compatibility.
 */
@Component({
  selector: 'app-ds-card',
  standalone: true,
  imports: [],
  templateUrl: './ds-card.component.html',
  styleUrl: './ds-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsCardComponent {
  public readonly variant = input<DsCardInputVariant>('outlined');
  public readonly padding = input<DsCardPadding>('md');
  public readonly interactive = input<boolean>(false);
  /**
   * @deprecated Border-left coloured accents are removed in V2. The input is
   * still accepted to preserve the public API but has no visual effect.
   */
  public readonly accent = input<DsCardInputAccent>('none');

  public readonly cardClick = output<MouseEvent>();

  protected readonly resolvedVariant = computed<'default' | 'interactive' | 'outlined'>(() => {
    const v = this.variant();
    if (v === 'outlined') {
      return 'outlined';
    }
    if (v === 'interactive') {
      return 'interactive';
    }
    // default | elevated (deprecated) | glass (deprecated)
    return 'default';
  });

  protected readonly classes = computed(() => {
    const interactive = this.interactive() || this.resolvedVariant() === 'interactive';
    return [
      'ds-card',
      `ds-card--${this.resolvedVariant()}`,
      `ds-card--padding-${this.padding()}`,
      interactive ? 'interactive' : '',
    ]
      .filter(Boolean)
      .join(' ');
  });

  protected onClick(event: MouseEvent): void {
    if (!this.isClickable()) {
      return;
    }
    this.cardClick.emit(event);
  }

  protected onKeydown(event: Event): void {
    if (!this.isClickable()) {
      return;
    }

    event.preventDefault();
    (event.currentTarget as HTMLElement | null)?.click();
  }

  protected isClickable(): boolean {
    return this.interactive() || this.resolvedVariant() === 'interactive';
  }
}
