import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

import { MatIconModule } from '@angular/material/icon';

import { DsIconButtonSize, DsIconButtonType, DsIconButtonVariant } from './ds-icon-button.types';

/**
 * Design system square icon button. V2 variants: `neutral`, `accent`, `danger`.
 *
 * Legacy variants `ghost`, `primary`, `subtle` are accepted and remapped for
 * backward compatibility (`ghost`→`neutral`, `primary`→`accent`,
 * `subtle`→`neutral` filled).
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
  public readonly variant = input<DsIconButtonVariant>('neutral');
  public readonly size = input<DsIconButtonSize>('md');
  public readonly disabled = input<boolean>(false);
  public readonly loading = input<boolean>(false);
  public readonly tooltip = input<string | undefined>(undefined);
  public readonly ariaLabel = input.required<string>();
  public readonly type = input<DsIconButtonType>('button');

  public readonly clicked = output<MouseEvent>();

  protected readonly isDisabled = computed(() => this.disabled() || this.loading());

  protected readonly resolvedVariant = computed<'neutral' | 'accent' | 'danger' | 'subtle'>(() => {
    switch (this.variant()) {
      case 'ghost':
        return 'neutral';
      case 'primary':
        return 'accent';
      case 'subtle':
        return 'subtle';
      default:
        return this.variant() as 'neutral' | 'accent' | 'danger';
    }
  });

  protected onClick(event: MouseEvent): void {
    if (this.isDisabled()) {
      return;
    }
    this.clicked.emit(event);
  }
}

