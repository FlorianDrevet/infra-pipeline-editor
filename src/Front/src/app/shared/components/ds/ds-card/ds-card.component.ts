import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';


/**
 * Design system card. Supports elevated/outlined/glass variants, accent borders and projection slots.
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
  public readonly variant = input<'elevated' | 'outlined' | 'glass'>('outlined');
  public readonly padding = input<'sm' | 'md' | 'lg' | 'none'>('md');
  public readonly interactive = input<boolean>(false);
  public readonly accent = input<'none' | 'primary' | 'success' | 'warning' | 'error'>('none');

  public readonly cardClick = output<MouseEvent>();

  protected readonly classes = computed(() => {
    const accent = this.accent();
    const accentClass = accent === 'none' ? '' : `ds-card--accent-${accent}`;

    return [
      'ds-card',
      `ds-card--${this.variant()}`,
      `ds-card--padding-${this.padding()}`,
      accentClass,
      this.interactive() ? 'interactive' : '',
    ]
      .filter(Boolean)
      .join(' ');
  });

  protected onClick(event: MouseEvent): void {
    if (!this.interactive()) {
      return;
    }
    this.cardClick.emit(event);
  }

  protected onKeydown(event: Event): void {
    if (!this.interactive()) {
      return;
    }

    event.preventDefault();
    (event.currentTarget as HTMLElement | null)?.click();
  }
}
