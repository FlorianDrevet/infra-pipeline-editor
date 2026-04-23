import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Design system card. Supports elevated/outlined/glass variants, accent borders and projection slots.
 */
@Component({
  selector: 'app-ds-card',
  standalone: true,
  imports: [CommonModule],
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

  protected readonly classes = computed(() =>
    [
      'ds-card',
      `ds-card--${this.variant()}`,
      `ds-card--padding-${this.padding()}`,
      this.accent() !== 'none' ? `ds-card--accent-${this.accent()}` : '',
      this.interactive() ? 'interactive' : '',
    ]
      .filter(Boolean)
      .join(' '),
  );

  protected onClick(event: MouseEvent): void {
    if (!this.interactive()) {
      return;
    }
    this.cardClick.emit(event);
  }
}
