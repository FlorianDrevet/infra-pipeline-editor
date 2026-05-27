import { ChangeDetectionStrategy, Component, input, model } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

import { DsAccordionTone } from './ds-accordion.types';

/**
 * Design system accordion. Expandable/collapsible content panel with header, icon, and tone variants.
 */
@Component({
  selector: 'app-ds-accordion',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './ds-accordion.component.html',
  styleUrl: './ds-accordion.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsAccordionComponent {
  /** Header title text. */
  public readonly title = input.required<string>();

  /** Optional Material icon name displayed before the title. */
  public readonly icon = input<string | undefined>(undefined);

  /** Color tone for the icon and indicator. */
  public readonly tone = input<DsAccordionTone>('neutral');

  /** Two-way bindable expanded state. */
  public readonly expanded = model<boolean>(false);

  /** Disables toggle interaction. */
  public readonly disabled = input<boolean>(false);

  protected toggle(): void {
    if (this.disabled()) {
      return;
    }
    this.expanded.set(!this.expanded());
  }
}
