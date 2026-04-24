import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

import { MatIconModule } from '@angular/material/icon';

/**
 * Design system circular icon button. Multiple variants and sizes.
 */
@Component({
  selector: 'app-ds-icon-button',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './ds-icon-button.component.html',
  styleUrl: './ds-icon-button.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsIconButtonComponent {
  public readonly icon = input.required<string>();
  public readonly variant = input<'ghost' | 'primary' | 'danger' | 'subtle'>('ghost');
  public readonly size = input<'sm' | 'md' | 'lg'>('md');
  public readonly disabled = input<boolean>(false);
  public readonly loading = input<boolean>(false);
  public readonly tooltip = input<string | undefined>(undefined);
  public readonly ariaLabel = input.required<string>();
  public readonly type = input<'button' | 'submit'>('button');

  public readonly clicked = output<MouseEvent>();

  protected readonly isDisabled = computed(() => this.disabled() || this.loading());

  protected onClick(event: MouseEvent): void {
    if (this.isDisabled()) {
      return;
    }
    this.clicked.emit(event);
  }
}
