import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-ds-panel-action-button',
  standalone: true,
  imports: [MatIconModule, MatTooltipModule],
  templateUrl: './ds-panel-action-button.component.html',
  styleUrl: './ds-panel-action-button.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsPanelActionButtonComponent {
  public readonly icon = input.required<string>();
  public readonly ariaLabel = input.required<string>();
  public readonly tooltip = input<string | undefined>(undefined);
  public readonly tone = input<'neutral' | 'accent' | 'danger'>('neutral');
  public readonly surface = input<'light' | 'dark'>('light');
  public readonly pressed = input<boolean | null>(null);
  public readonly ariaExpanded = input<boolean | null>(null);
  public readonly type = input<'button' | 'submit'>('button');

  public readonly clicked = output<MouseEvent>();

  protected onClick(event: MouseEvent): void {
    this.clicked.emit(event);
  }
}