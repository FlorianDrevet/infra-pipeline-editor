import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';

import { DsIconButtonComponent } from '../ds-icon-button/ds-icon-button.component';
import { DsPropertyHelpDialogComponent } from './ds-property-help-dialog.component';
import { DsPropertyHelpSection } from './ds-property-help-button.types';

/**
 * Small icon button that opens a structured help dialog for a property.
 * Place it next to any label or section header that needs contextual explanation.
 */
@Component({
  selector: 'app-ds-property-help-button',
  standalone: true,
  imports: [DsIconButtonComponent],
  templateUrl: './ds-property-help-button.component.html',
  styleUrl: './ds-property-help-button.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsPropertyHelpButtonComponent {
  public readonly title = input.required<string>();
  public readonly sections = input.required<DsPropertyHelpSection[]>();

  private readonly dialog = inject(MatDialog);

  protected openHelp(): void {
    this.dialog.open(DsPropertyHelpDialogComponent, {
      data: { title: this.title(), sections: this.sections() },
      width: '640px',
      autoFocus: false,
    });
  }
}
