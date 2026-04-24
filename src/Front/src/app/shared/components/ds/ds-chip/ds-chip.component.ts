import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

/**
 * Design system chip / badge / tag. Variants for status communication, optional removable icon.
 */
@Component({
  selector: 'app-ds-chip',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './ds-chip.component.html',
  styleUrl: './ds-chip.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsChipComponent {
  public readonly variant = input<'neutral' | 'primary' | 'success' | 'warning' | 'error' | 'cyan'>(
    'neutral',
  );
  public readonly size = input<'sm' | 'md'>('md');
  public readonly icon = input<string | undefined>(undefined);
  public readonly removable = input<boolean>(false);

  public readonly removed = output<void>();

  protected onRemove(event: MouseEvent): void {
    event.stopPropagation();
    this.removed.emit();
  }
}
