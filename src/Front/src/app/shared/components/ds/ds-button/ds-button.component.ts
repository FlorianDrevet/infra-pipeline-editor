import { ChangeDetectionStrategy, Component, HostBinding, computed, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

/**
 * Design system button. Variants, sizes, optional icon and loading state.
 */
@Component({
  selector: 'app-ds-button',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './ds-button.component.html',
  styleUrl: './ds-button.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsButtonComponent {
  public readonly variant = input<'primary' | 'secondary' | 'ghost' | 'danger'>('primary');
  public readonly size = input<'sm' | 'md' | 'lg'>('md');
  public readonly disabled = input<boolean>(false);
  public readonly loading = input<boolean>(false);
  public readonly icon = input<string | undefined>(undefined);
  public readonly iconPosition = input<'leading' | 'trailing'>('leading');
  public readonly type = input<'button' | 'submit'>('button');
  public readonly fullWidth = input<boolean>(false);

  public readonly clicked = output<MouseEvent>();

  protected readonly isDisabled = computed(() => this.disabled() || this.loading());
  protected readonly classes = computed(() => `ds-btn ds-btn--${this.variant()} ds-btn--${this.size()}`);

  @HostBinding('class.full-width')
  protected get hostFullWidth(): boolean {
    return this.fullWidth();
  }

  protected onClick(event: MouseEvent): void {
    if (this.isDisabled()) {
      return;
    }
    this.clicked.emit(event);
  }
}
