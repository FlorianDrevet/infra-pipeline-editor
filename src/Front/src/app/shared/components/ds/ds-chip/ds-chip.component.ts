import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

import { MatIconModule } from '@angular/material/icon';

import { DsChipInputVariant, DsChipSize } from './ds-chip.types';

/**
 * Design system chip / badge / tag. Variants for status communication, optional removable icon.
 *
 * Legacy variants (`primary`, `error`, `cyan`) are remapped to V2 variants
 * for backward compatibility.
 */
@Component({
  selector: 'app-ds-chip',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './ds-chip.component.html',
  styleUrl: './ds-chip.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsChipComponent {
  public readonly variant = input<DsChipInputVariant>('neutral');
  public readonly size = input<DsChipSize>('md');
  public readonly icon = input<string | undefined>(undefined);
  public readonly removable = input<boolean>(false);

  public readonly removed = output<void>();

  protected readonly resolvedVariant = computed<'neutral' | 'success' | 'warning' | 'danger' | 'accent' | 'info'>(() => {
    const variant = this.variant();

    switch (variant) {
      case 'primary':
        return 'info';
      case 'error':
        return 'danger';
      case 'cyan':
        return 'accent';
      default:
        return variant;
    }
  });

  protected onRemove(event: MouseEvent): void {
    event.stopPropagation();
    this.removed.emit();
  }
}
