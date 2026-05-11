import { ChangeDetectionStrategy, Component, HostBinding, computed, input, output } from '@angular/core';

import { MatIconModule } from '@angular/material/icon';

import { DsButtonIconPosition, DsButtonSize, DsButtonType, DsButtonVariant } from './ds-button.types';

/**
 * Design system button. Variants, sizes, optional icon and loading state.
 */
@Component({
  selector: 'app-ds-button',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './ds-button.component.html',
  styleUrl: './ds-button.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsButtonComponent {
  public readonly variant = input<DsButtonVariant>('primary');
  public readonly size = input<DsButtonSize>('md');
  public readonly disabled = input<boolean>(false);
  public readonly loading = input<boolean>(false);
  public readonly icon = input<string | undefined>(undefined);
  public readonly iconPosition = input<DsButtonIconPosition>('leading');
  public readonly type = input<DsButtonType>('button');
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
