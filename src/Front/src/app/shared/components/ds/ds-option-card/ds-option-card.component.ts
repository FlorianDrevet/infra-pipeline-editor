import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

/**
 * Design system selectable option card.
 * Displays a Material icon, title, and description with selection-state styling.
 * Wrap a group of these inside a `role="radiogroup"` container.
 */
@Component({
  selector: 'app-ds-option-card',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './ds-option-card.component.html',
  styleUrl: './ds-option-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsOptionCardComponent {
  public readonly icon = input.required<string>();
  public readonly title = input.required<string>();
  public readonly description = input.required<string>();
  public readonly selected = input<boolean>(false);
  public readonly disabled = input<boolean>(false);

  public readonly cardSelect = output<void>();

  protected onClick(): void {
    if (!this.disabled()) {
      this.cardSelect.emit();
    }
  }
}
